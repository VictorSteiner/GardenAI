# Release Runbook & Rollback Checklist

This runbook covers every step needed to ship a GardenAI release safely and how to roll back
if something goes wrong.

---

## Table of Contents

1. [Release Pipeline Overview](#1-release-pipeline-overview)
2. [Pre-Release Checklist](#2-pre-release-checklist)
3. [Release Steps](#3-release-steps)
4. [Post-Release Verification](#4-post-release-verification)
5. [Rollback Decision Points](#5-rollback-decision-points)
6. [Rollback Procedures](#6-rollback-procedures)
7. [Rollback Verification](#7-rollback-verification)

---

## 1. Release Pipeline Overview

```
Developer merges PR to main
        ↓
release-alpha.yml         (auto) creates semver pre-release tag vX.Y.Z-alpha.N
        ↓
build-push-images.yml     (auto, triggered on GitHub Release publish/prerelease)
  ├─ Build backend  linux/arm64  → ghcr.io/<repo>-backend:<tag>
  ├─ Build frontend linux/arm64  → ghcr.io/<repo>-frontend:<tag>
  └─ Package Pi USB installer → attached to GitHub Release as .tar.gz + .sha256
        ↓
Watchtower on Pi           (autonomous, polls GHCR every 10 min)
  └─ Detects new image → pulls & restarts containers
        ↓
New containers live on Pi  (~10-15 min after release) ✅
```

---

## 2. Pre-Release Checklist

Complete **all** items before creating or approving the GitHub Release.

### 2.1 Code & CI

- [ ] All PRs targeting this release are merged to `main`.
- [ ] Latest `ci.yml` run on `main` is **green** (Backend Build + Frontend Build both pass).
- [ ] PR title validation (`pr-title-conventional.yml`) passed for every merged PR.
- [ ] No open security alerts in GitHub Dependabot or Code Scanning that block the release.

### 2.2 Database Migrations

- [ ] All pending EF Core migrations are committed and included in `main`.
- [ ] Migrations apply cleanly against a clean database (run locally to confirm):

  ```bash
  dotnet ef database update \
    --project HomeAssistant.Infrastructure.Persistence \
    --startup-project HomeAssistant.Presentation
  ```

- [ ] If any migration is **destructive** (drops a column, renames a table), a data-backup step
  has been added to the deployment notes for this release.

### 2.3 Configuration & Secrets

- [ ] `.env.pi.example` is up to date with any new required environment variables.
- [ ] No secrets are hard-coded in source (run the command below to confirm only placeholder values
  exist – example and test files are intentionally excluded):

  ```bash
  git grep -E "password|secret|token" \
    -- '*.cs' '*.json' \
    ':!*.example*' ':!*Test*' ':!*tests*'
  ```
- [ ] `appsettings.json` / `appsettings.Production.json` reflect current defaults.

### 2.4 Documentation

- [ ] `README.md` updated if setup steps changed.
- [ ] `docs/deployment-pi.md` updated if Compose services or port mappings changed.
- [ ] `INSTALL_FROM_USB.md` updated if installer bundle contents changed.

### 2.5 Local Smoke Test (optional but recommended)

```bash
# Start all infrastructure services
docker compose up -d postgres mosquitto ollama

# Apply migrations
dotnet ef database update \
  --project HomeAssistant.Infrastructure.Persistence \
  --startup-project HomeAssistant.Presentation

# Run API
dotnet run --project HomeAssistant.Presentation --launch-profile http

# Verify OpenAPI schema loads
curl -sf http://localhost:5064/openapi/v1.json | head -5
```

---

## 3. Release Steps

### Step 1 – Verify the auto-generated pre-release tag

After merging to `main`, `release-alpha.yml` creates a tag automatically.

```bash
# Confirm the tag exists and points to the expected commit
git fetch --tags
git tag --list 'v*-alpha.*' --sort=-v:refname | head -5
```

### Step 2 – Review the GitHub Release draft

1. Go to **GitHub → Releases**.
2. The workflow publishes a pre-release automatically.  
   Check that:
   - Tag matches the expected `vX.Y.Z-alpha.N`.
   - Release notes (auto-generated) list the expected commits.
   - Installer `.tar.gz` and `.sha256` artifacts are attached (uploaded by
     `release-installer-artifact` job).

### Step 3 – Confirm Docker image build succeeded

1. Go to **GitHub → Actions → Build and Push Docker Images** for this release run.
2. Both `Build Backend (arm64)` and `Build Frontend (arm64)` jobs must be **green**.
3. Verify images in GHCR:

   ```bash
   # List available tags for backend image (requires gh CLI or browser)
   gh api /orgs/<org>/packages/container/<repo>-backend/versions \
     --jq '.[0:5] | .[] | .metadata.container.tags'
   ```

### Step 4 – Wait for Watchtower to pick up the new images

Watchtower polls GHCR every 10 minutes. Monitor the Pi:

```bash
ssh pi@raspberrypi.local

# Watch Watchtower for pull/restart activity
docker compose logs -f watchtower
# Look for lines like:
#   "Found new ghcr.io/... image ..."
#   "Stopping /ha-backend ..."
#   "Starting /ha-backend ..."
```

Expected wait time: **10–15 minutes** after the GitHub Release is published.

### Step 5 – Run post-release verification (see Section 4)

---

## 4. Post-Release Verification

Run each check on the Pi after Watchtower restarts the containers.

### 4.1 Container Health

```bash
ssh pi@raspberrypi.local

# All containers should show "Up" and healthy
docker compose -f docker-compose.yml -f docker-compose.pi.yml ps

# Expected output includes (status = "Up ... (healthy)"):
#   ha-postgres
#   ha-mosquitto
#   ha-ollama
#   ha-backend
#   ha-frontend
#   watchtower
```

### 4.2 Backend API

```bash
# Health endpoint (adjust port/host as needed)
curl -sf http://raspberrypi.local:5064/health && echo "✅ API healthy"

# OpenAPI schema (confirms routing is intact)
curl -sf http://raspberrypi.local:5064/openapi/v1.json | python3 -m json.tool | head -20
```

### 4.3 Frontend

```bash
# HTTP 200 from the frontend server
curl -o /dev/null -sw "%{http_code}" http://raspberrypi.local:3000
# Expected: 200
```

Or open `http://raspberrypi.local:3000` in a browser.

### 4.4 MQTT Broker

```bash
# Subscribe to the sensor topics and verify messages arrive
docker exec ha-mosquitto mosquitto_sub \
  -h localhost \
  -t "homeassistant/#" \
  -v \
  -C 1 \
  --keepalive 5
# In production: expect real Zigbee2MQTT sensor messages (e.g. on zigbee2mqtt/pot-*/soil/update).
# In development only: mock publisher also produces messages on homeassistant/test/mock-sensors/#.
# Expected: at least one message within 30 seconds.
```

### 4.5 Database

```bash
# Confirm postgres is accepting connections
docker exec ha-postgres pg_isready -U ha_user -d homeassistant
# Expected: "homeassistant:5432 - accepting connections"

# Quick row-count sanity check (no destructive queries)
docker exec ha-postgres psql -U ha_user -d homeassistant -c "\dt"
```

### 4.6 Image Version Confirmation

```bash
# Confirm the running image tags match the release
docker inspect ha-backend  | grep -i '"Image"'
docker inspect ha-frontend | grep -i '"Image"'
# Tags should contain the release version, e.g. "...backend:v0.2.0-alpha.1"
```

---

## 5. Rollback Decision Points

Trigger a rollback if **any** of the following is true after a release:

| # | Trigger | Severity |
|---|---------|----------|
| 1 | `docker compose ps` shows a container in `Restarting` or `Exited` state | 🔴 Critical |
| 2 | `curl` health check returns non-200 or times out | 🔴 Critical |
| 3 | Frontend returns HTTP 5xx or blank page | 🔴 Critical |
| 4 | EF Core migration fails or backend logs DB connection errors | 🔴 Critical |
| 5 | MQTT broker unreachable (no messages within expected interval) | 🟠 High |
| 6 | Sensor readings stop appearing in the database | 🟠 High |
| 7 | Chat/AI endpoint returns 500 repeatedly | 🟡 Medium |
| 8 | New errors appear in container logs that did not exist before the release | 🟡 Medium |

**Rule:** Any 🔴 Critical trigger → roll back immediately without further investigation.
For 🟠 High / 🟡 Medium triggers, allow up to **15 minutes** of investigation; roll back if not resolved.

---

## 6. Rollback Procedures

### Option A – Manual Image Tag Override on Pi (fastest, ~2 min)

Use this when you need an immediate rollback without triggering CI.

> **Important:** Execute Option C (stop Watchtower) **before** pulling the old images to prevent
> Watchtower from immediately re-upgrading back to the broken version.

```bash
ssh pi@raspberrypi.local
cd ~/GardenAI

# Step 1: stop Watchtower so it cannot undo the rollback
docker stop watchtower

# Step 2: identify the last known-good tag
git tag --list 'v*-alpha.*' --sort=-v:refname | head -10
# Choose the tag that was running before the bad release, e.g. v0.1.0-alpha.3

# Step 3: pin the IMAGE_TAG and pull the old images
export IMAGE_TAG=v0.1.0-alpha.3   # ← replace with actual previous tag
docker compose -f docker-compose.yml -f docker-compose.pi.yml pull
docker compose -f docker-compose.yml -f docker-compose.pi.yml up -d

# Step 4: confirm containers restarted with the old tag
docker inspect ha-backend  | grep '"Image"'
docker inspect ha-frontend | grep '"Image"'

# Step 5 (after rollback is verified): resume Watchtower
docker start watchtower
```

### Option B – Create a New GitHub Release Pointing to the Previous Tag (~15 min)

Use this for an official, auditable rollback that goes through the full CI pipeline.

1. Identify the last known-good Git tag:

   ```bash
   git tag --list 'v*-alpha.*' --sort=-v:refname | head -10
   # Note the tag that was running before the bad release, e.g. v0.1.0-alpha.3
   ```

2. On GitHub, go to **Releases → Draft a new release**.
3. Set the tag to the previous known-good version (e.g. `v0.1.0-alpha.3`).
4. Publish the release. `build-push-images.yml` will re-push images tagged with that version.
5. Watchtower detects the "new" (actually old) image and updates the Pi automatically.

### Option C – Stop Watchtower (Prevent Further Auto-Updates)

Use this when you need to freeze the Pi at its current state while debugging.

```bash
ssh pi@raspberrypi.local

# Stop Watchtower so it does not pull any more updates
docker stop watchtower
# --- debug / fix the issue ---

# Resume Watchtower when ready
docker start watchtower
```

### Option D – Full Stack Restart (If Containers Are Stuck)

If containers are in a bad state and won't recover on their own:

```bash
ssh pi@raspberrypi.local
cd ~/GardenAI

# Bring everything down (data volumes are preserved)
docker compose -f docker-compose.yml -f docker-compose.pi.yml down

# Identify the last known-good tag (if not already known)
git tag --list 'v*-alpha.*' --sort=-v:refname | head -10

# Bring back up with the desired image tag
export IMAGE_TAG=v0.1.0-alpha.3   # ← replace with actual previous tag
docker compose -f docker-compose.yml -f docker-compose.pi.yml up -d

# Monitor startup
docker compose -f docker-compose.yml -f docker-compose.pi.yml logs -f
```

---

## 7. Rollback Verification

After executing any rollback procedure, repeat the **Section 4** checks:

```bash
# 1. Container health
docker compose -f docker-compose.yml -f docker-compose.pi.yml ps

# 2. Backend
curl -sf http://raspberrypi.local:5064/health && echo "✅ Backend OK"

# 3. Frontend
curl -o /dev/null -sw "%{http_code}\n" http://raspberrypi.local:3000

# 4. Database connectivity
docker exec ha-postgres pg_isready -U ha_user -d homeassistant

# 5. Confirm rolled-back image tag is running
docker inspect ha-backend  | grep '"Image"'
docker inspect ha-frontend | grep '"Image"'
```

All checks must pass before declaring the rollback successful.

Once stable, open a GitHub issue to track the root cause of the failed release and block future
releases until a fix is confirmed.

---

## See Also

- [`docs/deployment-pi.md`](deployment-pi.md) – First-time Pi setup and automated update details
- [`docs/automation.md`](automation.md) – CI/CD workflow descriptions and branch protection rules
- [`docs/INSTALL_FROM_USB.md`](INSTALL_FROM_USB.md) – USB-based fresh install procedure
