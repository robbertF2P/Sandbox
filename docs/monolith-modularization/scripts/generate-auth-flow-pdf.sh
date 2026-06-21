#!/usr/bin/env bash
# Build HTML with inlined SVG diagrams, then generate PDF.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
HTML="${ROOT}/platform-v2-authentication-flow.html"
OUT="${ROOT}/platform-v2-authentication-flow.pdf"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

python3 "${SCRIPT_DIR}/build-auth-flow-html.py"

CHROME="${CHROME:-google-chrome}"
if ! command -v "$CHROME" >/dev/null 2>&1; then
  CHROME=chromium
fi

timeout 60 "$CHROME" \
  --headless=new \
  --disable-gpu \
  --no-sandbox \
  --user-data-dir="/tmp/chrome-pdf-profile-$$" \
  --virtual-time-budget=10000 \
  --run-all-compositor-stages-before-draw \
  --no-pdf-header-footer \
  --print-to-pdf="$OUT" \
  "file://${HTML}"

echo "Wrote $OUT"
