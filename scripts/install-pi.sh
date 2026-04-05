#!/usr/bin/env bash
set -euo pipefail

INSTALL_DIR="/opt/gardenai"
IMAGE_TAG="latest"
GHCR_USER="VictorSteiner"
GHCR_TOKEN=""
NON_INTERACTIVE="false"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

log() {
  printf '[install-pi] %s\n' "$*"
}

die() {
  printf '[install-pi] ERROR: %s\n' "$*" >&2
  exit 1
}

usage() {
  cat <<EOF
Usage: sudo bash scripts/install-pi.sh [options]

Options:
  --install-dir <path>     Installation directory (default: /opt/gardenai)
  --image-tag <tag>        Docker image tag to deploy (default: latest)
  --ghcr-user <user>       GHCR username (default: VictorSteiner)
  --ghcr-token <token>     GHCR token (read:packages)
  --non-interactive        Do not prompt for values
  --help                   Show this help
EOF
}

require_root() {
  if [[ "${EUID}" -ne 0 ]]; then
    die "Run as root (sudo)."
  fi
}

parse_args() {
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --install-dir)
        INSTALL_DIR="$2"
        shift 2
        ;;
      --image-tag)
        IMAGE_TAG="$2"
        shift 2
        ;;
      --ghcr-user)
        GHCR_USER="$2"
        shift 2
        ;;
      --ghcr-token)
        GHCR_TOKEN="$2"
        shift 2
        ;;
      --non-interactive)
        NON_INTERACTIVE="true"
        shift 1
        ;;
      --help)
        usage
        exit 0
        ;;
      *)
        die "Unknown argument: $1"
        ;;
    esac
  done
}

check_required_files() {
  [[ -f "${REPO_ROOT}/docker-compose.yml" ]] || die "Missing docker-compose.yml in bundle"
  [[ -f "${REPO_ROOT}/docker-compose.pi.yml" ]] || die "Missing docker-compose.pi.yml in bundle"
  [[ -f "${REPO_ROOT}/.env.pi.example" ]] || die "Missing .env.pi.example in bundle"
  [[ -f "${REPO_ROOT}/mosquitto/mosquitto.conf" ]] || die "Missing mosquitto/mosquitto.conf in bundle"
}

install_docker_if_needed() {
  if command -v docker >/dev/null 2>&1; then
    log "Docker already installed"
  else
    log "Installing Docker"
    curl -fsSL https://get.docker.com | sh
  fi

  if docker compose version >/dev/null 2>&1; then
    log "Docker Compose plugin already installed"
  else
    log "Installing Docker Compose plugin"
    apt-get update
    apt-get install -y docker-compose-plugin
  fi

  systemctl enable docker >/dev/null 2>&1 || true
  systemctl start docker
}

prompt_if_needed() {
  local prompt_text="$1"
  local default_value="$2"
  local secret_mode="$3"
  local out

  if [[ "${NON_INTERACTIVE}" == "true" ]]; then
    printf '%s' "${default_value}"
    return
  fi

  if [[ "${secret_mode}" == "true" ]]; then
    read -r -s -p "${prompt_text}: " out
    echo
    if [[ -z "${out}" ]]; then
      out="${default_value}"
    fi
  else
    read -r -p "${prompt_text} [${default_value}]: " out
    if [[ -z "${out}" ]]; then
      out="${default_value}"
    fi
  fi

  printf '%s' "${out}"
}

ensure_ghcr_login() {
  local token="${GHCR_TOKEN}"

  if [[ -z "${token}" ]]; then
    token="$(prompt_if_needed "GHCR token (read:packages)" "" "true")"
  fi

  [[ -n "${token}" ]] || die "GHCR token is required"

  log "Logging into GHCR as ${GHCR_USER}"
  printf '%s' "${token}" | docker login ghcr.io -u "${GHCR_USER}" --password-stdin >/dev/null
}

copy_bundle_files() {
  log "Preparing install directory at ${INSTALL_DIR}"
  mkdir -p "${INSTALL_DIR}/mosquitto"

  cp "${REPO_ROOT}/docker-compose.yml" "${INSTALL_DIR}/docker-compose.yml"
  cp "${REPO_ROOT}/docker-compose.pi.yml" "${INSTALL_DIR}/docker-compose.pi.yml"
  cp "${REPO_ROOT}/.env.pi.example" "${INSTALL_DIR}/.env.pi.example"
  cp "${REPO_ROOT}/mosquitto/mosquitto.conf" "${INSTALL_DIR}/mosquitto/mosquitto.conf"
}

upsert_env() {
  local key="$1"
  local value="$2"
  local env_file="$3"

  if grep -qE "^${key}=" "${env_file}"; then
    sed -i "s|^${key}=.*$|${key}=${value}|" "${env_file}"
  else
    printf '%s=%s\n' "${key}" "${value}" >> "${env_file}"
  fi
}

configure_env_file() {
  local env_file="${INSTALL_DIR}/.env"

  if [[ ! -f "${env_file}" ]]; then
    cp "${INSTALL_DIR}/.env.pi.example" "${env_file}"
  fi

  upsert_env "IMAGE_TAG" "${IMAGE_TAG}" "${env_file}"

  local pg_pass mqtt_pass
  pg_pass="$(prompt_if_needed "POSTGRES_PASSWORD" "changeme" "true")"
  mqtt_pass="$(prompt_if_needed "MQTT_PASSWORD" "changeme" "true")"

  upsert_env "POSTGRES_PASSWORD" "${pg_pass}" "${env_file}"
  upsert_env "MQTT_PASSWORD" "${mqtt_pass}" "${env_file}"

  chmod 600 "${env_file}"
}

deploy_stack() {
  log "Deploying containers"
  (
    cd "${INSTALL_DIR}"
    docker compose -f docker-compose.yml -f docker-compose.pi.yml pull
    docker compose -f docker-compose.yml -f docker-compose.pi.yml up -d
  )
}

print_summary() {
  cat <<EOF

Install complete.

- Install path: ${INSTALL_DIR}
- Image tag: ${IMAGE_TAG}
- Frontend URL: http://raspberrypi.local:3000
- API URL: http://raspberrypi.local:5064

Useful commands:
  cd ${INSTALL_DIR}
  docker compose -f docker-compose.yml -f docker-compose.pi.yml ps
  docker compose -f docker-compose.yml -f docker-compose.pi.yml logs -f watchtower
EOF
}

main() {
  parse_args "$@"
  require_root
  check_required_files
  install_docker_if_needed
  ensure_ghcr_login
  copy_bundle_files
  configure_env_file
  deploy_stack
  print_summary
}

main "$@"

