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
GitHub Actions (trigger-watchtower-deploy.yml)
    └─ POST webhook to Pi:6969
    ↓
Watchtower on Pi
    ├─ Receives webhook → immediate pull & restart
    └─ Daily poll (fallback if webhook fails)
    ↓
New containers running on Pi ✅
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

### On-Demand (Release Trigger)

When you create a GitHub **release**:

1. GitHub Actions builds Docker images (`backend:v0.1.0-alpha.1`, `frontend:v0.1.0-alpha.1`)
2. Images pushed to GHCR
3. `trigger-watchtower-deploy.yml` fires
4. Sends webhook to `http://raspberrypi.local:6969/v1/update`
5. Watchtower immediately pulls new images & restarts containers
6. Update complete in ~2-5 minutes ✅

### Fallback (Daily Poll)

If the webhook fails (Pi offline, firewall issue):
- Watchtower polls GHCR **once per day** (86400 seconds)
- Detects new image tags
- Pulls and restarts automatically
- Ensures Pi always eventually updates

## Secrets & Authentication

### GitHub Actions → GHCR

Uses `${{ secrets.GITHUB_TOKEN }}` (auto-provided by GitHub, no setup needed).

### GitHub Actions → Watchtower Webhook

Uses repository secret `PI_HOSTNAME` (set in Settings → Secrets):
- **Name:** `PI_HOSTNAME`
- **Value:** `raspberrypi.local` (or static Pi IP)

If Pi is not on local network, you'll need to set up:
- Port forwarding in your router (forward 6969 externally)
- Then set `PI_HOSTNAME` to your public IP or dynamic DNS

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
```bash
curl -X POST http://raspberrypi.local:6969/v1/update
```

## Troubleshooting

### Watchtower webhook timeout
- **Cause:** Pi not reachable on local network
- **Fix:** Verify Pi IP, check firewall, or wait for daily poll

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

