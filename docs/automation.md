# Automation Guide

This repository uses GitHub automation for CI, pull request hygiene, dependency updates, and alpha pre-releases.

## Workflows

- `.github/workflows/ci.yml`
  - Runs on `pull_request` and `push` to `main`.
  - Builds backend (`dotnet build HomeAssistant.sln`) and frontend (`npm run build` in `frontend/`).

- `.github/workflows/pr-title-conventional.yml`
  - Enforces Conventional Commit PR titles in the form `type(scope): description`.

- `.github/workflows/pr-linked-issue.yml`
  - Requires every PR to be linked to an issue with `Closes #<id>`, `Fixes #<id>`, `Resolves #<id>`, or `Refs #<id>`.
  - If missing, auto-links from branch names that include the issue number (for example `feature/123-short-title` -> `Refs #123`).
  - Fails the check when no explicit link exists and no issue number can be inferred.

- `.github/workflows/bot-auto-approve.yml`
  - Auto-approves PRs only when label `bot-auto-approve` is present.
  - Uses `BOT_GITHUB_TOKEN`.
  - Explicitly excludes `dependabot[bot]` so dependency PRs still require manual approval.

- `.github/workflows/release-alpha.yml`
  - On push to `main`, creates semantic pre-release tags like `v0.1.0-alpha.1`.
  - Bump rules from commit messages:
    - `BREAKING CHANGE` or `!:` -> major
    - `feat(...)` -> minor
    - otherwise -> patch

## Dependabot

Configured in `.github/dependabot.yml`.

- Weekly PRs for:
  - NuGet (repo root)
  - npm (`frontend/`)
  - GitHub Actions
- Dependency PRs are created automatically but require your manual review and approval.

## Required Secrets

Add in: `Settings -> Secrets and variables -> Actions`

- `BOT_GITHUB_TOKEN`
  - Fine-grained PAT (preferably from a bot account)
  - Minimum permissions:
    - Pull requests: Read and write
    - Contents: Read

## Recommended Repository Settings (manual)

In repository settings for `main` branch protection/ruleset:

- Require a pull request before merging
- Require at least one approval
- Require status checks:
  - `Backend Build (.NET)`
  - `Frontend Build (Node)`
  - `Validate PR Title`
- Require linear history
- Allow merge methods:
  - Squash merge
  - Rebase merge
- Optionally disable merge commits

## Release Channel Strategy

Current channel: **alpha** (`vX.Y.Z-alpha.N`).

When ready for beta:

1. Update release workflow to emit `-beta.N` suffix.
2. Keep same semantic bump rules.
3. Later remove pre-release suffix for stable versions.

