---
applyTo: "HomeAssistant.Domain/**/*.cs,HomeAssistant.Application/**/*.cs,HomeAssistant.Infrastructure.*/**/*.cs,HomeAssistant.Presentation/**/*.cs,HomeAssistant.*/*.csproj,HomeAssistant.sln"
---

# Architecture Instructions

## Clean Architecture Principles

The project follows **Clean Architecture** with four layers. Each layer has strict responsibilities and dependency rules.

```
┌─────────────────────────────────────────────────┐
│         Presentation Layer                      │
│  (HTTP endpoints, composition root, routing)    │
└────────────────────┬────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│         Application Layer                       │
│  (Use cases, CQRS dispatch, orchestration)      │
└────────────────────┬────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│         Domain Layer                            │
│  (Entities, repository interfaces, values)      │
└────────────────────┬────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────┐
│         Infrastructure Layer                    │
│  (EF Core, repos, external services, adapters)  │
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | May Reference | Responsibility |
|-------|---------------|---|
| **Presentation** | Application, Domain | HTTP routes, DTOs, composition root, middleware |
| **Application** | Domain only | Use case workflows, CQRS dispatch, service config |
| **Domain** | Nothing | Pure business logic, entities, interfaces, no frameworks |
| **Infrastructure** | Domain, Application | Implementations, databases, external APIs, adapters |

### Forbidden Dependencies

- ❌ Presentation → Infrastructure (directly)
- ❌ Application → Presentation or Infrastructure
- ❌ Domain → Presentation, Application, or Infrastructure
- ❌ Infrastructure → Presentation or Application

---

## Feature-Based Organization

Organize **all** code by **domain concern** (feature), not by technical type.

### Principle: Group by "What" not "How"

```
✅ Feature-based (Domain Concern)
Feature1/
  Entities/
    BusinessObject.cs
  Abstractions/
    IRepository.cs
  Commands/
    CreateFeatureCommand.cs
  Queries/
    GetFeatureQuery.cs
```

```
❌ Type-based (Technical Layer)
Entities/
  BusinessObject.cs
Abstractions/
  IRepository.cs
Commands/
  CreateCommand.cs
Queries/
  GetQuery.cs
```

### Why?

- Feature-based organization **isolates business logic** into cohesive units
- Easy to find all code related to a feature
- Scaling: add new features by adding new folders, not modifying existing structures
- Reduces coupling between unrelated business concerns

---

## Interface-First Design

### Contract Before Implementation

**Always** define interfaces before implementations:

1. Define the **business interface** in Domain
2. Implement concrete types in Infrastructure
3. Inject the interface in Application/Presentation

```csharp
// Domain/MyFeature/Abstractions/IRepository.cs
public interface IMyRepository
{
    Task<MyEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<MyEntity>> GetAllAsync(CancellationToken ct);
    Task AddAsync(MyEntity entity, CancellationToken ct);
}

// Infrastructure.Persistence/MyFeature/Repositories/MyRepository.cs
public sealed class MyRepository : IMyRepository { ... }

// Application/MyFeature/Services/MyService.cs
public sealed class MyService
{
    private readonly IMyRepository _repo;
    public MyService(IMyRepository repo) => _repo = repo;
}
```

### Benefits

- **Testability:** Mock the interface in unit tests
- **Flexibility:** Swap implementations without changing callers
- **Loose coupling:** Business logic doesn't depend on specific data stores or external services
- **Clarity:** Interface = contract, implementation = detail

### Registration in Composition Root

Instances are created and registered in **`Program.cs`** (the composition root). See the **Dependency Injection Composition** section for detailed registration examples.

---

## CQRS Pattern

### Commands (Write Operations)

Commands represent **state-changing operations**:

```csharp
public interface ICommand { }
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken ct);
}
```

**Characteristics:**
- Returns completion status only (no domain data back to caller)
- Dispatched through a **bounded channel** with concurrency limit
- Executed sequentially per command (order preserved)
- Suitable for transactional operations

### Queries (Read Operations)

Queries represent **data retrieval operations**:

```csharp
public interface IQuery<TResult> { }
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}
```

**Characteristics:**
- Returns typed result data
- Called **directly** (not through a channel)
- Can execute in parallel across request threads
- No side effects; safe to call multiple times

### Dispatcher Pattern

Commands flow through a dispatcher that manages concurrency:

```csharp
public interface ICommandDispatcher
{
    Task DispatchAsync(ICommand command, CancellationToken ct);
}
```

The dispatcher:
- Uses `System.Threading.Channels.Channel<T>` for queueing
- Uses `SemaphoreSlim` for concurrency control
- Ensures FIFO order for a given feature
- Prevents unbounded concurrent writes to shared state

---

## Dependency Injection Composition

### One Place to Compose

All services are registered in the **composition root** (typically `Program.cs`):

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register layers
builder.Services.AddDbContext<MyDbContext>(/* config */);
builder.Services.AddScoped<IMyRepository, MyRepository>();
builder.Services.AddScoped<IQueryHandler<GetDataQuery, DataDto>, GetDataQueryHandler>();
builder.Services.AddSingleton<ICommandDispatcher>(sp => new CommandDispatcher(sp, maxConcurrency: 4));

var app = builder.Build();
// ... routes and middleware
app.Run();
```

### No `new` Outside Composition Root

**✅ Correct:**
```csharp
public sealed class MyService
{
    private readonly IRepository _repo;
    public MyService(IRepository repo) => _repo = repo;
}
```

**❌ Wrong:**
```csharp
public sealed class MyService
{
    public MyService()
    {
        var repo = new ConcreteRepository();  // ❌ DO NOT DO THIS
    }
}
```

