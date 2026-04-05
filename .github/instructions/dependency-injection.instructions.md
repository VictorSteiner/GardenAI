---
applyTo: "HomeAssistant.Presentation/Program.cs,HomeAssistant.Presentation/Endpoints/**/*.cs,HomeAssistant.Presentation/Hubs/**/*.cs,HomeAssistant.Application/**/*.cs,HomeAssistant.Infrastructure.Persistence/**/*.cs,HomeAssistant.Infrastructure.Sensors/**/*.cs,HomeAssistant.Integrations.*/**/*.cs"
---

# Dependency Injection Instructions

## Principle

**No `new ConcreteService()`** outside of:
- `Program.cs` (composition root)
- Test factories
- Static factory methods (rare)

All dependencies flow through the **service provider** via constructor injection.

---

## Service Lifetimes

| Lifetime | Use Case | Example |
|----------|----------|---------|
| **Scoped** | Per HTTP request | Repositories, query handlers |
| **Transient** | New instance each time | Command handlers (rare; usually scoped) |
| **Singleton** | One instance for app lifetime | CommandDispatcher, Serilog logger, Ollama client |

```csharp
// Scoped (most common for repositories and handlers)
builder.Services.AddScoped<IPlantPotRepository, PlantPotRepository>();
builder.Services.AddScoped<IQueryHandler<GetAllPotsQuery, IReadOnlyList<PlantPotDto>>, GetAllPotsQueryHandler>();

// Singleton (stateless services, background processors)
builder.Services.AddSingleton(sp => new CommandDispatcher(sp, maxConcurrency: 4));
builder.Services.AddSingleton<ISensorProvider, MockSensorProvider>();  // Swappable per env

// Background services
builder.Services.AddHostedService<SensorPollingService>();
```

---

## The Composition Root (Program.cs)

All DI setup lives in one place:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Layer: Infrastructure (persistence)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=homeassistant.db"));
builder.Services.AddScoped<IPlantPotRepository, PlantPotRepository>();
builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();

// Layer: Application (use cases)
builder.Services.AddScoped<IQueryHandler<GetAllPotsQuery, IReadOnlyList<PlantPotDto>>, GetAllPotsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreatePlantPotCommand>, CreatePlantPotCommandHandler>();
builder.Services.AddSingleton(sp => new CommandDispatcher(sp, maxConcurrency: 4));

// Layer: Infrastructure (sensors)
if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<ISensorProvider, MockSensorProvider>();
else
    builder.Services.AddSingleton<ISensorProvider, Zigbee2MqttSensorProvider>();

// Layer: Infrastructure (background services)
builder.Services.AddHostedService<SensorPollingService>();
builder.Services.AddHostedService<CommandProcessorService>();

// Logging
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/homeassistant-.log", rollingInterval: RollingInterval.Day));

// Endpoints
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPlantPotEndpoints();
app.MapSensorReadingEndpoints();
app.MapHub<SensorHub>("/hubs/sensors");

app.Run();
```

---

## Mock-First Pattern (for Testing)

Register mock implementations in Development, real in Production:

```csharp
// Always inject the INTERFACE
public sealed class SensorPollingService : BackgroundService
{
    private readonly ISensorProvider _provider;  // ← Interface only
    
    public SensorPollingService(ISensorProvider provider) => _provider = provider;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var readings = await _provider.GetLatestReadingsAsync(stoppingToken);
            // process...
            await Task.Delay(5000, stoppingToken);
        }
    }
}

// In Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<ISensorProvider, MockSensorProvider>();
}
else
{
    builder.Services.AddSingleton<ISensorProvider, Zigbee2MqttSensorProvider>();
}
```

---

## Constructor Injection Pattern

```csharp
// ✅ Correct
public sealed class GetPlantPotsQueryHandler : IQueryHandler<GetAllPotsQuery, IReadOnlyList<PlantPotDto>>
{
    private readonly IPlantPotRepository _repo;
    private readonly ILogger<GetPlantPotsQueryHandler> _logger;
    
    public GetPlantPotsQueryHandler(IPlantPotRepository repo, ILogger<GetPlantPotsQueryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }
    
    public async Task<IReadOnlyList<PlantPotDto>> HandleAsync(GetAllPotsQuery query, CancellationToken ct)
    {
        _logger.LogInformation("Fetching all pots...");
        return (await _repo.GetAllAsync(ct)).Select(p => new PlantPotDto(p.Id, p.Label)).ToList();
    }
}

// ❌ Wrong
public sealed class GetPlantPotsQueryHandler
{
    public async Task<IReadOnlyList<PlantPotDto>> HandleAsync(CancellationToken ct)
    {
        var repo = new PlantPotRepository(context);  // DO NOT DO THIS
        // ...
    }
}
```

---

## Factory Methods (Rare)

Sometimes you need factories for complex object construction:

```csharp
// In Program.cs
builder.Services.AddSingleton(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    var ollamaUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434";
    
    return Kernel.CreateBuilder()
        .AddOllamaChatCompletion(
            modelId: "llama3.2",
            endpoint: new Uri(ollamaUrl))
        .Build();
});
```

---

## Scoped Services in Singleton Context

⚠️ **Never inject a scoped service into a singleton** — it creates a "captive dependency":

```csharp
// ❌ Wrong: Scoped service captured by singleton
builder.Services.AddSingleton(sp => 
    new SensorPollingService(sp.GetRequiredService<IPlantPotRepository>()));  // Repo is scoped!

// ✅ Correct: Singleton creates its own scope
builder.Services.AddSingleton(sp =>
{
    return new SensorPollingService(sp);  // Pass the provider, not the service
});

public sealed class SensorPollingService : BackgroundService
{
    private readonly IServiceProvider _provider;
    
    public SensorPollingService(IServiceProvider provider) => _provider = provider;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _provider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPlantPotRepository>();
        // use repo...
    }
}
```

---

## Testing with Mocks

```csharp
// xUnit test
public class GetPlantPotsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAllPots()
    {
        // Arrange
        var mockRepo = new Mock<IPlantPotRepository>();
        mockRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlantPot> { new(Guid.NewGuid(), "Pot1", Guid.NewGuid()) });
        
        var mockLogger = new Mock<ILogger<GetPlantPotsQueryHandler>>();
        var handler = new GetPlantPotsQueryHandler(mockRepo.Object, mockLogger.Object);
        
        // Act
        var result = await handler.HandleAsync(new GetAllPotsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Single(result);
    }
}
```

---

## Service Location (Anti-Pattern)

❌ Avoid service locator pattern:

```csharp
// ❌ Never do this
var repo = ServiceLocator.Get<IPlantPotRepository>();

// ✅ Always inject
public sealed class MyService
{
    private readonly IPlantPotRepository _repo;
    public MyService(IPlantPotRepository repo) => _repo = repo;
}
```

---

## See Also

- **architecture.instructions.md** – Layer structure and responsibilities
- **cqrs.instructions.md** – How handlers are resolved from the container
- **AGENTS.md** – Configuration, secrets, and options guidance for the repository

