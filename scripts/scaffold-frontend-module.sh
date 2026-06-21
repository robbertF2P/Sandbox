#!/usr/bin/env bash
set -euo pipefail

# Scaffold a bounded-context frontend lib group from the Reference template.
#
# Usage:
#   ./scripts/scaffold-frontend-module.sh Planning
#
# Creates:
#   F2pPlatform/web/libs/<context>/data-access/
#   F2pPlatform/web/libs/<context>/feature-status/
# Updates tsconfig paths. Register lazy route + home tile manually (or via prompt).

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <ContextName>" >&2
  exit 1
fi

CONTEXT="$1"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB="${ROOT}/F2pPlatform/web"
SOURCE="${WEB}/libs/reference"
DEST="${WEB}/libs/$(echo "${CONTEXT}" | tr '[:upper:]' '[:lower:]')"
LOWER="$(echo "${CONTEXT}" | tr '[:upper:]' '[:lower:]')"

if [[ ! "$CONTEXT" =~ ^[A-Z][A-Za-z0-9]*$ ]]; then
  echo "Context name must be PascalCase (e.g. Planning, Import)." >&2
  exit 1
fi

if [[ "$CONTEXT" == "Reference" ]]; then
  echo "Reference is the template — choose another name." >&2
  exit 1
fi

if [[ -e "$DEST" ]]; then
  echo "Frontend libs already exist: ${DEST}" >&2
  exit 1
fi

copy_and_rename() {
  local src="$1"
  local dest="$2"
  cp -R "$src" "$dest"

  find "$dest" -depth -type d -name '*reference*' -print0 \
    | while IFS= read -r -d '' path; do
        mv "$path" "${path//reference/${LOWER}}"
      done

  find "$dest" -type f -name '*reference*' -print0 \
    | while IFS= read -r -d '' path; do
        mv "$path" "${path//reference/${LOWER}}"
      done

  find "$dest" -type f \( -name '*.ts' -o -name '*.html' -o -name '*.json' \) -print0 \
    | while IFS= read -r -d '' file; do
        sed -i \
          -e "s/Reference/${CONTEXT}/g" \
          -e "s/reference/${LOWER}/g" \
          "$file"
      done
}

echo "Scaffolding frontend libs at ${DEST}"
copy_and_rename "$SOURCE" "$DEST"

TS_CONFIG="${WEB}/tsconfig.json"
if ! grep -q "@f2p/${LOWER}/data-access" "$TS_CONFIG"; then
  python3 - <<PY
import json
from pathlib import Path
path = Path("${TS_CONFIG}")
data = json.loads(path.read_text())
paths = data["compilerOptions"].setdefault("paths", {})
paths[f"@f2p/${LOWER}/data-access"] = [f"libs/${LOWER}/data-access/src/index.ts"]
paths[f"@f2p/${LOWER}/feature-status"] = [f"libs/${LOWER}/feature-status/src/index.ts"]
path.write_text(json.dumps(data, indent=2) + "\n")
print(f"Updated {path}")
PY
fi

echo ""
echo "Next steps:"
echo "  1. Add lazy route in apps/f2p-shell/src/app/app.routes.ts:"
echo "       path: '${LOWER}', loadChildren: () => import('@f2p/${LOWER}/feature-status').then(m => m.${LOWER}Routes)"
echo "  2. Add home tile in apps/f2p-shell/src/app/pages/home-page.component.ts"
echo "  3. Rename feature-status folder if this context needs a different feature slice"
echo "  4. npm run build (in F2pPlatform/web)"
