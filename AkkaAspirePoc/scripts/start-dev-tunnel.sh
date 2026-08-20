#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROXY_PORT="${TUNNEL_PROXY_PORT:-8888}"
NGROK_LOG="${NGROK_LOG:-/tmp/ngrok.log}"
PROXY_LOG="${PROXY_LOG:-/tmp/tunnel-proxy.log}"

if ! command -v ngrok >/dev/null 2>&1; then
  echo "ngrok not found. Install from https://ngrok.com/download" >&2
  exit 1
fi

if [[ -z "${NGROK_AUTHTOKEN:-}" && -n "${Ngrok token:-}" ]]; then
  export NGROK_AUTHTOKEN="${Ngrok token}"
fi

if [[ -n "${NGROK_AUTHTOKEN:-}" ]]; then
  ngrok config add-authtoken "${NGROK_AUTHTOKEN}" >/dev/null 2>&1 || true
fi

pkill -f 'dev-tunnel-proxy.mjs' 2>/dev/null || true
pkill -f 'localtunnel --port' 2>/dev/null || true
fuser -k "${PROXY_PORT}/tcp" 2>/dev/null || true
sleep 1

node "${ROOT}/scripts/dev-tunnel-proxy.mjs" >"${PROXY_LOG}" 2>&1 &
sleep 1

pkill -f 'ngrok http' 2>/dev/null || true
sleep 1
ngrok http "${PROXY_PORT}" --log=stdout >"${NGROK_LOG}" 2>&1 &
sleep 3

PUBLIC_URL="$(
  curl -fsS http://127.0.0.1:4040/api/tunnels 2>/dev/null \
    | python3 -c "import sys,json; t=json.load(sys.stdin).get('tunnels',[]); print(next((x['public_url'] for x in t if x.get('public_url','').startswith('https')), ''))" \
    || true
)"

if [[ -z "${PUBLIC_URL}" ]]; then
  echo "Tunnel proxy started on :${PROXY_PORT}, but ngrok public URL not ready yet." >&2
  echo "Check ${NGROK_LOG}" >&2
  exit 1
fi

echo ""
echo "=== Akka Aspire POC — public dev URL ==="
echo "Portal:    ${PUBLIC_URL}/"
echo "Todos:     ${PUBLIC_URL}/todos"
echo "API links: ${PUBLIC_URL}/api/links"
echo "Health:    ${PUBLIC_URL}/health"
echo ""
echo "Aspire dashboard (via tunnel):"
curl -fsS -H "ngrok-skip-browser-warning: true" "${PUBLIC_URL}/api/links" \
  | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['aspireDashboard']['url'] or 'unavailable')" 2>/dev/null || true
echo ""
echo "Proxy log: ${PROXY_LOG}"
echo "Ngrok log: ${NGROK_LOG}"
echo "Ngrok UI:  http://127.0.0.1:4040"
