# Raspberry Pi 5 Deployment & CD Pipeline

## Overview

This document describes how GardenAI is deployed to Raspberry Pi 5 with automated updates via Docker and Watchtower.

## Architecture

```
GitHub Release (manual)
    ↓
GitHub Actions (build-push-images.yml)
    ├─ Build backend (arm64)
    ├─ Build frontend (arm64)
    └─ Push to GHCR (GitHub Container Registry)
    ↓
Watchtower on Pi (autonomous)
    └─ Polls GHCR every 10 minutes
    ↓
New image detected → pulls & restarts
    ↓
New containers running on Pi ✅
(Update time: ~10-15 minutes after release)
```

## First-Time Setup (Manual, One-Time)

### Prerequisites
- Raspberry Pi 5 running Linux (e.g., Raspberry Pi OS 64-bit)
- Docker & Docker Compose installed
- Pi hostname: `raspberrypi.local` (or use its IP)
- Network: Pi reachable from your dev machine

### Steps

1. **SSH into Pi:**
   ```bash
   ssh pi@raspberrypi.local
   ```

2. **Clone repository:**
   ```bash
   git clone https://github.com/VictorSteiner/GardenAI.git
   cd GardenAI
   ```

3. **Create environment file:**
   ```bash
   cp .env.pi.example .env
   # Edit .env if needed (passwords, timezones, etc.)
   nano .env
   ```

4. **Start services:**
   ```bash
   docker compose -f docker-compose.yml -f docker-compose.pi.yml up -d
   ```

5. **Verify:**
   ```bash
   docker compose logs -f
   # Wait for all services to start (API on :5064, Frontend on :3000, Watchtower on :6969)
   ```

6. **Access frontend:**
   ```
   http://raspberrypi.local:3000
   ```

Done! Watchtower is now running and will auto-update on new releases.

## Automated Updates

Watchtower automatically polls GHCR every 10 minutes. When a new image is available:

1. Watchtower detects the new image tag
2. Pulls the new backend and frontend images
3. Stops old containers
4. Starts new containers with fresh images
5. Updates complete in ~2-3 minutes ✅

**Update latency:** Release created → ~10-15 minutes until all Pis updated (10 min poll interval + 2-3 min pull/restart)

### How It Works (No Network Exposure Required)

- **Pi initiates connection:** Watchtower polls GHCR outbound
- **GitHub never reaches Pi:** No port-forwarding, no firewall rules needed
- **Multi-Pi support:** Each Pi independently polls and updates
- **Resource overhead:** Negligible (~0.1% CPU, minimal bandwidth per check)

## Secrets & Authentication

### GitHub Actions → GHCR

Uses `${{ secrets.GITHUB_TOKEN }}` (auto-provided by GitHub, no setup needed).


### Pi → GHCR (Pull Authentication)

Docker on Pi needs credentials to pull private images from GHCR:

1. Create GitHub PAT (Personal Access Token):
   - GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Scopes: `read:packages`
   - Copy token

2. On Pi, log in to GHCR:
   ```bash
   docker login ghcr.io -u VictorSteiner -p <YOUR_PAT>
   ```

3. Docker now has auth cached; `docker compose pull` will work

## Rollback Strategy

### If a Release Breaks Things

**Option 1: Revert to Previous Version (Manual)**
```bash
ssh pi@raspberrypi.local
export IMAGE_TAG=v0.1.0-alpha.1  # previous version
docker compose -f docker-compose.yml -f docker-compose.pi.yml up -d
```

**Option 2: GitHub Release Rollback**
- Create a new release pointing to the previous tag
- Trigger the workflow again
- Watchtower pulls old images

**Option 3: Stop Auto-Updates (Temporary)**
```bash
# On Pi
docker stop watchtower
# ... debug ...
docker start watchtower
```

## Monitoring

### View Watchtower Logs
```bash
docker compose logs -f watchtower
```

### Check Running Containers
```bash
docker compose ps
```

### Manual Trigger Test (for debugging)

To manually check if Watchtower picks up updates:
```bash
ssh pi@raspberrypi.local
docker compose logs -f watchtower
# Watch for "Pulling" messages when a new image is released
```

## Troubleshooting

### Watchtower not detecting updates
- **Cause:** Watchtower not polling or image not pushed yet
- **Fix:** Check logs: `docker compose logs -f watchtower`

### Pull fails (auth error)
- **Cause:** Docker not logged into GHCR
- **Fix:** Re-run `docker login ghcr.io` on Pi

### Containers won't restart after pull
- **Cause:** Old container still running
- **Fix:** `docker compose down && docker compose -f docker-compose.yml -f docker-compose.pi.yml up -d`

## Future Enhancements

- [ ] Add Sentry/monitoring for crashed containers
- [ ] Slack/email notifications on failed updates
- [ ] Multi-Pi dashboard (manage fleet of Pis)
- [ ] Blue-green deployment (zero downtime)

