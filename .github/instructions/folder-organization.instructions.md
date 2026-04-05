---
applyTo: "HomeAssistant.Domain/**/*.cs,HomeAssistant.Application/**/*.cs,HomeAssistant.Infrastructure.Persistence/**/*.cs,HomeAssistant.Infrastructure.Sensors/**/*.cs,HomeAssistant.Integrations.*/**/*.cs,HomeAssistant.Presentation/**/*.cs"
---

# Folder Organization Instructions

## Goal

Keep the codebase readable and SOLID-friendly by separating files by responsibility within each feature.

## Core Rules

- Organize code by feature first, then by responsibility.
- Do not mix service implementations, DTO/contracts, interfaces, repositories, configuration types, providers, clients, and exceptions in the same folder when dedicated subfolders would improve clarity.
- Keep one public type per file.
- The filename must match the type name.

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

### Infrastructure
- Separate repositories from EF configurations.
- Separate providers/clients from options and exceptions.
- Keep database context and database-specific infrastructure in a dedicated database/context folder instead of the project root when the project has multiple feature folders.

### Presentation
- Separate endpoint contracts from service implementations and abstractions.
- Avoid placing request/response contracts in the same folder as concrete service implementations unless the feature is still trivially small.
- When a feature exposes multiple HTTP endpoints, introduce a `RouteBuilders/` folder with a `<Domain>RouteBuilder` entry point.
- Place each endpoint in its own folder under `Endpoints/<EndpointName>/` instead of flattening multiple endpoint files into one folder.

## Exceptions

These are acceptable exceptions when they improve clarity rather than reduce it:

- `*Extensions.cs` files may contain multiple related extension methods.
- Tiny, highly cohesive domain feature folders may keep closely related domain types together.
- A feature with only one type does not need artificial subfolders.

