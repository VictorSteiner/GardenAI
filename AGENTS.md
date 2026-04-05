# HomeAssistant – Agent Guide

## Project Vision

A **custom, agent-based garden automation platform** running on a Raspberry Pi 5.
A "Council of Agents" (Gardener, Weather Expert, Planner) — orchestrated via **Semantic Kernel** — monitors 6 DIY plant pots using Zigbee soil-moisture and temperature sensors (delivered through Zigbee2MQTT) and makes autonomous watering/care decisions.

This is **not** a Home Assistant integration. It is a standalone system.

---

## Solution Architecture (Clean Architecture)

| Layer | Project | Status | Responsibility |
|---|---|---|---|
| API / Host | `HomeAssistant.Presentation` | ✅ exists | Minimal API endpoints, SignalR hubs, composition root |
| Use Cases | `HomeAssistant.Application` | 🔜 planned | Agent orchestration (Semantic Kernel), CQRS dispatching, service interfaces |
| Domain | `HomeAssistant.Domain` | 🔜 planned | `PlantPot`, `PlantSpecies`, `SensorReading`, repository & sensor interfaces, CQRS marker interfaces |
| DB / IO | `HomeAssistant.Infrastructure.Persistence` + `HomeAssistant.Infrastructure.Sensors` | ✅ exists | EF Core persistence adapters and sensor provider adapters |

New projects → added to `HomeAssistant.sln`, named `HomeAssistant.<Layer>`.

### Adding a layer project
```powershell
dotnet new classlib -n HomeAssistant.Domain -f net10.0
dotnet sln add HomeAssistant.Domain/HomeAssistant.Domain.csproj
# Then add a <ProjectReference> in the consuming .csproj
```

---

## Domain Model

Core entities live in `HomeAssistant.Domain`:

```
PlantSpecies          – name, ideal moisture range, ideal temp range
PlantPot              – id, label, position, PlantSpecies, list of SensorReadings
SensorReading         – potId, timestamp, soilMoisture (%), temperatureC
```

Council of Agents (orchestrated in `HomeAssistant.Application`):
- **Gardener** – interprets sensor data, decides watering actions
- **Weather Expert** – fetches/forecasts weather to adjust schedules
- **Planner** – coordinates schedules across all 6 pots

---

## Key Abstractions (never skip these)

```csharp
// HomeAssistant.Domain
public interface ISensorProvider
{
    Task<IReadOnlyList<SensorReading>> GetLatestReadingsAsync(CancellationToken ct = default);
}

public interface IPlantPotRepository { /* CRUD + latest readings */ }
public interface ISensorReadingRepository { /* append + query */ }

// CQRS marker interfaces (HomeAssistant.Domain/Common/)
public interface ICommand { }
public interface IQuery<TResult> { }
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
```

**`ISensorProvider` is the primary seam** between mock (dev/test) and Zigbee2MQTT (production).
Always inject it; never instantiate a concrete sensor class directly.

---

## Full Stack

| Concern | Technology |
|---|---|
| Backend API | ASP.NET Core Minimal API (.NET 10) |
| Real-time push | SignalR (sensor updates to frontend) |
| AI agents | Semantic Kernel + Ollama (local, on-Pi LLM) |
| ORM / DB | EF Core 10 + PostgreSQL (Docker on dev; production Pi) |
| CQRS dispatch | Custom channel-based dispatcher (`System.Threading.Channels` + `SemaphoreSlim`) |
| Background polling | `BackgroundService` in infrastructure adapter projects |
| Logging | Serilog (console + rolling file sink) |
| Metrics | `System.Diagnostics.Metrics` (built-in .NET meter/counter) |
| Frontend | React + TypeScript + TanStack Router + TanStack Query + Tailwind CSS |
| Sensor bridge | Zigbee2MQTT over MQTT (Mosquitto broker) |
| Deployment | Docker Compose on Raspberry Pi 5 |
| Reverse proxy | Nginx (planned, not yet wired) |

---

## Project Folder Structure

Feature-based folders inside each layer — group by domain concept, not by type.

