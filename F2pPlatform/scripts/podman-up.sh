#!/usr/bin/env bash
set -euo pipefail

platform_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
repo_root="$(cd "${platform_root}/.." && pwd)"
compose_file="${platform_root}/docker-compose.stack.yml"

resolve_compose() {
  if command -v podman >/dev/null 2>&1 && podman compose version >/dev/null 2>&1; then
    echo "podman compose"
    return
  fi

  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    echo "docker compose"
    return
  fi

  if command -v podman-compose >/dev/null 2>&1; then
    echo "podman-compose"
    return
  fi

  echo "No container compose tool found. Install Podman (podman compose) or Docker." >&2
  exit 1
}

read_logging_package_version() {
  sed -n 's/.*<PlatformSerilogLoggingVersion>\([^<]*\)<\/PlatformSerilogLoggingVersion>.*/\1/p' \
    "${repo_root}/build/Platform.Logging.Versions.props" | head -n 1
}

wait_for_url() {
  local url="$1"
  local label="$2"
  local attempts="${3:-60}"

  for ((i = 1; i <= attempts; i++)); do
    if curl -fsS "${url}" >/dev/null 2>&1; then
      echo "  ${label} ready (${url})"
      return 0
    fi

    sleep 2
  done

  echo "Timed out waiting for ${label} at ${url}" >&2
  return 1
}

compose_cmd="$(resolve_compose)"
logging_version="$(read_logging_package_version)"

if [[ -z "${logging_version}" ]]; then
  echo "Could not read PlatformSerilogLoggingVersion from build/Platform.Logging.Versions.props" >&2
  exit 1
fi

echo "==> Packing local Platform.Serilog.Logging ${logging_version} to local-feed"
"${repo_root}/scripts/pack-platform-logging.sh" "${logging_version}"

echo "==> Building and starting F2pPlatform stack (${compose_cmd})"
(
  cd "${platform_root}"
  # shellcheck disable=SC2086
  ${compose_cmd} -f "${compose_file}" up --build -d "$@"
)

echo "==> Waiting for services"
wait_for_url "http://localhost:5080/health" "API"
wait_for_url "http://localhost:5180/" "Tenant UI"

cat <<EOF

F2pPlatform stack is running.

  Tenant UI:       http://localhost:5180
  Hour approvals:  http://localhost:5180/hour-approvals
  API / Swagger:   http://localhost:5080/swagger
  Seq logs:        http://localhost:8083

Login with supervisor.demo (any password) to approve, or foreman.demo (read-only).

Stop:  ${platform_root}/scripts/podman-down.sh
Logs:  cd ${platform_root} && ${compose_cmd} -f docker-compose.stack.yml logs -f
EOF
