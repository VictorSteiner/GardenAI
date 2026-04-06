---
applyTo: "GardenAI.Domain/**/*.cs,GardenAI.Application/**/*.cs,GardenAI.Infrastructure.Persistence/**/*.cs,GardenAI.Infrastructure.Sensors/**/*.cs,GardenAI.Integrations.*/**/*.cs,GardenAI.Presentation/**/*.cs"
---

# Folder Organization Instructions

## Goal

Keep the codebase readable and SOLID-friendly by separating files by responsibility within each feature.

## Core Rules

- Organize code by feature first, then by responsibility.
- Do not mix service implementations, DTO/contracts, interfaces, repositories, configuration types, providers, clients, and exceptions in the same folder when dedicated subfolders would improve clarity.
- Keep one public type per file.
- The filename must match the type name.
- Place feature-local contracts next to the owning feature flow (endpoint/command/query/service).
- Keep only truly reused contracts in shared `Contracts/` folders.

## Preferred Subfolders

Use these subfolders when a feature contains multiple responsibility kinds:

- `Abstractions/` – interfaces and abstraction seams
- `Contracts/` – request/response DTOs, records, payload contracts
- `Services/` – service implementations
- `Repositories/` – persistence repository implementations
- `Configurations/` – EF Core entity type configurations and other mapping/configuration classes
- `Providers/` – provider implementations such as sensor providers
- `Clients/` – HTTP/API client implementations
- `Configuration/` – options classes and provider-specific configuration objects
- `Exceptions/` – custom exception types

## Practical Guidance by Layer

### Domain
- Keep domain concepts feature-grouped.
- Prefer `Entities/` and `Abstractions/` subfolders when a domain feature contains both models and contracts.
- In shared domain infrastructure such as `Common/`, separate marker interfaces from handler contracts using folders such as `Markers/` and `Handlers/`.
- Do not keep growing flat domain feature folders once different responsibilities are present.

### Application
- Separate abstractions from implementations.
- Commands, queries, handlers, dispatchers, and agent implementations should not be dumped into one folder when subfolders improve clarity.
- For features with multiple commands or queries, prefer one folder per command/query so related request, handler, validator, and mapping files stay together.
- For contract-heavy features, split `Contracts/` by flow (for example `Contracts/Completions`, `Contracts/Agentic`, `Contracts/Advice`) and avoid flat catch-all contract folders.

### Infrastructure
- Separate repositories from EF configurations.
- Separate providers/clients from options and exceptions.
- Keep database context and database-specific infrastructure in a dedicated database/context folder instead of the project root when the project has multiple feature folders.

### Presentation
- Separate endpoint contracts from service implementations and abstractions.
- Avoid placing request/response contracts in the same folder as concrete service implementations unless the feature is still trivially small.
- When a feature exposes multiple HTTP endpoints across distinct domain intents, decompose into **domain slices** (see Domain-Sliced Endpoint Structure below).
- Place each endpoint in its own folder under `Endpoints/<EndpointName>/` instead of flattening multiple endpoint files into one folder.
- Place endpoint-specific request/response contracts under the endpoint folder, for example `Endpoints/<EndpointName>/Contracts/`.
- Keep shared presentation contracts only for models reused across multiple endpoints.
- Internal AI tool / protocol function endpoints belong in a dedicated `ProtocolTools/` slice — do not mix them into domain-facing endpoint folders.

## Domain-Sliced Endpoint Structure

When a presentation feature grows to span distinct domain responsibilities (for example: planning, pot management, room queries, advice, internal tools), split it into **domain slices** rather than broadening a single feature folder.

### Folder Pattern

Each domain slice owns its routes, endpoints, and feature-local contracts:

```text
GardenAI.Presentation/<Feature>/
  RouteBuilders/
    <Feature>RouteBuilder.cs            ? aggregator: calls each slice's Map*Routes()
    <Slice1>RouteBuilder.cs             ? e.g. GardenPlanningRouteBuilder.cs
    <Slice2>RouteBuilder.cs             ? e.g. PotManagementRouteBuilder.cs
    <Slice3>RouteBuilder.cs             ? e.g. ProtocolToolsRouteBuilder.cs
  <Slice1>/
    Endpoints/
      <EndpointName>/
        <EndpointName>Endpoint.cs
        Contracts/
          <Request>.cs
          <Response>.cs
    Contracts/                          ? contracts shared across this slice only
    RouteBuilders/                      ? slice-internal route builder (optional)
  <Slice2>/
    ...
  Contracts/                            ? truly cross-slice shared contracts only
  Abstractions/                         ? service interfaces owned by the feature
  Services/                             ? service implementations
```

### Real Example (GardenAdvisor)

