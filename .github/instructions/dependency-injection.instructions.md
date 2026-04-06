---
applyTo: "GardenAI.Presentation/Program.cs,GardenAI.Application/**/*.cs,GardenAI.Infrastructure.*/**/*.cs"
---

# Dependency Injection Instructions

## Core Principle

**Never instantiate services directly** outside the composition root. All dependencies flow through constructor injection via the service provider.

**Exceptions:** Immutable records (DTOs), value objects, and test factories may use `new` directly since they are not shared singleton/scoped services.

```csharp
// ? Wrong: Creating services directly
var service = new MyRepository();

// ? Correct: Injecting via interface
public sealed class MyClass
{
    private readonly IRepository _repo;
    public MyClass(IRepository repo) => _repo = repo;
}

// ? Exception: Creating DTOs directly is fine
var dto = new MyDto(id, name);  // ? OK, immutable record
```

---

## Service Lifetimes

Choose the correct lifetime for each service:

| Lifetime | Meaning | Use Case | Example |
|----------|---------|----------|---------|
| **Scoped** | New instance per HTTP request | Stateful per-request services | Repositories, DbContext, query/command handlers |
| **Transient** | New instance every time | Stateless throwaway objects | Rare; usually scoped is better |
| **Singleton** | One instance for entire app lifetime | Shared, stateless services | Configuration, logging, CommandDispatcher |

```csharp
// Scoped: fresh for each request
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IQueryHandler<GetQuery, ResultDto>, GetQueryHandler>();

// Singleton: shared across all requests
builder.Services.AddSingleton<ICommandDispatcher>(sp => new CommandDispatcher(sp, maxConcurrency: 4));
builder.Services.AddSingleton<ILogger, Logger>();

// Transient: new each time (avoid unless necessary)
builder.Services.AddTransient<IValidator, Validator>();
```

---

## Composition Root Pattern

### One Place to Register Everything

All service registration happens in **`Program.cs`** — no other file should call `AddScoped` or similar:

```csharp
var builder = WebApplication.CreateBuilder(args);

// -- Layer 1: Domain Interfaces (no implementations here)
// (Domain layer defines interfaces only, not registered here)

// -- Layer 2: Application Services & CQRS
builder.Services.AddScoped<IQueryHandler<GetDataQuery, DataDto>, GetDataQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateDataCommand>, CreateDataCommandHandler>();

// -- Layer 3: Infrastructure - Persistence
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IRepository, Repository>();

// -- Layer 3: Infrastructure - External Services
builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>((_, client) =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// -- Layer 3: Infrastructure - Messaging
var mqttOptions = new MqttOptions();
builder.Configuration.GetSection("Mqtt").Bind(mqttOptions);
builder.Services.AddSingleton(mqttOptions);
builder.Services.AddSingleton<IMessageBroker, MqttMessageBroker>();

// -- Layer 2: CQRS Dispatcher
builder.Services.AddSingleton(sp => new CommandDispatcher(sp, maxConcurrency: 4));

// -- Logging
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

// -- Build and map routes
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapFeatureRoutes();

app.Run();
```

### Benefits

- **One source of truth** – easy to see all dependencies at a glance
- **No hidden wiring** – no magic DI setup scattered across classes
- **Testability** – tests can mock the composition by swapping implementations
- **Clarity** – dependencies are explicit and visible

---

## Constructor Injection

Always inject dependencies via constructor:

```csharp
// ? Correct
public sealed class MyQueryHandler : IQueryHandler<GetDataQuery, DataDto>
{
    private readonly IRepository _repo;
    private readonly ILogger<MyQueryHandler> _logger;
    
    public MyQueryHandler(IRepository repo, ILogger<MyQueryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }
    
    public async Task<DataDto> HandleAsync(GetDataQuery query, CancellationToken ct)
    {
        _logger.LogInformation("Processing query...");
        var data = await _repo.GetDataAsync(ct);
        return new DataDto(data.Id, data.Name);
    }
}

// ? Wrong: Service locator pattern
public sealed class MyQueryHandler
{
    public async Task<DataDto> HandleAsync(GetDataQuery query)
    {
        var repo = ServiceLocator.Resolve<IRepository>();  // DO NOT DO THIS
        return await repo.GetDataAsync();
    }
}

// ? Wrong: Direct instantiation
public sealed class MyQueryHandler
{
    public async Task<DataDto> HandleAsync(GetDataQuery query)
    {
        var repo = new Repository(context);  // DO NOT DO THIS
        return await repo.GetDataAsync();
    }
}
```

---

## Guard Clauses in Constructors

Always validate constructor arguments:

```csharp
public sealed class MyService
{
    private readonly IRepository _repo;
    private readonly ILogger<MyService> _logger;
    
    public MyService(IRepository repo, ILogger<MyService> logger)
    {
        ArgumentNullException.ThrowIfNull(repo);
        ArgumentNullException.ThrowIfNull(logger);
        
        _repo = repo;
        _logger = logger;
    }
}
```

