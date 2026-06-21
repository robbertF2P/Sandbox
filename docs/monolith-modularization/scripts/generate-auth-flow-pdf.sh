#!/usr/bin/env bash
# Regenerate platform-v2-authentication-flow.pdf from HTML + SVG diagrams.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
HTML="file://${ROOT}/platform-v2-authentication-flow.html"
OUT="${ROOT}/platform-v2-authentication-flow.pdf"

CHROME="${CHROME:-google-chrome}"
if ! command -v "$CHROME" >/dev/null 2>&1; then
  CHROME=chromium
fi

"$CHROME" \
  --headless=new \
  --disable-gpu \
  --no-sandbox \
  --user-data-dir=/tmp/chrome-pdf-profile \
  --no-pdf-header-footer \
  --print-to-pdf="$OUT" \
  "$HTML"

echo "Wrote $OUT"
