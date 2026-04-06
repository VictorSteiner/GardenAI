# GardenAI – Agent Guide

## Project Vision

A **lightweight, modular platform** running on a Raspberry Pi 5 powered by local LLMs (via Semantic Kernel + Ollama).
The system demonstrates clean architecture principles, CQRS dispatch patterns, and extensible integration adapters.

This is a **reference implementation** of scalable ASP.NET Core design on embedded hardware.

---

## Solution Architecture (Clean Architecture)

The project follows **Clean Architecture** with strict layer separation:

| Layer | Projects | Responsibility |
|---|---|---|
| Presentation | `GardenAI.Presentation` | HTTP API surface, request/response contracts, composition root |
| Application | `GardenAI.Application` | Use case orchestration, CQRS dispatch, service configuration |
| Domain | `GardenAI.Domain` | Business entities, repository interfaces, CQRS marker abstractions |
| Infrastructure | `GardenAI.Infrastructure.*` | Persistence, external integrations, concrete implementations |

### Layer Rules (Strict)

1. **Presentation** may reference Application and Domain (never Infrastructure directly)
2. **Application** may reference Domain only
3. **Domain** has zero external dependencies (pure business logic)
4. **Infrastructure** implements Domain/Application interfaces only (no upward references)

---

## Domain-Driven Design Approach

### Feature-Based Organization

Organize code by **domain concern**, not by technical type:

**? Good:**
```
Feature/
  Entities/          ? core business objects
  Abstractions/      ? repository/service contracts
  Commands/          ? domain write operations
  Queries/           ? domain read operations
```

**? Avoid:**
```
Entities/
Repositories/
Services/
Commands/
Queries/
```

### No Implementation Details in Documentation

Documentation describes **principles and contracts**, never "put this file in that folder." Implementations may vary based on project needs.

---

## CQRS Pattern (via Channels)

### Command Dispatch
- Commands are **write operations** that change state
- All commands flow through a bounded channel with semaphore-controlled concurrency (max 4 concurrent operations on Pi 5)
- Command handlers execute sequentially within the channel
- Returns completion status (success/failure), not domain data

### Query Dispatch
- Queries are **read operations** that fetch data without side effects
- Query handlers are called **directly** (no channel needed)
- May be called in parallel by multiple request threads
- Returns typed result data

