#!/usr/bin/env bash
set -euo pipefail

# Scaffold a bounded-context module from the Reference template.
#
# Usage:
#   ./scripts/scaffold-module.sh Import
#   ./scripts/scaffold-module.sh Import --target /path/to/F2pPlatform
#
# Creates src/Modules/<Context>/ and tests/Modules/<Context>/ under the target root
# (default: F2pPlatform/) and appends projects to F2pPlatform.slnx when present.

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <ContextName> [--target <F2pPlatformRoot>]" >&2
  exit 1
fi

CONTEXT="$1"
shift

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
  echo "Context name must be PascalCase (e.g. Import, Planning)." >&2
  exit 1
fi

if [[ "$CONTEXT" == "Reference" ]]; then
  echo "Reference is the template module — choose another name." >&2
  exit 1
fi

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE_MODULE="${ROOT}/F2pPlatform/src/Modules/Reference"
SOURCE_TESTS="${ROOT}/F2pPlatform/tests/Modules/Reference"
DEST_MODULE="${TARGET_ROOT}/src/Modules/${CONTEXT}"
DEST_TESTS="${TARGET_ROOT}/tests/Modules/${CONTEXT}"

if [[ ! -d "$SOURCE_MODULE" ]]; then
  echo "Template not found: ${SOURCE_MODULE}" >&2
  exit 1
fi

if [[ -e "$DEST_MODULE" ]]; then
  echo "Module already exists: ${DEST_MODULE}" >&2
  exit 1
fi

mkdir -p "$(dirname "$DEST_MODULE")" "$(dirname "$DEST_TESTS")"

copy_and_rename() {
  local src="$1"
  local dest="$2"
  local lower_context
  lower_context="$(echo "${CONTEXT}" | tr '[:upper:]' '[:lower:]')"

  cp -R "$src" "$dest"

  find "$dest" -depth -type d -name '*Reference*' -print0 \
    | while IFS= read -r -d '' path; do
        mv "$path" "${path//Reference/${CONTEXT}}"
      done

  find "$dest" -type f -name '*Reference*' -print0 \
    | while IFS= read -r -d '' path; do
        mv "$path" "${path//Reference/${CONTEXT}}"
      done

  find "$dest" -type f \( -name '*.cs' -o -name '*.csproj' -o -name '*.json' \) -print0 \
    | while IFS= read -r -d '' file; do
        sed -i \
          -e "s/Reference/${CONTEXT}/g" \
          -e "s/reference/${lower_context}/g" \
          "$file"
      done
}

echo "Scaffolding ${CONTEXT} module at ${DEST_MODULE}"
copy_and_rename "$SOURCE_MODULE" "$DEST_MODULE"
copy_and_rename "$SOURCE_TESTS" "$DEST_TESTS"

SLNX="${TARGET_ROOT}/F2pPlatform.slnx"
if [[ -f "$SLNX" ]] && ! grep -q "${CONTEXT}.Domain" "$SLNX"; then
  python3 - <<PY
from pathlib import Path
slnx = Path("${SLNX}")
context = "${CONTEXT}"
insert_src = f'''    <Project Path="src/Modules/{context}/{context}.Domain/{context}.Domain.csproj" />
    <Project Path="src/Modules/{context}/{context}.Application/{context}.Application.csproj" />
    <Project Path="src/Modules/{context}/{context}.Infrastructure/{context}.Infrastructure.csproj" />
    <Project Path="src/Modules/{context}/{context}.Api/{context}.Api.csproj" />'''
insert_tests = f'''    <Project Path="tests/Modules/{context}/{context}.Unit.Tests/{context}.Unit.Tests.csproj" />
    <Project Path="tests/Modules/{context}/{context}.Characterization.Tests/{context}.Characterization.Tests.csproj" />'''
text = slnx.read_text()
if f"/Modules/{context}/" not in text:
    text = text.replace(
        '  <Folder Name="/src/Modules/Reference/">',
        f'  <Folder Name="/src/Modules/{context}/">\n{insert_src}\n  </Folder>\n  <Folder Name="/src/Modules/Reference/">',
        1,
    )
    text = text.replace(
        '  <Folder Name="/tests/Modules/Reference/">',
        f'  <Folder Name="/tests/Modules/{context}/">\n{insert_tests}\n  </Folder>\n  <Folder Name="/tests/Modules/Reference/">',
        1,
    )
    slnx.write_text(text)
    print(f"Updated {slnx}")
PY
fi

echo ""
echo "Next steps:"
echo "  1. Register in host Program.cs:"
echo "       builder.Services.Add${CONTEXT}Module(builder.Configuration);"
echo "       app.Map${CONTEXT}Module();"
echo "  2. dotnet build ${TARGET_ROOT}"
echo "  3. dotnet test ${TARGET_ROOT}"
