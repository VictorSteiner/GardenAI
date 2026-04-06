---
applyTo: "GardenAI.Domain/Common/**/*.cs,GardenAI.Application/**/*.cs,GardenAI.Presentation/Program.cs,GardenAI.Presentation/Endpoints/**/*.cs"
---

# CQRS Instructions

## Command Query Responsibility Segregation

Separate read operations (queries) from write operations (commands) for clarity and scalability.

---

## Commands (Write Operations)

### Pattern
```csharp
// 1. Define marker interface (Domain/Common)
public interface ICommand { }

// 2. Define command class (Application/Feature/Commands)
public sealed record CreatePlantPotCommand(string Label, Guid SpeciesId) : ICommand;

// 3. Define handler interface (Domain/Common)
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}

// 4. Implement handler (Application/Feature/Commands)
public sealed class CreatePlantPotCommandHandler : ICommandHandler<CreatePlantPotCommand>
{
    private readonly IPlantPotRepository _repo;
    private readonly ILogger<CreatePlantPotCommandHandler> _logger;
    
    public CreatePlantPotCommandHandler(IPlantPotRepository repo, ILogger<CreatePlantPotCommandHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }
    
    public async Task HandleAsync(CreatePlantPotCommand cmd, CancellationToken ct)
    {
        var pot = new PlantPot(Guid.NewGuid(), cmd.Label, cmd.SpeciesId);
        await _repo.AddAsync(pot, ct);
        _logger.LogInformation("Plant pot created: {PotId}", pot.Id);
    }
}
```

### Dispatch via Channel

Commands are dispatched **asynchronously** through a **bounded channel** with a **semaphore** for concurrency control:

```csharp
// Endpoint dispatches command
app.MapPost("/pots", async (CreatePlantPotCommand cmd, CommandDispatcher dispatcher, CancellationToken ct) =>
{
    await dispatcher.DispatchAsync(cmd, ct);  // Fire and forget
    return Results.Accepted();
})
.WithName("CreatePlantPot");

// CommandDispatcher (Application/Dispatching)
public sealed class CommandDispatcher : IAsyncDisposable
{
    private readonly Channel<(ICommand Command, CancellationToken Ct)> _channel;
    private readonly SemaphoreSlim _semaphore;  // Limits concurrent execution
    private readonly IServiceProvider _provider;

    public CommandDispatcher(IServiceProvider provider, int maxConcurrency = 4)
    {
        _provider = provider;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);  // Max 4 on Pi 5
        _channel = Channel.CreateBounded<(ICommand, CancellationToken)>(
            new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });
    }

    public async ValueTask DispatchAsync(ICommand command, CancellationToken ct = default)
        => await _channel.Writer.WriteAsync((command, ct), ct);

    public async Task ProcessAsync(CancellationToken ct)
    {
        await foreach (var (command, commandCt) in _channel.Reader.ReadAllAsync(ct))
        {
            await _semaphore.WaitAsync(ct);
            _ = Task.Run(async () =>
            {
                try { await HandleCoreAsync(command, commandCt); }
                finally { _semaphore.Release(); }  // Always release in finally
            }, ct);
        }
    }

    private async Task HandleCoreAsync(ICommand command, CancellationToken ct)
    {
        await using var scope = _provider.CreateAsyncScope();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync((dynamic)command, ct);
    }

    public async ValueTask DisposeAsync() => await _channel.Reader.Completion;
}
```

### Semaphore Rules

- ? Initialize with **equal initialCount and maxCount**: `new SemaphoreSlim(4, 4)`
- ? Release in a **finally** block — never only on happy path
- ? Use **WaitAsync(ct)** — never blocking `.Wait()`

```csharp
try
{
    await _semaphore.WaitAsync(ct);
    // do work
}
finally
{
    _semaphore.Release();  // Always reached
}
```

---

## Queries (Read Operations)

