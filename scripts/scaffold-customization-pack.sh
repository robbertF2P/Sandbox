#!/usr/bin/env bash
set -euo pipefail

# Scaffold a customization (UI) pack from starter-kit templates.
#
# Usage:
#   ./scripts/scaffold-customization-pack.sh HourApprovals Acme acme-hour-approvals-v1
#   ./scripts/scaffold-customization-pack.sh HourApprovals Acme acme-hour-approvals-v1 --target /path/to/F2pPlatform
#
# Prerequisites:
#   - <Context> module exists with I<Context>CustomizationPack port and <Context>Screens
#   - Default<Context>Pack in Infrastructure (or adjust generated test)

if [[ $# -lt 3 ]]; then
  echo "Usage: $0 <ContextName> <ClientName> <pack-id> [--target <F2pPlatformRoot>]" >&2
  echo "Example: $0 HourApprovals Acme acme-hour-approvals-v1" >&2
  exit 1
fi

CONTEXT="$1"
CLIENT="$2"
PACK_ID="$3"
shift 3

TARGET_ROOT="${F2P_PLATFORM_ROOT:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/F2pPlatform}"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --target)
      TARGET_ROOT="$2"
      shift 2
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

if [[ ! "$CONTEXT" =~ ^[A-Z][A-Za-z0-9]*$ ]]; then
  echo "Context name must be PascalCase (e.g. HourApprovals)." >&2
  exit 1
fi

if [[ ! "$CLIENT" =~ ^[A-Z][A-Za-z0-9]*$ ]]; then
  echo "Client name must be PascalCase (e.g. Acme)." >&2
  exit 1
fi

if [[ ! "$PACK_ID" =~ ^[a-z0-9]+(-[a-z0-9]+)*-v[0-9]+$ ]]; then
  echo "pack-id should be kebab-case with version suffix (e.g. acme-hour-approvals-v1)." >&2
  exit 1
fi

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TEMPLATE_DIR="${ROOT}/docs/monolith-modularization/starter-kit/templates/customization-pack"
PACK_DIR="${TARGET_ROOT}/src/Packs/${CONTEXT}.Packs.${CLIENT}"
PROJECT_NAME="${CONTEXT}.Packs.${CLIENT}"
CONTEXT_CAMEL="$(python3 -c "c='${CONTEXT}'; print(c[0].lower()+c[1:])")"
CLIENT_UPPER="$(echo "${CLIENT}" | tr '[:lower:]' '[:upper:]')"
CONTEXT_UPPER="$(echo "${CONTEXT}" | tr '[:lower:]' '[:upper:]')"

if [[ ! -d "$TEMPLATE_DIR" ]]; then
  echo "Templates not found: ${TEMPLATE_DIR}" >&2
  exit 1
fi

MODULE_APP="${TARGET_ROOT}/src/Modules/${CONTEXT}/${CONTEXT}.Application"
if [[ ! -d "$MODULE_APP" ]]; then
  echo "Module Application layer not found: ${MODULE_APP}" >&2
  echo "Run ./scripts/scaffold-module.sh ${CONTEXT} first." >&2
  exit 1
fi

if [[ -e "$PACK_DIR" ]]; then
  echo "Pack already exists: ${PACK_DIR}" >&2
  exit 1
fi

mkdir -p "$PACK_DIR"

apply_tokens() {
  local file="$1"
  sed -i \
    -e "s/<Context>/${CONTEXT}/g" \
    -e "s/<Client>/${CLIENT}/g" \
    -e "s/<pack-id>/${PACK_ID}/g" \
    -e "s/<context>/${CONTEXT_CAMEL}/g" \
    -e "s/<CLIENT>/${CLIENT_UPPER}/g" \
    -e "s/<CONTEXT>/${CONTEXT_UPPER}/g" \
    "$file"
}

cp "${TEMPLATE_DIR}/PACK.md" "${PACK_DIR}/PACK.md"
cp "${TEMPLATE_DIR}/CustomizationPack.cs" "${PACK_DIR}/${CLIENT}${CONTEXT}Pack.cs"
cp "${TEMPLATE_DIR}/DependencyInjection.cs" "${PACK_DIR}/DependencyInjection.cs"
cp "${TEMPLATE_DIR}/CustomizationPack.csproj" "${PACK_DIR}/${PROJECT_NAME}.csproj"

find "$PACK_DIR" -type f -print0 | while IFS= read -r -d '' file; do
  apply_tokens "$file"
done

UNIT_TESTS="${TARGET_ROOT}/tests/Modules/${CONTEXT}/${CONTEXT}.Unit.Tests"
if [[ -d "$UNIT_TESTS" ]]; then
  TEST_FILE="${UNIT_TESTS}/${CLIENT}${CONTEXT}PackShould.cs"
  if [[ ! -f "$TEST_FILE" ]]; then
    cp "${TEMPLATE_DIR}/CustomizationPackShould.cs" "$TEST_FILE"
    apply_tokens "$TEST_FILE"
    echo "Created ${TEST_FILE}"
  fi
fi

I18N_FRAGMENT="${TARGET_ROOT}/web/libs/$(echo "${CONTEXT}" | sed 's/\([A-Z]\)/-\L\1/g' | sed 's/^-//')/data-access/src/lib/pack-${PACK_ID}.i18n.ts.fragment"
if [[ -d "$(dirname "$I18N_FRAGMENT")" ]]; then
  cp "${TEMPLATE_DIR}/pack.i18n.ts.fragment" "$I18N_FRAGMENT"
  apply_tokens "$I18N_FRAGMENT"
  echo "Created i18n fragment: ${I18N_FRAGMENT}"
fi

SLNX="${TARGET_ROOT}/F2pPlatform.slnx"
if [[ -f "$SLNX" ]] && ! grep -q "${PROJECT_NAME}" "$SLNX"; then
  python3 - <<PY
from pathlib import Path
slnx = Path("${SLNX}")
project = "${PROJECT_NAME}"
path = f'src/Packs/{project}/{project}.csproj'
text = slnx.read_text()
needle = '  <Folder Name="/src/Packs/">'
insert = f'    <Project Path="{path}" />'
if project not in text:
    text = text.replace(needle, needle + "\n" + insert, 1)
    slnx.write_text(text)
    print(f"Updated {slnx}")
PY
fi

echo ""
echo "Scaffolded customization pack at ${PACK_DIR}"
echo ""
echo "Next steps:"
echo "  1. Edit PACK.md — screens, extension fields, legacy mapping"
echo "  2. Implement GetView / GetRowExtensions in ${CLIENT}${CONTEXT}Pack.cs"
echo "  3. Merge i18n fragment into web libs (if created)"
echo "  4. Register in host Program.cs:"
echo "       builder.Services.Add${CLIENT}${CONTEXT}Pack();"
echo "  5. Entitle tenant: customizationPacks: [\"${PACK_ID}\"]"
echo "  6. dotnet build ${TARGET_ROOT} && dotnet test ${TARGET_ROOT}"
echo ""
echo "Blueprint: docs/monolith-modularization/platform-pack-blueprint.md"
