---
applyTo: "HomeAssistant.Domain/**/*.cs,HomeAssistant.Application/**/*.cs,HomeAssistant.Infrastructure.Persistence/**/*.cs,HomeAssistant.Infrastructure.Sensors/**/*.cs,HomeAssistant.Integrations.*/**/*.cs"
---

# Interface-First Discipline

## Rule: Interface Before Implementation

**Every concrete class must have a corresponding interface**, and the **interface must be defined first** in the plan and in the file structure.

---

## Pattern

```csharp
// ✅ Step 1: Define interface in Domain
// HomeAssistant.Domain/PlantPots/Abstractions/IPlantPotRepository.cs
public interface IPlantPotRepository
{
    /// <summary>Retrieves all plant pots.</summary>
    Task<IReadOnlyList<PlantPot>> GetAllAsync(CancellationToken ct = default);
    
    /// <summary>Retrieves a pot by ID.</summary>
    Task<PlantPot?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>Adds a new pot.</summary>
    Task AddAsync(PlantPot pot, CancellationToken ct = default);
}

// ✅ Step 2: Implement in Infrastructure
// HomeAssistant.Infrastructure.Persistence/PlantPots/Repositories/PlantPotRepository.cs
public sealed class PlantPotRepository : IPlantPotRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PlantPotRepository> _logger;
    
    public PlantPotRepository(AppDbContext context, ILogger<PlantPotRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<IReadOnlyList<PlantPot>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all pots");
        return (await _context.PlantPots.ToListAsync(ct)).AsReadOnly();
    }
    
    public async Task<PlantPot?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(id.ToString());
        return await _context.PlantPots.FindAsync([id], cancellationToken: ct);
    }
    
    public async Task AddAsync(PlantPot pot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pot);
        await _context.PlantPots.AddAsync(pot, ct);
        await _context.SaveChangesAsync(ct);
    }
}
```

---

## Layer Responsibilities

### Domain Layer Interfaces
```csharp
// HomeAssistant.Domain/PlantPots/Abstractions/
public interface IPlantPotRepository { }

// HomeAssistant.Domain/SensorReadings/Abstractions/
public interface ISensorReadingRepository { }
public interface ISensorProvider { }

// HomeAssistant.Domain/Common/Markers/
public interface ICommand { }
public interface IQuery<TResult> { }

// HomeAssistant.Domain/Common/Handlers/
public interface ICommandHandler<TCommand> where TCommand : ICommand { }
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult> { }
```

### Application Layer Interfaces
```csharp
// HomeAssistant.Application/Services/
public interface IGardenerAgent { }
public interface IWeatherExpertAgent { }
public interface IPlannerAgent { }
```

### Infrastructure Layer Implementations
```csharp
// HomeAssistant.Infrastructure.Persistence/PlantPots/
public sealed class PlantPotRepository : IPlantPotRepository { }
public sealed class SensorReadingRepository : ISensorReadingRepository { }

// HomeAssistant.Infrastructure.Sensors/Sensors/
public sealed class MockSensorProvider : ISensorProvider { }
public sealed class Zigbee2MqttSensorProvider : ISensorProvider { }
```

---

## No Concrete Types Across Boundaries

### ❌ Wrong: Concrete type leaks to Presentation

```csharp
// HomeAssistant.Infrastructure.Persistence
public sealed class PlantPotRepository : IPlantPotRepository { }

// HomeAssistant.Presentation
app.MapGet("/pots", async (PlantPotRepository repo) =>  // ❌ Concrete type!
{
    var pots = await repo.GetAllAsync(CancellationToken.None);
    return Results.Ok(pots);
});
```

### ✅ Correct: Only interface crosses boundaries

```csharp
// HomeAssistant.Presentation
app.MapGet("/pots", async (IPlantPotRepository repo) =>  // ✅ Interface only
{
    var pots = await repo.GetAllAsync(CancellationToken.None);
    return Results.Ok(pots);
});
```

---

## Sealed Classes

Use `sealed` on concrete implementations to prevent accidental subclassing:

```csharp
// ✅ Correct
public sealed class PlantPotRepository : IPlantPotRepository { }

// ❌ Wrong
public class PlantPotRepository : IPlantPotRepository { }
```

---

## XML Docs on Interfaces

Document the contract thoroughly:

```csharp
public interface IPlantPotRepository
{
    /// <summary>Retrieves all plant pots in the system.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of plant pots, or empty if none exist.</returns>
    /// <exception cref="OperationCanceledException">Thrown if cancellation is requested.</exception>
    Task<IReadOnlyList<PlantPot>> GetAllAsync(CancellationToken ct = default);
}
```

---

## Validation with Guard Clauses

Defensive checks at entry point:

```csharp
public async Task<PlantPot?> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    ArgumentException.ThrowIfNullOrEmpty(id.ToString());  // Guard
    return await _context.PlantPots.FindAsync([id], cancellationToken: ct);
}

public async Task AddAsync(PlantPot pot, CancellationToken ct = default)
{
    ArgumentNullException.ThrowIfNull(pot);  // Guard
    await _context.PlantPots.AddAsync(pot, ct);
    await _context.SaveChangesAsync(ct);
}
```

---

## Testing with Interfaces

Mock interfaces, not concrete classes:

```csharp
// ✅ Correct: Mock the interface
var mockRepo = new Mock<IPlantPotRepository>();
mockRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<PlantPot> { /* ... */ });

var handler = new GetAllPotsQueryHandler(mockRepo.Object);

// ❌ Wrong: Can't mock a sealed concrete class easily
var mockRepo = new Mock<PlantPotRepository>();  // Less flexible
```

---

## Nullable References in Interfaces

Enable `<Nullable>enable</Nullable>` in all `.csproj` files:

```csharp
public interface IPlantPotRepository
{
    /// <summary>Gets a pot, or null if not found.</summary>
    Task<PlantPot?> GetByIdAsync(Guid id, CancellationToken ct = default);  // ← ? for nullable
}
```

---

## Checklist

When planning or implementing a feature:

- [ ] Interface defined **before** concrete class
- [ ] Interface resides in **Domain or Application** layer
- [ ] Implementation is **sealed**
- [ ] Implementation resides in **Infrastructure** layer
- [ ] No concrete types leak across layer boundaries
- [ ] All public methods have XML `<summary>` docs
- [ ] Guard clauses for null/invalid input
- [ ] Return types use `?` for nullable references

---

## See Also

- **architecture.instructions.md** – Layer responsibilities and folder structure
- **dependency-injection.instructions.md** – How interfaces are resolved from the container

