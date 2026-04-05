---
applyTo: "**"
---

# Git Commit Instructions – Conventional Commits

This project uses **Semantic Conventional Commits** (based on [conventionalcommits.org v1.0](https://www.conventionalcommits.org/en/v1.0.0/)) for all commits.

---

## Commit Message Format

```
<type>(<scope>): <short description>

[optional body]

[optional footer(s)]
```

### Rules

- Subject line (first line) **must not exceed 72 characters**.
- Use **imperative mood** in the subject: "add", "fix", "remove" — not "added", "fixes", "removed".
- Do **not** capitalise the first word of the description.
- Do **not** end the subject line with a period.
- Separate subject from body with a blank line.
- Wrap body text at 100 characters per line.
- The body should explain **why**, not just what.

---

## Types

| Type | When to use |
|---|---|
| `feat` | A new user-visible feature or capability |
| `fix` | A bug fix |
| `refactor` | Code change that is neither a feature nor a bug fix |
| `docs` | Documentation only (markdown, XML docs, `.md` files, `.http` comments) |
| `test` | Adding or correcting tests (xUnit, integration tests) |
| `chore` | Build scripts, tooling, dependency bumps, project file changes with no runtime impact |
| `ci` | CI/CD workflow changes (GitHub Actions, Docker, deployment) |
| `perf` | Performance improvement |
| `style` | Formatting, whitespace — no logic change |
| `build` | Changes to the build system or external dependencies (`.csproj`, `Directory.Packages.props`) |
| `revert` | Reverts a previous commit |

---

## Scopes

Use the **project layer or feature area** as the scope. Omit scope only for truly cross-cutting changes.

| Scope | Applies to |
|---|---|
| `domain` | `HomeAssistant.Domain/**` |
| `application` | `HomeAssistant.Application/**` |
| `persistence` | `HomeAssistant.Infrastructure.Persistence/**` |
| `sensors` | `HomeAssistant.Infrastructure.Sensors/**` |
| `messaging` | `HomeAssistant.Infrastructure.Messaging/**` |
| `openmeteo` | `HomeAssistant.Integrations.OpenMeteo/**` |
| `presentation` | `HomeAssistant.Presentation/**` (API layer, routes, hubs) |
| `frontend` | `frontend/**` |
| `config` | `appsettings*.json`, `.env*`, `docker-compose.yml`, `mosquitto/**` |
| `agents` | `.github/agents/**` |
| `instructions` | `.github/instructions/**` |
| `ci` | `.github/workflows/**` |
| `deps` | `Directory.Packages.props`, `*.csproj` package version bumps |

Multiple scopes are written as comma-separated values inside the parentheses: `feat(domain,application):`.

---

## Breaking Changes

Two equivalent ways to mark a breaking change:

```
feat(domain)!: rename PlantPot.Label to PlantPot.Name

BREAKING CHANGE: The `Label` property on `PlantPot` has been renamed to `Name`.
Update all callers and EF Core configurations accordingly.
```

Or inline:

```
feat(domain)!: rename PlantPot.Label to PlantPot.Name
```

Always include a `BREAKING CHANGE:` footer with migration notes when a breaking change is made.

---

## Atomic Commits

Each commit must represent **one cohesive logical change**.

- If a feature touches Domain + Application + Presentation, one `feat` commit covering the full vertical slice is preferred.
- If Infrastructure (migration) is a separate mechanical step, it may be a separate `chore(persistence)` or `feat(persistence)` commit.
- Never mix unrelated changes (e.g. a bug fix and a refactor) in the same commit.
- If multiple independent features or fixes were worked on, split into multiple commits.

---

## Examples

```
feat(domain): add PlantSpecies entity and repository interface
```

```
feat(application,presentation): add GetAllPlantPots query and endpoint
```

```
fix(sensors): correct soil-moisture unit conversion from raw ADC value
```

```
refactor(application): extract CommandDispatcher into dedicated Dispatching folder
```

```
docs(presentation): add XML summary docs to sensor reading endpoint handlers
```

```
chore(persistence): add EF Core migration for PlantSpecies table
```

```
build(deps): bump Microsoft.EntityFrameworkCore to 10.0.1
```

```
feat(frontend): add sensor dashboard page with real-time SignalR updates
```

```
ci: add Docker Compose health-check for postgres service
```

---

## What Must Never Be Committed

- Secrets, API keys, passwords, or connection strings with credentials.
- `.env` files (only `.env.example` is allowed).
- `bin/`, `obj/`, `node_modules/`, `.vs/` build artefacts.
- Database files (`*.db`, `*.db-shm`, `*.db-wal`) — already in `.gitignore`.
- Any file containing `TODO: remove before commit` markers.

Always run `git status` and review `git diff --staged` before confirming commits to catch accidental inclusions.

---

## Commit Sequence for a Full Feature

When a complete feature goes through the Architect → Engineer → Reviewer → Commit pipeline, the typical commit sequence is:

1. **Domain changes** – `feat(domain):`
2. **Application changes** – `feat(application):`
3. **Infrastructure / migration** – `feat(persistence):` or `chore(persistence):`
4. **Presentation changes** – `feat(presentation):`
5. **Frontend changes** – `feat(frontend):` (if applicable)
6. **Documentation / instruction updates** – `docs:` or `docs(agents):`

These may be squashed into a single `feat(<scope>):` commit if they form one indivisible unit, but only if the result still reads clearly in `git log --oneline`.

---

## See Also

- [Conventional Commits specification v1.0](https://www.conventionalcommits.org/en/v1.0.0/)
- `AGENTS.md` – Domain model and layer responsibilities
- `.github/agents/git-commit.agent.md` – The Git Commit Agent rules