---

## Scoped Services in Singleton Context

?? **Never inject a scoped service into a singleton** — this creates a "captive dependency":

```csharp
// ? WRONG: Scoped service captured by singleton
builder.Services.AddSingleton(sp => 
{
    var repo = sp.GetRequiredService<IRepository>();  // ? Repo is scoped!
    return new BackgroundProcessor(repo);               // ? But this is singleton!
});

// ? CORRECT: Singleton creates its own scope per operation
builder.Services.AddSingleton(sp =>
{
    return new BackgroundProcessor(sp);  // ? Pass provider, not service
});

public sealed class BackgroundProcessor : BackgroundService
{
    private readonly IServiceProvider _provider;
    
    public BackgroundProcessor(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create a NEW scope for each operation
            await using var scope = _provider.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
            
            // Use repo...
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

---

## Factory Methods

Sometimes object construction is complex. Use factory delegates in Program.cs:

```csharp
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["ExternalApi:Key"] ?? throw new InvalidOperationException("Missing API key");
    
    return new ExternalApiClient(apiKey, new HttpClient());
});
```

Or use a factory class:

```csharp
builder.Services.AddSingleton<IMyService>(sp =>
    MyServiceFactory.Create(
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<ILogger<MyService>>()));
```

---

## Options Pattern for Configuration

Use the **Options pattern** to inject configuration:

```csharp
// Define options class in Application layer
public sealed class MyServiceOptions
{
    public required string ApiUrl { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

// In Program.cs: bind from configuration
var options = new MyServiceOptions();
builder.Configuration.GetSection("MyService").Bind(options);
builder.Services.AddSingleton(options);

// In service: inject the options
public sealed class MyService
{
    private readonly MyServiceOptions _options;
    
    public MyService(MyServiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }
    
    public async Task CallApiAsync()
    {
        using var cts = new CancellationTokenSource(_options.Timeout);
        // ... call API
    }
}
```

---

## Conditional Registration

Register different implementations based on environment:

```csharp
// Development: use mock
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IExternalService, MockExternalService>();
}
else
{
    // Production: use real implementation
    builder.Services.AddSingleton<IExternalService, RealExternalService>();
}
```

---

## Testing with Dependency Injection

### Unit Tests with Mocks

```csharp
[Fact]
public async Task HandleAsync_CallsRepository()
{
    // Arrange
    var mockRepo = new Mock<IRepository>();
    mockRepo.Setup(x => x.GetDataAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Data(Guid.NewGuid(), "Test"));
    
    var mockLogger = new Mock<ILogger<MyQueryHandler>>();
    var handler = new MyQueryHandler(mockRepo.Object, mockLogger.Object);
    
    // Act
    var result = await handler.HandleAsync(new GetDataQuery(), CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    mockRepo.Verify(x => x.GetDataAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Tests with WebApplicationFactory

```csharp
[Fact]
public async Task GetData_ReturnsOk()
{
    // Arrange: Create host with DI container
    var factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override specific services for testing
                services.AddScoped<IRepository>(_ => new MockRepository());
            });
        });
    
    var client = factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/data");
    
    // Assert
    Assert.True(response.IsSuccessStatusCode);
}
```

---

## IServiceProvider Usage

When you need to resolve services manually (rare):

```csharp
// ? Avoid: Manual resolution usually means design issue
public sealed class MyClass
{
    private readonly IServiceProvider _provider;
    
    public MyClass(IServiceProvider provider)
    {
        var repo = provider.GetRequiredService<IRepository>();  // Awkward
    }
}

// ? Prefer: Direct injection
public sealed class MyClass
{
    private readonly IRepository _repo;
    
    public MyClass(IRepository repo)
    {
        _repo = repo;  // Clean
    }
}
```

Only use `IServiceProvider` when you need dynamic resolution or scope creation:

```csharp
// Legitimate use: creating new scope in background service
public sealed class BackgroundService
{
    private readonly IServiceProvider _provider;
    
    public BackgroundService(IServiceProvider provider) => _provider = provider;
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = _provider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<MyQuery, MyResult>>();
        // ...
    }
}
```

---

## Anti-Patterns to Avoid

| Anti-Pattern | Problem | Solution |
|---|---|---|
| **Service Locator** | Hidden dependencies, hard to test | Use constructor injection |
| **Direct `new`** | Tight coupling, untestable | Inject interface |
| **Static singletons** | Global state, concurrency issues | Inject via DI |
| **Scoped in Singleton** | Captive dependency, memory leak | Create new scope |
| **Two composition roots** | Confusion, inconsistency | One Program.cs |

---

## See Also

- **architecture.instructions.md** – Layer structure and responsibilities
- **interface-first.instructions.md** – Designing contracts before implementations
- **AGENTS.md** – Full system overview and conventions
