# GitHub Project Workflow

This repository follows a task-first workflow using GitHub Issues + GitHub Project.

## 1) Start with a task

- Create an issue before implementation.
- Assign work-type and priority labels.
- Add the issue to project `Garden AI`.

## 2) Use parent/sub-issues for initiatives

For larger work:

- Create one parent issue.
- Create child issues for concrete deliverables.
- Link children as sub-issues of the parent.
- Keep parent open until children are complete.

## 3) Investigation tasks

Default investigation reporting is issue-native:

- Append findings to parent issue description or comment thread.
- Create remediation sub-issues from findings.
- Avoid standalone markdown reports unless explicitly needed.

If a temporary markdown report exists, create a dedicated cleanup issue to remove it after remediation.

## 4) Feature approval gate

Feature tasks require owner approval before execution:

- Label feature issue as `needs-approval`.
- Set project field `Approval` to `Pending Approval`.
- Move to `Todo` only after:
  - label changed to `approved`
  - `Approval` changed to `Approved`

## 5) PR requirements

- Every PR must link an issue (`Closes #<id>`, `Fixes #<id>`, `Resolves #<id>`, or `Refs #<id>`).
- PR title must follow Conventional Commits.

## 6) Completion

Close the task only when:

- implementation is merged
- verification is complete
- parent progress is updated (automatic for sub-issues)

