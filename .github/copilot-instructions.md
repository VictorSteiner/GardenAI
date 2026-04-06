# Copilot Instructions – GardenAI Project

Welcome! This project uses a **four-agent workflow** for all feature development.


## 🎯 The Four Agents

### 1. **Architect** (`.github/agents/architect.agent.md`)
**When to invoke:** At the start of ANY new feature or structural change.

**Role:** Plan the implementation before any code is written.
- Reads requirements and existing codebase
- Produces a file-by-file breakdown with layer assignments
- Defines API contracts (C# DTOs + TypeScript interfaces)
- Specifies CQRS dispatch strategy and configuration
- Lists logging, metrics, and any blocking questions

**Handoff:** "✅ **Ready for Engineer.**" → proceed to Engineer.

**Blocked:** "⏸ **Blocked – awaiting answers.**" → user must provide clarification.

---

### 2. **Engineer** (`.github/agents/engineer.agent.md`)
**When to invoke:** After the Architect completes and approves a plan.

**Role:** Implement EXACTLY what the Architect specified.
- Follows strict C# and TypeScript rules (interface-first, typed results, CQRS dispatch, etc.)
- Creates/modifies files according to the plan
- Updates `.http` test file for all new endpoints
- Ensures Linux-compatible code (no Windows APIs)
- Never deviates from the plan without asking

**Handoff:** "✅ **Ready for Reviewer.**" → proceed to Reviewer.

---

### 3. **Reviewer** (`.github/agents/reviewer.agent.md`)
**When to invoke:** After the Engineer completes implementation.

**Role:** Audit the implementation against 13-point checklist.
- Verifies architecture layers, CQRS discipline, typed results, DI, async/await, null-handling, Linux compatibility, logging, metrics, configuration, documentation, API contracts, and TypeScript quality
- Classifies findings as:
  - 🔴 **Structural Issue** → back to Architect (requires redesign)
  - 🟡 **Minor Issue** → Engineer applies inline fix (no redesign needed)

**Handoff:** "✅ **Approved**" or "🟡 **Approved with fixes**" → proceed to Git Commit.

---

### 4. **Git Commit** (`.github/agents/git-commit.agent.md`)
**When to invoke:** After the Reviewer approves (✅ or 🟡).

**Role:** Stage and commit all changes using Semantic Conventional Commits.
- Inspects `git status` and `git diff` before touching anything
- Groups changes into atomic, logically cohesive commits
- Applies correct `type(scope): description` format from `.github/instructions/git-commit.instructions.md`
- Enforces safety rules (no secrets, no build artefacts, no `.env` files)
- Verifies the working tree is clean after committing

**Handoff:** "✅ **Committed.**" with a commit summary table.

**Safety halt:** "⚠️ **Commit halted.**" → unexpected files detected, user must review.

---

## 📋 The Workflow Loop

```
                    USER: New feature request
                            ↓
                    [Read Feature Request]
                            ↓
              🏗️ ARCHITECT: Plan phase
              (`.github/agents/architect.agent.md`)
                            ↓
          ✅ Plan complete? → YES
                            ↓
                🧑‍💻 ENGINEER: Implement phase
                (`.github/agents/engineer.agent.md`)
                            ↓
              ✅ All files done? → YES
                            ↓
                👀 REVIEWER: Audit phase
                (`.github/agents/reviewer.agent.md`)
                            ↓
              ┌─────────────┴─────────────┐
              ↓                           ↓
          🟡 Minor Issues            🔴 Structural Issues
          (inline fix)               (back to Architect)
              ↓                           ↓
          🧑‍💻 ENGINEER                🏗️ ARCHITECT
          (apply fixes)              (revise plan)
              ↓                           ↓
              └─────────────┬─────────────┘
                            ↓
                🔖 GIT COMMIT: Commit phase
                (`.github/agents/git-commit.agent.md`)
                            ↓
                    ✅ Committed & merged
```

---

## 🔒 Workflow Gating (Applies to All Chats)

These gates are mandatory even when the user does not explicitly invoke an agent.

1. **No implementation before planning**
   - For any new feature, endpoint, refactor, or structural code change, do not implement code until an Architect plan is present and approved in the chat.
   - Required Architect handoff: `✅ Ready for Engineer.`
   - If missing, stop and request Architect phase first.

2. **Engineer only after approved Architect plan**
   - Engineer work implements the approved plan exactly.
   - Do not invent or skip files, contracts, CQRS flow, DI registrations, logging, or metrics outside the plan.
   - Required Engineer handoff: `✅ Ready for Reviewer.`

3. **Reviewer only after Engineer handoff**
   - Reviewer audit starts only after Engineer completion output is provided.
   - Findings are classified as:
     - `🔴 Structural Issue` → back to Architect for re-plan.
     - `🟡 Minor Issue` → Engineer applies inline fixes without redesign.
   - Merge-ready outcomes: `✅ Approved` or `🟡 Approved with fixes`.

4. **Git Commit only after Reviewer approval**
   - Git Commit phase starts only after one of these Reviewer handoff phrases:
     - `✅ All checks passed. Ready to merge.`
     - `🟡 Approved with inline fixes.`
   - Git Commit agent must inspect `git status` and `git diff` before staging anything.
   - Enforces safety rules: no secrets, no build artefacts, no `.env` files.
   - Required Git Commit handoff: `✅ Committed.`

5. **Structural issue gate**
   - If a structural gap is discovered during Engineer or Reviewer phases, halt implementation and return to Architect.
   - Do not continue with ad-hoc redesign in Engineer mode.

6. **Allowed direct responses (no gate required)**
   - Clarifications, explanations, diagnostics, read-only reviews, and non-structural docs/wording edits may proceed directly.
   - Any request that changes runtime behavior or architecture must re-enter Architect → Engineer → Reviewer.

---

## ⚙️ Optional Orchestrator Convenience Mode

You may optionally use `.github/agents/orchestrator.agent.md` to coordinate the four phases in one guided run.

Important constraints:
- It is a convenience wrapper, not a replacement for the four-agent model.
- It must preserve the same hard gates and handoff phrases.
- Structural issues still return to Architect re-plan.

If used, expected phase order remains:
1. Architect
2. Engineer
3. Reviewer
4. Git Commit

---

## 🚀 How to Use

### **Starting a new feature:**

1. **Call the Architect agent:**
   ```
   "Please plan [feature description]. Read AGENTS.md and any existing files in the affected areas."
   ```
   The Architect will produce a plan covering all layers, files, API contracts, CQRS dispatch, config, logging, and metrics.

2. **Review the plan and approve** (or ask clarifying questions if needed).

3. **Call the Engineer agent:**
   ```
   "Implement this plan: [paste plan]. Follow every rule in .github/agents/engineer.agent.md."
   ```
   The Engineer will create all files, update `.http` tests, and hand off to Reviewer.

4. **Call the Reviewer agent:**
   ```
   "Review all files in this implementation: [paste Engineer output]. Use the 13-point checklist in .github/agents/reviewer.agent.md."
   ```
   The Reviewer will audit against strict rules and either:
   - ✅ Approve (merge ready)
   - 🟡 Approve with fixes (Engineer applies inline corrections)
   - 🔴 Reject (back to Architect for redesign)

5. **Call the Git Commit agent:**
   ```
   "Commit all changes. Reviewer approved. Follow .github/agents/git-commit.agent.md."
   ```
   The Git Commit agent will inspect the working tree, group changes into atomic commits, apply Conventional Commit messages, and confirm a clean working tree.

### **Optional one-call orchestration:**

You can invoke `.github/agents/orchestrator.agent.md` to run the same gated sequence in a single guided flow while preserving Architect → Engineer → Reviewer → Git Commit semantics.

---

## 📖 Key Project Files

- **`AGENTS.md`** – Complete architecture, domain model, conventions, and folder structure
- **`.github/agents/architect.agent.md`** – Plan phase details and output format
- **`.github/agents/engineer.agent.md`** – Implementation rules and patterns
- **`.github/agents/reviewer.agent.md`** – 13-point audit checklist and issue classification
- **`.github/agents/git-commit.agent.md`** – Commit phase rules, safety checks, and handoff format
- **`.github/agents/orchestrator.agent.md`** – Optional convenience wrapper that sequences Architect → Engineer → Reviewer → Git Commit
- **`.github/instructions/*.instructions.md`** – Scoped instruction files with YAML `applyTo` path globs
- **`.github/instructions/folder-organization.instructions.md`** – SOLID-friendly folder separation rules across all projects
- **`.github/instructions/integrations.instructions.md`** – Integration adapter conventions (feature folders, no redundant nesting, one public type per file)
- **`.github/instructions/git-commit.instructions.md`** – Conventional Commits type/scope table, examples, and safety rules
- **`GardenAI.sln`** – Solution file; layer projects added here
- **`GardenAI.Presentation/Program.cs`** – Sole composition root; all DI and routes

### Scoped Instruction Files

Topic-specific instruction files in `.github/instructions/` use the `*.instructions.md` naming pattern and must include YAML frontmatter with `applyTo` globs so Copilot can apply them to the right paths.

---

## 🔧 Agent Capabilities & Access

### **Architect Agent** (Plan Phase)
**Can:**
- ✅ Read all files in the workspace (AGENTS.md, existing code, config files)
- ✅ Analyze domain model and existing layer structure
- ✅ Produce file-by-file breakdowns and API contracts
- ✅ Ask clarifying questions if the plan is blocked

**Cannot:**
- ❌ Write code or create files
- ❌ Run terminals or builds
- ❌ Make decisions without user approval

**Access Required:**
- Read access to: `AGENTS.md`, `GardenAI.sln`, all project files
- Must produce plan output in structured markdown format (see `architect.agent.md` for format)

---

### **Engineer Agent** (Implement Phase)
**Can:**
- ✅ Create and modify files in the workspace (following the Architect's plan exactly)
- ✅ Run `dotnet build` and `dotnet add package` commands to verify compilation
- ✅ Update `.http` test files with new endpoint definitions
- ✅ Apply strict C# and TypeScript conventions from `engineer.agent.md`

**Cannot:**
- ❌ Deviate from the Architect's plan
- ❌ Skip interface definitions
- ❌ Use Windows-specific APIs or paths
- ❌ Proceed if the plan contains structural issues

**Access Required:**
- Write access to: all `.cs`, `.ts`, `.tsx`, `.csproj`, `.http` files
- Execution access to: `dotnet build`, `dotnet add package` commands
- Must follow every rule in `.github/agents/engineer.agent.md` to the letter

---

### **Reviewer Agent** (Review Phase)
**Can:**
- ✅ Read all created/modified files from the Engineer
- ✅ Audit against the 13-point checklist (architecture, CQRS, typed results, DI, async/await, null-handling, Linux compatibility, logging, metrics, config, documentation, API contracts, TypeScript)
- ✅ Classify findings as 🔴 Structural (requires Architect re-plan) or 🟡 Minor (inline fix)
- ✅ Provide corrected code snippets for minor issues

**Cannot:**
- ❌ Apply fixes directly (Engineer does that)
- ❌ Approve structural issues — must require re-planning
- ❌ Skip the checklist

**Access Required:**
- Read access to: all modified files, Architect's plan, `engineer.agent.md`, `AGENTS.md`
- Must produce detailed audit report with file:line citations

---

### **Git Commit Agent** (Commit Phase)
**Can:**
- ✅ Run `git status`, `git diff`, `git add`, `git commit`, and `git log` commands
- ✅ Group changed files into atomic, logically cohesive commits
- ✅ Apply Conventional Commit messages following `.github/instructions/git-commit.instructions.md`
- ✅ Detect and report unsafe files (secrets, artefacts, `.env`) before staging

**Cannot:**
- ❌ Commit before Reviewer approval (`✅ Approved` or `🟡 Approved with fixes`)
- ❌ Use `git add .` without reviewing `git status` first
- ❌ Rewrite shared history without explicit user instruction
- ❌ Proceed if unexpected or dangerous files are present in the working tree

**Access Required:**
- Execution access to: `git status`, `git diff`, `git add`, `git commit`, `git log`
- Read access to: `AGENTS.md`, `.github/instructions/git-commit.instructions.md`, Reviewer output

---

## 🛠️ Quick Reference: Common Commands

```powershell
# Build solution
dotnet build GardenAI.sln

# Run API locally (Development, mock sensors)
dotnet run --project GardenAI.Presentation --launch-profile http

# Add a new layer project
dotnet new classlib -n GardenAI.<LayerName> -f net10.0
dotnet sln add GardenAI.<LayerName>/GardenAI.<LayerName>.csproj

# Add a NuGet package
dotnet add GardenAI.Presentation package <PackageName>

# Run EF Core migrations
dotnet ef migrations add <Name> `
  --project GardenAI.Infrastructure `
  --startup-project GardenAI.Presentation
dotnet ef database update `
  --project GardenAI.Infrastructure `
  --startup-project GardenAI.Presentation
```

---

## 🎓 Design Principles

1. **Clean Architecture** – Domain → Application → Infrastructure → Presentation
2. **CQRS via Channels** – Commands dispatched through `CommandDispatcher` (bounded channel + semaphore), Queries called directly
3. **Interface First** – Every concrete class has a corresponding interface in Domain/Application
4. **Mock-First Sensors** – `ISensorProvider` swappable: `MockSensorProvider` in Development, `Zigbee2MqttSensorProvider` in Production
5. **Minimal APIs** – No MVC controllers; all HTTP surface in `Program.cs` or `IEndpointRouteBuilder` extensions
6. **Typed Results** – All endpoints return `Results.Ok<T>()`, `Results.NotFound()`, etc. with `.Produces<T>()` annotations
7. **Dependency Injection** – No `new ConcreteService()` except in tests/factories; all registered in `Program.cs`
8. **Async/Await** – No `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`; all methods accept `CancellationToken ct`
9. **Null Handling** – Nullable reference types are disabled; use guard clauses and explicit validation at every entry point
10. **Linux-Compatible** – No Windows APIs; target: Raspberry Pi 5 arm64

---

## ❓ Questions?

Refer to the specific agent file for detailed rules:
- **Architecture questions** → `.github/agents/architect.agent.md`
- **C# / TypeScript implementation questions** → `.github/agents/engineer.agent.md`
- **Code quality / compliance questions** → `.github/agents/reviewer.agent.md`
- **Commit message / git workflow questions** → `.github/agents/git-commit.agent.md`
- **General architecture / conventions** → `AGENTS.md`
