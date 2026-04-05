# USB Installer (No Repo Clone on Pi)

Use this flow when you want to install GardenAI on a Raspberry Pi from a release artifact on USB.

## What you need

- Raspberry Pi 5 (Linux)
- Internet access from Pi (for Docker image pulls)
- GitHub PAT with `read:packages` for `ghcr.io`
- USB drive with release installer archive

## Download installer archive

From GitHub Release assets, download:

- `gardenai-pi-installer-<tag>.tar.gz`
- `gardenai-pi-installer-<tag>.sha256`

Copy both files to your USB drive.

## Install on Pi

1. Insert USB into Pi.
2. Open terminal on Pi and mount USB if needed.
3. Verify checksum:

```bash
cd /media/pi/<USB_NAME>
sha256sum -c gardenai-pi-installer-<tag>.sha256
```

4. Extract and run installer:

```bash
tar -xzf gardenai-pi-installer-<tag>.tar.gz
cd gardenai-pi-installer-<tag>
sudo bash scripts/install-pi.sh --image-tag <tag> --ghcr-user VictorSteiner
```

The installer will:
- install Docker and Docker Compose (if missing)
- copy Compose files to `/opt/gardenai`
- prompt for required secret values
- log in to GHCR
- pull and start services

## Update an existing Pi

Run again with a new tag:

```bash
sudo bash scripts/install-pi.sh --image-tag <new-tag> --ghcr-user VictorSteiner
```

This updates `.env` with the new image tag and redeploys.