```
HomeAssistant.Domain/
  PlantPots/
    Entities/           ← PlantPot.cs, PlantSpecies.cs
    Abstractions/       ← IPlantPotRepository.cs
  SensorReadings/
    Entities/           ← SensorReading.cs
    Abstractions/       ← ISensorReadingRepository.cs, ISensorProvider.cs
  Assistant/
    Entities/           ← ChatSession.cs, ChatMessage.cs
    Abstractions/       ← IChatSessionRepository.cs
  Common/
    Markers/            ← ICommand.cs, IQuery.cs
    Handlers/           ← ICommandHandler.cs, IQueryHandler.cs

HomeAssistant.Application/
  PlantPots/
    Commands/           ← e.g. UpdatePlantPotCommand.cs + Handler
    Queries/            ← e.g. GetAllPotsQuery.cs + Handler
  SensorReadings/
    Queries/            ← e.g. GetLatestReadingsQuery.cs + Handler
  Dispatching/
    Abstractions/       ← ICommandDispatcher.cs
    Services/           ← CommandDispatcher.cs, QueryDispatcher.cs
  Agents/               ← GardenerAgent.cs, WeatherExpertAgent.cs, PlannerAgent.cs

HomeAssistant.Infrastructure.Persistence/
  Database/
    AppDbContext.cs
  Migrations/
  PlantPots/
    Repositories/       ← PlantPotRepository.cs
    Configurations/     ← entity configurations
  SensorReadings/
    Repositories/       ← SensorReadingRepository.cs
    Configurations/     ← entity configurations
  Assistant/
    Repositories/       ← ChatSessionRepository.cs
    Configurations/     ← entity configurations

HomeAssistant.Infrastructure.Sensors/
  Sensors/
    Providers/          ← MockSensorProvider.cs, Zigbee2MqttSensorProvider.cs

HomeAssistant.Integrations.OpenMeteo/
  Forecast/
    Abstractions/       ← IOpenMeteoForecastClient.cs
    Clients/            ← OpenMeteoForecastClient.cs
    Configuration/      ← OpenMeteoClientOptions.cs
    Contracts/          ← forecast request/response models
    Exceptions/         ← OpenMeteoApiException.cs

# Future split targets
HomeAssistant.Infrastructure.Messaging/
  Messaging/            ← MqttService.cs
HomeAssistant.Infrastructure.BackgroundServices/
  BackgroundServices/   ← SensorPollingService.cs

HomeAssistant.Presentation/
  Chat/
    Abstractions/       ← IChatAssistant.cs
    Contracts/          ← chat DTOs
    RouteBuilders/      ← ChatRouteBuilder.cs
    Endpoints/          ← one folder per endpoint, e.g. PostChatPrompt/PostChatPromptEndpoint.cs
    Services/           ← OllamaChatAssistant.cs
  Endpoints/            ← PlantPotEndpoints.cs, SensorReadingEndpoints.cs (extension methods)
  Hubs/                 ← SensorHub.cs
  Program.cs
```

---

## Key Files

- `HomeAssistant.Presentation/Program.cs` – sole composition root; all DI registrations and route/hub mappings.
- `HomeAssistant.Presentation/Properties/launchSettings.json` – HTTP `localhost:5064`, HTTPS `localhost:7008`.
- `HomeAssistant.Presentation/HomeAssistant.Presentation.http` – quick manual endpoint tests.
- `docker-compose.yml` – Postgres + Ollama services (at solution root). Use `docker compose up -d postgres ollama` to start locally.
- `.env.example` – Template for environment variable overrides. Copy to `.env` and fill secrets before running.

---

## Developer Workflows

```powershell
# Build entire solution
dotnet build HomeAssistant.sln

# Run API locally (HTTP, Development, uses mock ISensorProvider)
dotnet run --project HomeAssistant.Presentation --launch-profile http

# Add a NuGet package
dotnet add HomeAssistant.Presentation package <PackageName>

# Apply EF Core migration (persistence project)
dotnet ef migrations add <Name> --project HomeAssistant.Infrastructure.Persistence --startup-project HomeAssistant.Presentation
dotnet ef database update          --project HomeAssistant.Infrastructure.Persistence --startup-project HomeAssistant.Presentation
```

---

## Conventions