### CQRS Marker Interfaces
```csharp
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

---

## Repository Pattern

All data access is behind **repository interfaces**:

```csharp
// Define in Domain
public interface IMyRepository
{
    Task<MyEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MyEntity>> GetAllAsync(CancellationToken ct = default);
    Task<MyEntity> CreateAsync(MyEntity entity, CancellationToken ct = default);
    Task UpdateAsync(MyEntity entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

// Implement in Infrastructure
public sealed class MyRepository : IMyRepository { ... }
```

**Key Principle:** Repository interfaces live in Domain, implementations live in Infrastructure.

---

## Full Stack

| Concern | Technology | Purpose |
|---------|-----------|---------|
| Backend | ASP.NET Core Minimal API (.NET 10) | HTTP surface |
| Database | EF Core 10 + PostgreSQL | Persistence |
| ORM | Entity Framework Core | Data mapping |
| CQRS | System.Threading.Channels + SemaphoreSlim | Command dispatch |
| Logging | Serilog | Structured logging |
| Metrics | System.Diagnostics.Metrics | Built-in observability |
| AI/LLM | Semantic Kernel + Ollama | Local language models |
| Frontend | React + TypeScript + TanStack | Web UI |
| Deployment | Docker Compose | Container orchestration |

---

## Conventions

- **Minimal APIs only** – no MVC controllers; map routes in extension methods
- **Typed Results** – all endpoints return `Results.Ok<T>()`, `Results.NotFound()`, etc. with `.Produces<T>()`
- **Repository Pattern** – inject interfaces, never `new` concrete classes
- **`record` types for DTOs** – define request/response models as immutable records
- **Null Handling** – nullable reference types are disabled; enforce guard clauses and explicit validation at boundaries
- **Async/Await throughout** – no `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`
- **Dependency Injection only** – all services resolved via constructor injection
- **Configuration via DI** – options classes bound from `IConfiguration` in Program.cs
- **Serilog for logging** – `ILogger<T>` injected everywhere, never `Console.WriteLine`
- **Metrics via Meter** – track domain events with built-in `System.Diagnostics.Metrics`
- **OpenAPI in Development only** – gates Swagger/Scalar behind environment check
- **Linux-compatible** – no Windows APIs; target Linux arm64 (Raspberry Pi)

---

## Agent Workflow

All feature work follows this gated pipeline:

```
Architect (Plan) ? Engineer (Implement) ? Reviewer (Audit) ? Git Commit
     ?                                           |
     |                    ?? Structural Issue    |
     +------- back to Architect ----------------+
     
     |                    ?? Minor Issue --? Engineer (inline fix) --? Reviewer --? Git Commit
```

| Role | File | Responsibility |
|------|------|---|
| **Architect** | `.github/agents/architect.agent.md` | Feature plan, file-by-file breakdown, API contracts, layer assignments |
| **Engineer** | `.github/agents/engineer.agent.md` | Implement exactly per plan, follow conventions, interface-first, no deviations |
| **Reviewer** | `.github/agents/reviewer.agent.md` | Audit 13-point checklist, classify issues (structural vs minor), approve or reject |
| **Git Commit** | `.github/agents/git-commit.agent.md` | Stage changes, atomic commits, Conventional Commit messages, safety checks |

**Gate:** No implementation begins until Architect plan is approved. No review until Engineer is complete. No commit until Reviewer approves.

---

## Schema Change Policy

### When a migration is required

Any change to persistence schema (new entity, new column, renamed column, changed relationships, index changes) **MUST** include:

1. **EF Core migration** created and tested
2. **Migration applied** to development database
3. **Clean build** after migration creation

### When a migration is NOT required

- Query/repository logic changes with no schema impact
- Configuration or options changes
- DI registration changes
- Service implementations

---

## Testing Strategy

- **Unit tests:** Mock repositories and external services
- **Integration tests:** Spin up full host via `WebApplicationFactory<Program>`
- **In-memory persistence:** Use EF Core `UseInMemoryDatabase` for unit testing
- **Docker for local dev:** Use docker-compose to run real PostgreSQL + Ollama locally

---

## Deployment Model

Single docker-compose.yml defines all services:
- API service (linux/arm64 self-contained publish)
- Frontend service (Nginx serving React build)
- External services (PostgreSQL, Ollama, message brokers, etc.)
- No secrets in images; inject via environment variables at runtime

---

## Key Principles (Non-Negotiable)

1. **Clean Architecture Layers** – strict separation, downward-only dependencies
2. **Interface-First Design** – define contracts before implementations
3. **Dependency Injection** – compose everything in one place (Program.cs)
4. **CQRS Discipline** – write operations via commands, read operations via queries
5. **Repository Pattern** – all data access behind interfaces
6. **No `new ConcreteService()`** – except in Program.cs, tests, or static factories
7. **Async/Await Throughout** – no blocking calls
8. **Null Safety** – nullable reference types disabled; use guard clauses and explicit validation at all boundaries
9. **Serilog Everywhere** – structured logging, no Console.WriteLine
10. **Testability First** – interfaces enable mocking and substitution

---

## See Also

- **`.github/agents/architect.agent.md`** – Planning phase rules and templates
- **`.github/agents/engineer.agent.md`** – Implementation rules and patterns
- **`.github/agents/reviewer.agent.md`** – 13-point audit checklist
- **`.github/agents/git-commit.agent.md`** – Commit message conventions
- **`.github/instructions/architecture.instructions.md`** – Layer rules and patterns
- **`.github/instructions/dependency-injection.instructions.md`** – DI composition patterns
- **`.github/instructions/interface-first.instructions.md`** – Contract definition principles