### Service Lifetimes

| Lifetime | Use Case | Example |
|----------|----------|---------|
| **Scoped** | Per HTTP request | Repositories, query handlers, DbContext |
| **Transient** | New instance each call | Rare; most services are scoped |
| **Singleton** | App lifetime | CommandDispatcher, Serilog, configuration |

---

## Configuration Management

### Options Pattern

Use the ASP.NET Core **Options pattern** for configuration:

```csharp
// Define in Application layer
public sealed class MyServiceOptions
{
    public required string ApiUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}

// In Program.cs
var options = new MyServiceOptions();
configuration.GetSection("MyService").Bind(options);
builder.Services.AddSingleton(options);

// Inject in services
public sealed class MyService
{
    private readonly MyServiceOptions _options;
    public MyService(MyServiceOptions options) => _options = options;
}
```

### Configuration Sources

1. **appsettings.json** – Non-sensitive configuration
2. **appsettings.{Environment}.json** – Environment-specific overrides
3. **Environment variables** – Secrets, secrets management

**Never commit secrets.** Use environment variable injection in production.

---

## Testing Architecture

### Unit Tests

Mock all external dependencies; test business logic in isolation:

```csharp
[Fact]
public async Task HandleAsync_WithValidInput_ReturnsSuccess()
{
    // Arrange: mock the interface
    var mockRepo = new Mock<IRepository>();
    mockRepo.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new MyEntity(Guid.NewGuid()));
    
    var handler = new MyCommandHandler(mockRepo.Object);
    var command = new MyCommand(Guid.NewGuid());
    
    // Act
    await handler.HandleAsync(command, CancellationToken.None);
    
    // Assert
    mockRepo.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Tests

Spin up the full host and test end-to-end:

```csharp
[Fact]
public async Task GetAsync_ReturnsExpectedData()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/myfeature/data");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### In-Memory Persistence

Use EF Core's in-memory database for fast unit tests without SQL Server:

```csharp
var options = new DbContextOptionsBuilder<MyDbContext>()
    .UseInMemoryDatabase("TestDb")
    .Options;

using var context = new MyDbContext(options);
// ... test code
```

---

## Data Access Patterns

### Repository Interface Lives in Domain

```csharp
// HomeAssistant.Domain/MyFeature/Abstractions/IMyRepository.cs
public interface IMyRepository
{
    Task<MyEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MyEntity>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(MyEntity entity, CancellationToken ct = default);
    Task UpdateAsync(MyEntity entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### Repository Implementation Lives in Infrastructure

```csharp
// HomeAssistant.Infrastructure.Persistence/MyFeature/Repositories/MyRepository.cs
public sealed class MyRepository : IMyRepository
{
    private readonly MyDbContext _context;
    public MyRepository(MyDbContext context) => _context = context;
    
    public async Task<MyEntity?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.MyEntities.FirstOrDefaultAsync(x => x.Id == id, ct);
    // ... other methods
}
```

### Entity Configurations

Keep Entity Framework configurations **separate from DbContext**:

```csharp
// Infrastructure.Persistence/MyFeature/Configurations/MyEntityConfiguration.cs
public sealed class MyEntityTypeConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100);
        // ...
    }
}

// DbContext applies all configurations via assembly scanning
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyEntityTypeConfiguration).Assembly);
}
```

---

## API Design

### Minimal APIs

Use extension methods to organize routes by feature:

```csharp
// Program.cs
app.MapMyFeatureRoutes();

// MyFeature/RouteBuilders/MyFeatureRouteBuilder.cs
public static class MyFeatureRouteBuilder
{
    public static void MapMyFeatureRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/myfeature")
            .WithName("MyFeature")
            .WithOpenApi();
        
        group.MapGet("/", GetAll).WithName("GetAll");
        group.MapPost("/", Create).WithName("Create");
        group.MapPut("/{id}", Update).WithName("Update");
    }
}
```

### Typed Results

Always use typed results with `.Produces<T>()`:

```csharp
static async Task<Ok<MyDto>> GetAll(IQueryHandler<GetAllQuery, IReadOnlyList<MyDto>> handler, CancellationToken ct)
{
    var result = await handler.HandleAsync(new GetAllQuery(), ct);
    return TypedResults.Ok(result);
}
```

Benefits:
- Type-safe endpoint definitions
- Automatic OpenAPI documentation
- IDE autocomplete for response types

---

## Logging Strategy

### Use Serilog

All logging via **injected `ILogger<T>`**, never `Console.WriteLine`:

```csharp
public sealed class MyService
{
    private readonly ILogger<MyService> _logger;
    public MyService(ILogger<MyService> logger) => _logger = logger;
    
    public async Task DoWork()
    {
        _logger.LogInformation("Starting work...");
        try
        {
            // ... work
            _logger.LogInformation("Work completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Work failed");
            throw;
        }
    }
}
```

### Structured Logging

Include context in log messages:

```csharp
_logger.LogInformation("Processing feature {FeatureId} for user {UserId}", featureId, userId);
```

Configure Serilog in `Program.cs`:

```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));
```

---

## Null Safety

### Nullable Reference Types Enabled

All projects must have nullable reference types enabled in `.csproj`:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

### Guard Clauses

Check inputs at method entry:

```csharp
public sealed class MyService
{
    private readonly IRepository _repo;
    
    public MyService(IRepository repo)
    {
        ArgumentNullException.ThrowIfNull(repo);
        _repo = repo;
    }
    
    public async Task<MyDto> GetAsync(Guid id, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return null;  // or throw if required
        
        return new MyDto(entity.Id, entity.Name);
    }
}
```
