#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PUBLIC_URL_FILE="${ASPIRE_DASHBOARD_PUBLIC_URL_FILE:-/tmp/aspire-dashboard-public-url.txt}"
NGROK_LOG="${NGROK_LOG:-/tmp/ngrok.log}"
CLOUDFLARED_LOG="${CLOUDFLARED_LOG:-/tmp/cloudflared-aspire.log}"

if [[ -z "${NGROK_AUTHTOKEN:-}" && -n "${Ngrok token:-}" ]]; then
  export NGROK_AUTHTOKEN="${Ngrok token}"
fi

if [[ -n "${NGROK_AUTHTOKEN:-}" ]]; then
  ngrok config add-authtoken "${NGROK_AUTHTOKEN}" >/dev/null 2>&1 || true
fi

pkill -f 'ngrok http 4200' 2>/dev/null || true
pkill -f 'cloudflared tunnel --url https://127.0.0.1:17261' 2>/dev/null || true
sleep 1

ngrok http 4200 --log=stdout >"${NGROK_LOG}" 2>&1 &
cloudflared tunnel --url https://127.0.0.1:17261 --no-tls-verify >"${CLOUDFLARED_LOG}" 2>&1 &
sleep 6

APP_URL="$(curl -fsS http://127.0.0.1:4040/api/tunnels | python3 -c "import sys,json; print(json.load(sys.stdin)['tunnels'][0]['public_url'])")"
ASPIRE_URL="$(grep -o 'https://[^ ]*trycloudflare.com' "${CLOUDFLARED_LOG}" | head -1)"

if [[ -z "${ASPIRE_URL}" ]]; then
  echo "Aspire tunnel failed. See ${CLOUDFLARED_LOG}" >&2
  exit 1
fi

echo "${ASPIRE_URL}" >"${PUBLIC_URL_FILE}"

echo ""
echo "App:       ${APP_URL}"
echo "Dashboard: ${ASPIRE_URL}/login?t=<see aspire run console>"
echo ""
echo "Aspire public URL written to ${PUBLIC_URL_FILE}"
