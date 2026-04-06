---
applyTo: "GardenAI.Integrations.*/**/*.cs,GardenAI.Integrations.*/**/*.csproj,GardenAI.Integrations.*/README.md"
---

# Integrations Instructions

## Scope

These rules apply to external provider adapters under `GardenAI.Integrations.*` projects.

## Folder and Namespace Conventions

- Use feature folders under the integration project (for example: `Forecast/`, `Plants/`).
- Avoid redundant nested folders that duplicate the project name (for example: `GardenAI.Integrations.OpenMeteo/OpenMeteo/`).
- Match namespaces to folder structure.
- Within a feature, separate abstractions, contracts, clients, configuration objects, and exceptions into distinct subfolders when more than one responsibility exists.

## File Granularity

- Keep one public type per file.
- The filename must match the type name.
- Exceptions: extension-method containers (`*Extensions.cs`) may include multiple related extension members.

## Integration Design Rules

- Keep integrations as standalone adapter projects unless explicitly wired into Application/Presentation.
- Use interface-first contracts for client seams.
- Keep all calls async and accept `CancellationToken ct`.
- Do not hardcode secrets; use options/configuration and environment overrides.
- Preserve Linux compatibility and avoid Windows-only APIs.

## Suggested File Shape

For each integration capability, prefer:

- `Abstractions/I<Provider><Capability>Client.cs`
- `Clients/<Provider><Capability>Client.cs`
- `Contracts/<Provider><Capability>Request.cs`
- `Contracts/<Provider><Capability>Response.cs`
- `Exceptions/<Provider>ApiException.cs`
- `Configuration/<Provider>ClientOptions.cs`

