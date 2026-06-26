#!/usr/bin/env bash
set -euo pipefail

platform_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
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

  echo "No container compose tool found." >&2
  exit 1
}

compose_cmd="$(resolve_compose)"

(
  cd "${platform_root}"
  # shellcheck disable=SC2086
  ${compose_cmd} -f "${compose_file}" down "$@"
)

echo "F2pPlatform stack stopped."