### Pattern
```csharp
// 1. Define query marker interface (Domain/Common)
public interface IQuery<TResult> { }

// 2. Define query class (Application/Feature/Queries)
public sealed record GetPlantPotsQuery() : IQuery<IReadOnlyList<PlantPotDto>>;

// 3. Define handler interface (Domain/Common)
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}

// 4. Implement handler (Application/Feature/Queries)
public sealed class GetPlantPotsQueryHandler : IQueryHandler<GetPlantPotsQuery, IReadOnlyList<PlantPotDto>>
{
    private readonly IPlantPotRepository _repo;
    
    public GetPlantPotsQueryHandler(IPlantPotRepository repo) => _repo = repo;
    
    public async Task<IReadOnlyList<PlantPotDto>> HandleAsync(GetPlantPotsQuery query, CancellationToken ct)
        => (await _repo.GetAllAsync(ct))
            .Select(p => new PlantPotDto(p.Id, p.Label))
            .ToList();
}
```

### Direct Call (No Channel)

Queries are called **directly** from endpoints — no channel dispatch needed:

```csharp
// Endpoint calls handler directly
app.MapGet("/pots", async (IQueryHandler<GetPlantPotsQuery, IReadOnlyList<PlantPotDto>> handler, CancellationToken ct) =>
    Results.Ok(await handler.HandleAsync(new GetPlantPotsQuery(), ct)))
.Produces<IReadOnlyList<PlantPotDto>>()
.WithName("GetPlantPots");
```

---

## DI Registration

```csharp
// In Program.cs

// Commands
builder.Services.AddScoped<ICommandHandler<CreatePlantPotCommand>, CreatePlantPotCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdatePlantPotCommand>, UpdatePlantPotCommandHandler>();

// Queries
builder.Services.AddScoped<IQueryHandler<GetPlantPotsQuery, IReadOnlyList<PlantPotDto>>, GetPlantPotsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetPlantPotByIdQuery, PlantPotDto?>, GetPlantPotByIdQueryHandler>();

// Dispatcher (singleton for Pi to manage single queue across requests)
builder.Services.AddSingleton(sp => new CommandDispatcher(sp, maxConcurrency: 4));
```

---

## Background Service (Command Processor)

Start the dispatcher consumer loop:

```csharp
// Infrastructure/BackgroundServices/CommandProcessorService.cs
public sealed class CommandProcessorService : BackgroundService
{
    private readonly CommandDispatcher _dispatcher;
    private readonly ILogger<CommandProcessorService> _logger;
    
    public CommandProcessorService(CommandDispatcher dispatcher, ILogger<CommandProcessorService> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CommandProcessor starting...");
        await _dispatcher.ProcessAsync(stoppingToken);
    }
}

// In Program.cs
builder.Services.AddHostedService<CommandProcessorService>();
```

---

## Concurrency Strategy

**Semaphore Max = 4** on Raspberry Pi 5 (balances throughput vs. resource usage)

```csharp
var dispatcher = new CommandDispatcher(serviceProvider, maxConcurrency: 4);
```

For Pi 5 (4-core arm64):
- 4 concurrent commands avoids thread pool starvation
- Bounded channel (capacity 100) prevents memory explosion
- Release in finally prevents deadlocks

---

## Channel Characteristics

```csharp
// BoundedChannelOptions
new BoundedChannelOptions(100)  // Max 100 queued commands
{
    FullMode = BoundedChannelFullMode.Wait  // Block writers if full, don't drop
}
```

- **Bounded** (not unlimited) — prevents OOM on Pi
- **Wait mode** — backpressure when queue full (safe for HTTP async)
- **Capacity 100** — configurable based on system load

---

## Anti-Patterns ?

? Calling command handler directly: `await handler.HandleAsync(cmd, ct);`  
? Use dispatcher: `await dispatcher.DispatchAsync(cmd, ct);`

? Mixing commands and queries in same handler  
? Separate concerns: ICommandHandler<T> vs. IQueryHandler<T, R>

? Awaiting dispatcher response  
? Fire-and-forget: dispatcher is async background work

? Using `.Result` or `.Wait()` on async code  
? Use `await` throughout

---

## See Also

- **dependency-injection.instructions.md** – How handlers are registered and resolved
- **api-design.instructions.md** – Endpoint patterns that dispatch commands and queries
- **AGENTS.md** – Logging, metrics, and broader repository conventions