- **Minimal APIs only** – no MVC controllers. Routes in `Program.cs` or `IEndpointRouteBuilder` extension methods.
- **Typed Results** – all endpoint return types use `Results.Ok<T>()`, `Results.NotFound()`, `Results.Problem()` etc. Annotate with `.Produces<T>()` for OpenAPI.
- **CQRS via Channels** – write operations dispatch `ICommand` through a `Channel<ICommand>` backed by a `SemaphoreSlim` (max 4 concurrent on Pi 5). Read operations call `IQueryHandler<,>` directly (no channel needed).
- **Repository Pattern** – all data access behind interfaces defined in `HomeAssistant.Domain`; implemented in `HomeAssistant.Infrastructure.Persistence`.
- **Mock-first** – register `MockSensorProvider : ISensorProvider` in Development; swap for `Zigbee2MqttSensorProvider` in Production via environment check in `Program.cs`.
- **Dependency Injection throughout** – no `new ConcreteService()` except in tests or factory methods.
- **Composition adapter exception** – `HomeAssistant.Presentation` references only `HomeAssistant.Application` plus the infrastructure composition adapter project; concrete infra/integration registrations live in that adapter so Presentation code stays free of `HomeAssistant.Infrastructure.*` usings.
- **`record` types for DTOs/responses** – defined in `HomeAssistant.Application` or near their endpoint; match TypeScript interface field-for-field.
- **`Nullable` enabled** – all code must be null-safe; use `?` annotations and guard clauses.
- **Async/Await** – no `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`. All async methods accept `CancellationToken ct`.
- **Serilog** – structured logging via Serilog with console + rolling file sink. Inject `ILogger<T>` everywhere; never use `Console.WriteLine`.
- **Metrics** – use `System.Diagnostics.Metrics.Meter` for domain counters (e.g. sensor readings received, agent decisions made).
- **OpenAPI** gated to Development: `app.MapOpenApi()` only inside `if (app.Environment.IsDevelopment())`.
- **Configuration** – non-sensitive config in `appsettings.json`; secrets via environment variables (overriding `appsettings.json`) injected by Docker Compose in production. No secrets committed to source.
- **Ollama** – Semantic Kernel connects to Ollama at `http://ollama:11434` (Docker) or `http://localhost:11434` (local dev), configured via `appsettings.json` `Ollama:BaseUrl` + `Ollama:Model`.
- **Linux-compatible only** – no `System.Drawing`, Windows registry, or Win32 APIs (target: Raspberry Pi 5 / Linux arm64).

---

## Agent Workflow

All feature work follows this loop:

```
Architect (Plan) → Engineer (Implement) → Reviewer (Review) → Git Commit (Commit)
                         ↑                        |
                         |   🔴 Structural issue  |
                         +--------- revised Plan --+
                         |
                         |   🟡 Minor issue → inline fix → Reviewer re-checks → Git Commit
```

| Role | File | Responsibility |
|---|---|---|
| **Architect** | `.github/agents/architect.agent.md` | File-by-file plan, API contracts (C# DTOs + TS interfaces), layer assignments |
| **Engineer** | `.github/agents/engineer.agent.md` | Implementation following the plan; interface-first, XML docs, `.http` updates |
| **Reviewer** | `.github/agents/reviewer.agent.md` | 13-point checklist; classifies findings as Structural (→ re-plan) or Minor (→ inline fix) |
| **Git Commit** | `.github/agents/git-commit.agent.md` | Stages and commits approved changes using Semantic Conventional Commits; enforces safety rules |
| **Orchestrator (optional)** | `.github/agents/orchestrator.agent.md` | Convenience wrapper that coordinates Architect → Engineer → Reviewer → Git Commit while preserving the same gates |

**Start every new feature by invoking the Architect agent first.**

The orchestrator is optional convenience mode and does not replace the canonical four-agent workflow.

---

## Schema Change Policy

**Any pull request that changes the data model — new entity, new property, renamed column, new index, changed relationship, or changed max length — MUST include:**

1. **An EF Core migration** created via:
   ```powershell
   dotnet ef migrations add <MeaningfulName> `
     --project HomeAssistant.Infrastructure.Persistence `
     --startup-project HomeAssistant.Presentation
   ```

2. **A confirmed `database update`** applied to the local development database:
   ```powershell
   dotnet ef database update `
     --project HomeAssistant.Infrastructure.Persistence `
     --startup-project HomeAssistant.Presentation
   ```

3. **A clean `dotnet build HomeAssistant.sln`** after migration creation.

### What does NOT require a migration

- Query logic or repository method changes with no schema impact.
- Configuration or `appsettings.json` changes.
- Adding/removing DI registrations.

### Entity configuration rules

- Keep entity configurations **out of `AppDbContext`**.
- Use one `IEntityTypeConfiguration<T>` file per entity, co-located in the feature folder.
- `AppDbContext.OnModelCreating` must only call `modelBuilder.ApplyConfigurationsFromAssembly(...)`.

> See `.github/instructions/persistence.instructions.md` for the full detail.

---

## Testing

- No test project yet. When adding: `xUnit`, named `HomeAssistant.<Layer>.Tests`.
- Use `MockSensorProvider` and in-memory EF Core (`UseInMemoryDatabase`) for unit tests.
- Integration tests should spin up the full host via `WebApplicationFactory<Program>`.

---

## Docker Deployment (planned)

`docker-compose.yml` at solution root will define:

```
services:
  api          → HomeAssistant.Presentation  (linux/arm64)
  frontend     → React build served by Nginx
  mosquitto    → MQTT broker
  zigbee2mqtt  → Zigbee USB bridge → MQTT
  ollama       → Local LLM server (http://ollama:11434)
```

Secrets injected at runtime via Docker environment variables — never baked into images.

Publish the .NET app as a self-contained linux-arm64 image:
```dockerfile
dotnet publish -r linux-arm64 --self-contained true
```
