---
applyTo: "GardenAI.Infrastructure.Persistence/**/*.cs,GardenAI.Presentation/Program.cs,GardenAI.Presentation/appsettings*.json"
---

# Persistence & EF Core Instructions

## Scope

These rules apply whenever persistence models, `DbContext` mappings, repository behavior, or connection-related configuration changes.

## Required Migration Workflow

When schema-affecting changes are made (new entity, property, index, relationship, key, max length, nullable change):

1. Create an EF Core migration in the persistence project.
2. Apply the migration to the target database.
3. Verify the solution builds after migration creation.

Use:

```powershell
dotnet ef migrations add <MeaningfulName> --project GardenAI.Infrastructure.Persistence --startup-project GardenAI.Presentation
dotnet ef database update --project GardenAI.Infrastructure.Persistence --startup-project GardenAI.Presentation
```

## Non-Schema Changes

If only query logic/repository logic changes and schema is untouched:

- No migration is needed.
- Still run `dotnet build GardenAI.sln`.

## Mapping Organization

- Keep entity mappings out of `AppDbContext` when possible.
- Use one `IEntityTypeConfiguration<T>` class per entity near its feature folder.
- Keep `AppDbContext.OnModelCreating` minimal (`ApplyConfigurationsFromAssembly`).

## Review Checklist

- Migration file exists for schema changes.
- `dotnet ef database update` was executed (or failure reason documented).
- No direct SQL schema edits committed without migration.
- `AppDbContext` remains compact and readable.

