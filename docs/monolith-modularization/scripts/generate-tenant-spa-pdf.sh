#!/usr/bin/env bash
# Regenerate platform-v2-tenant-spa.pdf from HTML + SVG diagram.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
HTML="file://${ROOT}/platform-v2-tenant-spa.html"
OUT="${ROOT}/platform-v2-tenant-spa.pdf"

CHROME="${CHROME:-google-chrome}"
if ! command -v "$CHROME" >/dev/null 2>&1; then
  CHROME=chromium
fi

"$CHROME" \
  --headless=new \
  --disable-gpu \
  --no-sandbox \
  --user-data-dir=/tmp/chrome-pdf-profile-tenant-spa \
  --no-pdf-header-footer \
  --print-to-pdf="$OUT" \
  "$HTML"

echo "Wrote $OUT"
