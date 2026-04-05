---
applyTo: "HomeAssistant.Domain/**/*.cs,HomeAssistant.Application/**/*.cs,HomeAssistant.Infrastructure.Persistence/**/*.cs,HomeAssistant.Infrastructure.Sensors/**/*.cs,HomeAssistant.Integrations.*/**/*.cs,HomeAssistant.Presentation/**/*.cs,HomeAssistant.*/*.csproj,HomeAssistant.sln"
---

# Architecture Instructions

## Clean Architecture Layers

The project follows **Clean Architecture** with four layers:

```
HomeAssistant.Presentation  ← ASP.NET Core Minimal APIs, SignalR hubs, composition root
        ↓
HomeAssistant.Application   ← Use cases, CQRS handlers, Semantic Kernel agents
        ↓
HomeAssistant.Domain        ← Domain model, interfaces, value objects, CQRS markers
        ↓
HomeAssistant.Infrastructure.Persistence + HomeAssistant.Infrastructure.Sensors ← EF Core, repositories, sensor providers, background services
```

### Layer Rules

1. **Presentation** → May reference Application & Domain (not Infrastructure)
2. **Application** → May reference Domain only (orchestration logic)
3. **Domain** → No external dependencies (pure business logic)
4. **Infrastructure projects** → Implement Domain/Application interfaces (no upward references)

---

## Domain Model

### Core Entities
- **PlantPot** – A single plant pot with id, label, position, species
- **PlantSpecies** – Definition of a plant type (ideal moisture range, temp range)
- **SensorReading** – Single timestamp, soil moisture %, temperature °C for a pot

### 6 Plant Pots
- Domain logic must account for exactly 6 monitored pots
- All pot data stored in PostgreSQL (via EF Core + Npgsql)
- Latest readings accessible via repository

---

## Folder Structure

**Feature-based organization** (not type-based):

```
HomeAssistant.Domain/
  PlantPots/
    Entities/
      PlantPot.cs
      PlantSpecies.cs
    Abstractions/
      IPlantPotRepository.cs
  SensorReadings/
    Entities/
      SensorReading.cs
    Abstractions/
      ISensorReadingRepository.cs
      ISensorProvider.cs
  Assistant/
    Entities/
      ChatSession.cs
      ChatMessage.cs
    Abstractions/
      IChatSessionRepository.cs
  Common/
    Markers/
      ICommand.cs
      IQuery.cs
    Handlers/
      ICommandHandler.cs
      IQueryHandler.cs

HomeAssistant.Application/
  PlantPots/
    Commands/
      CreatePlantPot/
        CreatePlantPotCommand.cs
        CreatePlantPotCommandHandler.cs
    Queries/
      GetPlantPots/
        GetPlantPotsQuery.cs
        GetPlantPotsQueryHandler.cs
  SensorReadings/
    Queries/
      GetLatestReadings/
        GetLatestReadingsQuery.cs
        GetLatestReadingsQueryHandler.cs
  Agents/
    GardenerAgent.cs
    WeatherExpertAgent.cs
    PlannerAgent.cs
  Dispatching/
    Abstractions/
      ICommandDispatcher.cs
    Services/
      CommandDispatcher.cs

HomeAssistant.Infrastructure.Persistence/
  Database/
    AppDbContext.cs
  PlantPots/
    Configurations/
      PlantPotEntityTypeConfiguration.cs
      PlantSpeciesEntityTypeConfiguration.cs
    Repositories/
      PlantPotRepository.cs
  SensorReadings/
    Configurations/
      SensorReadingEntityTypeConfiguration.cs
    Repositories/
      SensorReadingRepository.cs
  Assistant/
    Configurations/
      ChatMessageEntityTypeConfiguration.cs
      ChatSessionEntityTypeConfiguration.cs
    Repositories/
      ChatSessionRepository.cs

HomeAssistant.Infrastructure.Sensors/
  Sensors/
    Providers/
      MockSensorProvider.cs
      Zigbee2MqttSensorProvider.cs
  BackgroundServices/
    SensorPollingService.cs

HomeAssistant.Integrations.OpenMeteo/
  Forecast/
    Abstractions/
      IOpenMeteoForecastClient.cs
    Clients/
      OpenMeteoForecastClient.cs
    Configuration/
      OpenMeteoClientOptions.cs
    Contracts/
      OpenMeteoForecastRequest.cs
      OpenMeteoForecastResponse.cs
      OpenMeteoTimeSeriesBlock.cs
    Exceptions/
      OpenMeteoApiException.cs

HomeAssistant.Presentation/
  Chat/
    Abstractions/
      IChatAssistant.cs
    Contracts/
      ChatRequest.cs
      ChatResponse.cs
    RouteBuilders/
      ChatRouteBuilder.cs
    Endpoints/
      PostChatPrompt/
        PostChatPromptEndpoint.cs
      CreateChatSession/
        CreateChatSessionEndpoint.cs
      ListChatSessions/
        ListChatSessionsEndpoint.cs
      GetChatSession/
        GetChatSessionEndpoint.cs
      PostChatSessionMessage/
        PostChatSessionMessageEndpoint.cs
    Services/
      OllamaChatAssistant.cs
  Endpoints/
    PlantPotEndpoints.cs
    SensorReadingEndpoints.cs
  Hubs/
    SensorHub.cs
  Program.cs
  HomeAssistant.Presentation.http
```

