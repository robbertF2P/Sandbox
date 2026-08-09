#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
MONITOR_DIR="${ROOT}/personal/media-monitor"
VENV="${MONITOR_DIR}/.venv"

if [[ ! -d "$VENV" ]]; then
  python3 -m venv "$VENV"
  "${VENV}/bin/pip" install -q -r "${MONITOR_DIR}/requirements.txt"
fi

CONFIG="${MONITOR_DIR}/config.yaml"
if [[ ! -f "$CONFIG" ]]; then
  CONFIG="${MONITOR_DIR}/config.example.yaml"
fi

cd "$MONITOR_DIR"
exec "${VENV}/bin/python" -m media_monitor --config "$CONFIG" "$@"
