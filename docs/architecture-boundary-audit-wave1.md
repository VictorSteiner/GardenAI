# Architecture Boundary Audit - Wave 1

This audit was created for issue #1 and focuses on current layer/reference boundaries against repository rules in `AGENTS.md` and `.github/agents/architect.agent.md`.

## Scope

- Solution-level project references (`*.csproj`)
- High-risk service placement in Presentation layer
- Conformance to stated Clean Architecture direction: Domain -> Application -> Infrastructure -> Presentation

## Findings (ordered by severity)

## 1) Presentation directly references Infrastructure/Integrations (Structural)

**Evidence**

`HomeAssistant.Presentation/HomeAssistant.Presentation.csproj`:
- line 16 -> `HomeAssistant.Infrastructure.Messaging`
- line 17 -> `HomeAssistant.Infrastructure.Persistence`
- line 18 -> `HomeAssistant.Infrastructure.Sensors`
- line 19 -> `HomeAssistant.Integrations.OpenMeteo`
- line 20 -> `HomeAssistant.Infrastructure.HomeAssistant`

**Why this is a boundary problem**

Repository guidance states Presentation should not depend directly on Infrastructure. Current references tightly couple HTTP host code to adapter implementations.

**Impact**

- Harder substitution/testing of adapters
- Higher ripple effect when infrastructure changes
- Layering contract is no longer enforceable by compiler

**Follow-up issue**

- #7 `tech-debt: remove direct infrastructure references from presentation project`

## 2) Use-case orchestration lives in Presentation services (Structural)

**Evidence**

`HomeAssistant.Presentation/GardenAdvisor/Services/GardenAdvisorService.cs`:
- lines 53-95: orchestration of sensor/weather/chat pipeline in Presentation service
- lines 102-167: room-context orchestration logic in Presentation service

`HomeAssistant.Presentation/GardenAdvisor/Services/GardenPlannerService.cs`:
- lines 70-113: planner turn orchestration and business sequencing in Presentation
- lines 115-136: domain prompt composition logic
- lines 228-240: tool dispatch orchestration path

**Why this is a boundary problem**

These are application use-case concerns and should primarily live in `HomeAssistant.Application`, with Presentation focused on transport concerns (endpoint mapping/contracts).

**Impact**

- HTTP layer becomes difficult to reason about and test
- Business behavior is coupled to web host project
- Application layer underused

**Follow-up issue**

- #8 `tech-debt: move GardenAdvisor orchestration logic from presentation to application layer`

## 3) Hardcoded Pot GUID mapping in Presentation (Major)

**Evidence**

`HomeAssistant.Presentation/GardenAdvisor/Services/GardenPlannerService.cs`:
- lines 20-29: static `PotNumberToId` dictionary with fixed GUID constants

**Why this is a boundary problem**

Identifier mapping is environment/data concern; hardcoding in Presentation risks drift and breaks portability.

**Impact**

- Production mismatch risk if pot IDs differ
- brittle behavior during migration/reset
- harder setup for multi-instance deployments

**Follow-up issue**

- #9 `tech-debt: replace hardcoded pot identifier map with persisted/configured source`

## 4) Home Assistant adapter project structure does not align to integration conventions (Moderate)

**Evidence**

Current adapter project name: `HomeAssistant.Infrastructure.HomeAssistant`

Repository integration guidance favors dedicated integration adapter conventions and consistent placement/naming.

**Why this matters**

This is primarily an organizational/maintainability boundary issue rather than an immediate runtime defect.

**Follow-up issue**

- #10 `tech-debt: align Home Assistant protocol adapter project with integrations conventions`

## Suggested Refactor Order

1. **#7** - enforce top-level project boundaries first
2. **#8** - move orchestration into Application layer
3. **#9** - remove hardcoded identifier map
4. **#10** - finalize naming/folder convention alignment

## Quick Wins

- Keep endpoint handlers in Presentation, but move orchestration classes behind Application abstractions.
- Introduce interfaces in Application for weather/messaging adapters; register concrete infra implementations in composition root.
- Replace hardcoded pot map with repository/config query and validation.

## Completion Mapping

Issue #1 acceptance criteria required:
- report committed under `docs/` -> **Done** (`docs/architecture-boundary-audit-wave1.md`)
- each violation mapped to follow-up issue -> **Done** (#7, #8, #9, #10)