```text
GardenAI.Presentation/GardenAdvisor/
  RouteBuilders/
    GardenAdvisorRouteBuilder.cs        ? aggregator; calls all slice Map*Routes()
    GardenPlanningRouteBuilder.cs
    PotManagementRouteBuilder.cs
    RoomInsightsRouteBuilder.cs
    GardenInsightsRouteBuilder.cs
    ProtocolToolsRouteBuilder.cs        ? internal AI tool endpoint surface
  GardenPlanning/
    Endpoints/
      PostGardenPlannerChat/
        PostGardenPlannerChatEndpoint.cs
        Contracts/
          GardenPlannerChatRequest.cs
          GardenPlannerChatResponse.cs
  PotManagement/
    Endpoints/
      PostSavePotConfiguration/
      PostUpdateSeedStatus/
    Contracts/
      PotConfigurationResponse.cs       ? shared within PotManagement slice
  RoomInsights/
    Endpoints/
      GetAvailableRooms/
      GetRoomSummary/
  GardenInsights/
    Endpoints/
      GetDashboard/
      GetHarvestReadiness/
  GardenAdvice/
    Endpoints/
      GetLatestGardenAdvice/
      PostGenerateGardenAdvice/
  Endpoints/
    ProtocolTools/
      GardenPlannerToolEndpoints.cs     ? AI-invokable tool endpoint surface
      Contracts/
        SavePotConfigurationRequest.cs
        PotNumberRequest.cs
        ...
  Contracts/                            ? cross-slice shared contracts
    DashboardAggregationResponse.cs
    RoomSummaryResponse.cs
  Abstractions/
    IGardenPlannerService.cs
    IGardenPlannerToolService.cs
  Services/
    GardenPlannerService.cs
    GardenPlannerToolService.cs
```

### Route Builder Convention

- The **aggregator route builder** (e.g. `GardenAdvisorRouteBuilder`) is the only entry point called from `MiddlewareConfiguration.MapRoutes()`.
- Each **slice route builder** registers one logical domain group and calls `endpoints.MapGroup(...)`.
- Naming: `Map<DomainSlice>Routes()` extension method on `IEndpointRouteBuilder`.
- The `ProtocolTools` route builder registers AI-tool endpoints under a stable internal path (e.g. `/api/garden/planner/functions/`). These are not user-facing domain endpoints.

### When to Create a New Slice

Create a new domain slice when:
- A new endpoint group has a distinct domain owner (e.g. "room queries" vs "pot config writes").
- An existing route builder file exceeds ~60 lines or maps more than ~5 unrelated endpoint families.
- Adding a new endpoint would require touching an unrelated existing route group.

Do **not** create a slice for a single endpoint — use a simple `Endpoints/<EndpointName>/` folder directly.

## Contract Placement Examples

```text
GardenAI.Presentation/GardenAdvisor/
  GardenPlanning/
    Endpoints/
      PostGardenPlannerChat/
        Contracts/
          GardenPlannerChatRequest.cs    ? endpoint-local contract
          GardenPlannerChatResponse.cs
  Endpoints/
    ProtocolTools/
      Contracts/
        PotNumberRequest.cs              ? ProtocolTools-local contract
  Contracts/
    DashboardAggregationResponse.cs      ? reused across GardenInsights + ProtocolTools

GardenAI.Application/Chat/
  Contracts/
    Completions/
      ChatCompletionRequest.cs
    Agentic/
      AgenticChatResult.cs
      ChatFunctionCall.cs
```

## SOLID Guardrails

Apply these checks during planning, implementation, and review.

### Single Responsibility (SRP)

- A service should have one primary reason to change (one business workflow family).
- Avoid mixing orchestration, persistence mapping, transport formatting, and protocol concerns in one class.
- If a class has clearly distinct sections for unrelated workflows, split it into focused services.

### Interface Segregation (ISP)

- Prefer small, purpose-focused interfaces over broad "do everything" interfaces.
- Consumers should not depend on methods they do not call.
- Split read and write responsibilities when callers only need one side.

### Dependency Inversion (DIP)

- Core workflows depend on abstractions (`I...`) rather than concrete infrastructure types.
- Add a new abstraction seam when logic must be testable independent of transport/provider details.

## Refactor Triggers

Treat these as practical thresholds that trigger a split proposal:

- A service grows beyond ~250 lines and contains multiple workflow domains.
- An interface exceeds ~10 methods or has methods used by different caller groups.
- A folder mixes 3+ responsibility kinds (for example services + contracts + provider client glue).
- New feature work requires touching unrelated methods in the same class to avoid regressions.
- A constructor requires 8+ dependencies, suggesting orchestration concerns are over-aggregated.

When a trigger is hit, create/refine sub-issues and split by business capability rather than by technical layer names alone.

## Service Splitting Example

```text
Before
GardenPlannerService
  - chat orchestration
  - tool execution routing
  - history formatting
  - MQTT publish payload shaping

After
GardenPlannerChatService         # prompt and chat loop orchestration
GardenPlannerToolRouter          # tool-call dispatching
GardenPlannerHistoryPublisher    # history payload publishing
GardenPlannerResponsePublisher   # planner response publishing
```

## Interface Sizing Example

```text
Before
IGardenPlannerToolService (12 methods spanning commands, queries, and advice)
  - save/update commands
  - room queries
  - dashboard queries
  - advice generation

After
IGardenPlannerCommandService   ? save/update pot workflows
IGardenPlannerQueryService     ? pot/room/dashboard read workflows
IGardenPlannerAdviceService    ? advice generation and retrieval
```

## Exceptions

These are acceptable exceptions when they improve clarity rather than reduce it:

- `*Extensions.cs` files may contain multiple related extension methods.
- Tiny, highly cohesive domain feature folders may keep closely related domain types together.
- A feature with only one type does not need artificial subfolders.

