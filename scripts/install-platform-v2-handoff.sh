#!/usr/bin/env bash
set -euo pipefail

# Install Platform 2.0 handoff bundle into the Floor2Plan monolith.
#
# Usage (from extracted bundle root):
#   ./install-into-monolith.sh /path/to/Floor2Plan.Core2
#   ./install-into-monolith.sh /path/to/Floor2Plan.Core2 --with-pilot-reference
#
# Usage (from SandBox repo):
#   ./scripts/install-platform-v2-handoff.sh /path/to/Floor2Plan.Core2

BUNDLE_ROOT=""
MONO_ROOT=""
WITH_PILOT=0
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -f "${SCRIPT_DIR}/VERSION.txt" ]] && [[ -d "${SCRIPT_DIR}/docs/modularization" ]]; then
  BUNDLE_ROOT="$SCRIPT_DIR"
fi

usage() {
  echo "Usage: $0 <monolith-root> [--with-pilot-reference]" >&2
  echo "Run from extracted handoff bundle root (install-into-monolith.sh) or pass bundle via HANDOFF_BUNDLE_ROOT." >&2
  exit 1
}

if [[ -n "${HANDOFF_BUNDLE_ROOT:-}" ]]; then
  BUNDLE_ROOT="$HANDOFF_BUNDLE_ROOT"
fi

if [[ $# -lt 1 ]]; then
  usage
fi

MONO_ROOT="$(cd "$1" && pwd)"
shift

while [[ $# -gt 0 ]]; do
  case "$1" in
    --with-pilot-reference)
      WITH_PILOT=1
      shift
      ;;
    --bundle-root)
      BUNDLE_ROOT="$2"
      shift 2
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      ;;
  esac
done

if [[ -z "$BUNDLE_ROOT" ]] || [[ ! -d "$BUNDLE_ROOT/docs/modularization" ]]; then
  echo "Could not find handoff bundle. Extract the archive or set HANDOFF_BUNDLE_ROOT." >&2
  exit 1
fi

MONO_PLATFORM_DIR="${MONO_PLATFORM_DIR:-Src/Platform/F2pPlatform}"

TAR_EXCLUDES=(
  --exclude='bin'
  --exclude='obj'
  --exclude='node_modules'
  --exclude='.git'
)

copy_tree_contents() {
  local src="$1"
  local dest="$2"
  mkdir -p "$dest"
  tar -cf - -C "$src" "${TAR_EXCLUDES[@]}" . | tar -xf - -C "$dest"
}

echo "==> Installing from ${BUNDLE_ROOT}"
echo "==> Monolith root: ${MONO_ROOT}"

mkdir -p "${MONO_ROOT}/docs/modularization"
copy_tree_contents "${BUNDLE_ROOT}/docs/modularization" "${MONO_ROOT}/docs/modularization"

if [[ -d "${BUNDLE_ROOT}/docs/coding-standards" ]]; then
  mkdir -p "${MONO_ROOT}/docs/coding-standards"
  copy_tree_contents "${BUNDLE_ROOT}/docs/coding-standards" "${MONO_ROOT}/docs/coding-standards"
fi

for doc in "${BUNDLE_ROOT}"/docs/floor2plan-*.md; do
  if [[ -f "$doc" ]]; then
    cp "$doc" "${MONO_ROOT}/docs/"
  fi
done

mkdir -p "${MONO_ROOT}/Build/Platform"
cp "${BUNDLE_ROOT}/build/"*.props "${MONO_ROOT}/Build/Platform/"

mkdir -p "${MONO_ROOT}/scripts"
for script in scaffold-module.sh scaffold-frontend-module.sh scaffold-customization-pack.sh \
  add-platform-logging-to-module.sh pack-platform-logging.sh pack-local-platform-packages.sh \
  pack-import-pipeline-domain.sh pack-platform-control-plane-client.sh; do
  if [[ -f "${BUNDLE_ROOT}/scripts/${script}" ]]; then
    cp "${BUNDLE_ROOT}/scripts/${script}" "${MONO_ROOT}/scripts/"
    chmod +x "${MONO_ROOT}/scripts/${script}"
  fi
done

if [[ -f "${BUNDLE_ROOT}/agent/agent-rules.md" ]]; then
  cp "${BUNDLE_ROOT}/agent/agent-rules.md" "${MONO_ROOT}/docs/modularization/agent-rules.md"
fi

mkdir -p "${MONO_ROOT}/.github"
if [[ -f "${BUNDLE_ROOT}/agent/copilot-instructions.md" ]]; then
  cp "${BUNDLE_ROOT}/agent/copilot-instructions.md" "${MONO_ROOT}/.github/copilot-instructions.md"
fi

if [[ "$WITH_PILOT" -eq 1 ]] && [[ -d "${BUNDLE_ROOT}/pilot/F2pPlatform" ]]; then
  mkdir -p "${MONO_ROOT}/${MONO_PLATFORM_DIR}"
  rm -rf "${MONO_ROOT}/${MONO_PLATFORM_DIR}"
  copy_tree_contents "${BUNDLE_ROOT}/pilot/F2pPlatform" "${MONO_ROOT}/${MONO_PLATFORM_DIR}"
  echo "==> Copied pilot F2pPlatform to ${MONO_ROOT}/${MONO_PLATFORM_DIR}"
fi

cat <<EOF

Done.

Next steps:
  1. Read ${MONO_ROOT}/docs/modularization/foundation-and-pilot-plan.md (Phase A)
  2. Configure nuget.config for Platform.Serilog.Logging (feed or local ${BUNDLE_ROOT}/platform/local-feed)
  3. Phase 0 inventory: docs/modularization/analysis-instructions.md
  4. Optional: copy agent/skills from bundle to your IDE config

EOF
