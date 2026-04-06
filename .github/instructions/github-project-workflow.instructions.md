---
applyTo: "**/*"
---

# GitHub Project Workflow Instructions

Use this repository in a strict task-first mode.

## Core Rule

- Do not implement non-trivial changes unless there is an existing GitHub issue/task for the work.
- Any newly created issue or sub-issue must be added to the repository GitHub Project (`Garden AI`) immediately after creation.
- Every implementation PR must link an issue using one of:
  - `Closes #<id>`
  - `Fixes #<id>`
  - `Resolves #<id>`
  - `Refs #<id>`
- If you forget to add this explicitly, use branch names containing the issue number (for example `feature/123-chat-persistence`); CI will auto-append `Refs #123`.

## Parent/Sub-Issue Model

- Use one parent issue for each investigation or initiative.
- Convert remediation work into sub-issues of that parent.
- Keep parent open until all sub-issues are closed.

## Investigation Output Policy

For investigation tasks, default output location is the parent issue itself:

1. Append findings summary directly to parent issue description or comments.
2. Map each finding to a concrete follow-up sub-issue.
3. Avoid creating standalone markdown reports unless explicitly requested or needed for long-lived documentation.

If a temporary markdown report is created, create a cleanup task to remove it after remediation is complete.

## Labels and Priorities

Use standard labels:

- Work type: `feature`, `technical-debt`, `bug`, `refactor`, `docs`, `ci`, `chore`
- Priority: `priority-high`, `priority-medium`, `priority-low`
- Approval state when needed: `needs-approval`, `approved`

## Feature Approval Gate

For feature tasks:

- Set `needs-approval` until owner approves.
- Only move feature tasks to `Todo` when approved.
- Use project field `Approval` with values `Pending Approval`, `Approved`, or `Rejected`.

## Definition of Done

A task is done only when all are true:

- Code/docs changes are merged
- Related issue is closed
- Parent issue progress is updated (automated workflow handles this for sub-issues)

## Commit Automation Default

- After Reviewer approval (`✅ All checks passed. Ready to merge.` or `🟡 Approved with inline fixes.`), run the Git Commit phase automatically unless the user explicitly asks to pause.
- Git Commit phase must always include: `git status`, `git diff`, conventional commit with issue link, push branch, and PR creation/update.