### File Placement Rules

- **Never** add files to project root
- **Always** use feature folders (PlantPots, SensorReadings, Agents, etc.)
- **Group by concern**, not by type
- **Interfaces in Domain**, implementations in Infrastructure
- In `HomeAssistant.Domain`, prefer `Entities/` and `Abstractions/` subfolders once a feature contains both domain models and contracts
- In `HomeAssistant.Domain/Common`, keep marker interfaces under `Markers/` and handler contracts under `Handlers/`
- When a feature contains multiple responsibility kinds, split into focused subfolders such as `Abstractions`, `Contracts`, `Services`, `Repositories`, `Configurations`, `Providers`, `Clients`, `Configuration`, and `Exceptions`
- Do not mix service implementations with contracts or abstractions in the same folder unless the feature is trivially small
- In `HomeAssistant.Presentation`, keep `Program.cs` as the composition root and delegate HTTP mapping to feature route builders such as `<Domain>RouteBuilder`
- For presentation features with multiple endpoints, place each endpoint in its own `Endpoints/<EndpointName>/` folder instead of flattening endpoint files together

---

## Key Abstractions

### Repositories (Domain → Infrastructure)
```csharp
// Domain
public interface IPlantPotRepository
{
    Task<PlantPot?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PlantPot>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(PlantPot pot, CancellationToken ct = default);
}

// Infrastructure
public sealed class PlantPotRepository : IPlantPotRepository { ... }
```

### Sensor Provider (Domain → Infrastructure)
```csharp
// Domain
public interface ISensorProvider
{
    Task<IReadOnlyList<SensorReading>> GetLatestReadingsAsync(CancellationToken ct = default);
}

// Development (mock)
public sealed class MockSensorProvider : ISensorProvider { ... }

// Production (real hardware)
public sealed class Zigbee2MqttSensorProvider : ISensorProvider { ... }
```

---

## Composition Root

**All DI registration happens in `Program.cs`** — never instantiate services elsewhere.

```csharp
// Program.cs
builder.Services.AddScoped<IPlantPotRepository, PlantPotRepository>();
builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<ISensorProvider, MockSensorProvider>();
else
    builder.Services.AddSingleton<ISensorProvider, Zigbee2MqttSensorProvider>();
```

---

## Layer Boundaries

### ✅ Allowed References
- Presentation → Application interfaces, Domain interfaces
- Application → Domain interfaces only
- Infrastructure → implements Domain/Application interfaces

### ❌ Forbidden
- Presentation → Infrastructure (except via interfaces)
- Infrastructure → Presentation
- Domain → anything external
- "new ConcreteService()" outside Program.cs

---

## Adding a New Layer

```powershell
# 1. Create project
dotnet new classlib -n HomeAssistant.<LayerName> -f net10.0

# 2. Add to solution
dotnet sln add HomeAssistant.<LayerName>/HomeAssistant.<LayerName>.csproj

# 3. Add project reference in consuming layer
# Edit consuming .csproj: <ProjectReference Include="..\HomeAssistant.<LayerName>\HomeAssistant.<LayerName>.csproj" />

# 4. Register in Program.cs (if providing services)
```

---

## See Also

- **cqrs.instructions.md** – How commands and queries flow through layers
- **dependency-injection.instructions.md** – DI setup and patterns
- **interface-first.instructions.md** – Always define interfaces before implementations

