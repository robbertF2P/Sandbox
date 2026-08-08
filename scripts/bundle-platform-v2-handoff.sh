#!/usr/bin/env bash
set -euo pipefail

# Bundle Platform 2.0 plan docs, starter kit, pilots, and agent rules for laptop handoff.
#
# Usage:
#   ./scripts/bundle-platform-v2-handoff.sh
#   ./scripts/bundle-platform-v2-handoff.sh --skip-pack
#   ./scripts/bundle-platform-v2-handoff.sh --output-dir /tmp/handoff
#
# Creates:
#   <output-dir>/platform-v2-handoff/          (staging tree)
#   <output-dir>/platform-v2-handoff-<stamp>.tar.gz
#   <output-dir>/platform-v2-handoff-<stamp>.zip (when zip available)

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="${OUTPUT_DIR:-${ROOT}/dist}"
SKIP_PACK=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --skip-pack)
      SKIP_PACK=1
      shift
      ;;
    --output-dir)
      OUTPUT_DIR="$2"
      shift 2
      ;;
    *)
      echo "Unknown argument: $1" >&2
      echo "Usage: $0 [--skip-pack] [--output-dir <dir>]" >&2
      exit 1
      ;;
  esac
done

if [[ "${HANDOFF_PACK_NUGETS:-1}" == "0" ]]; then
  SKIP_PACK=1
fi

STAMP="$(date +%Y%m%d)"
GIT_SHA="$(git -C "$ROOT" rev-parse --short HEAD 2>/dev/null || echo nogit)"
BUNDLE_NAME="platform-v2-handoff-${STAMP}-${GIT_SHA}"
STAGE="${OUTPUT_DIR}/${BUNDLE_NAME}"
ARCHIVE_BASE="${OUTPUT_DIR}/${BUNDLE_NAME}"

TAR_EXCLUDES=(
  --exclude='bin'
  --exclude='obj'
  --exclude='node_modules'
  --exclude='.git'
  --exclude='dist'
  --exclude='.vs'
  --exclude='*.user'
)

# Copy directory contents into dest (dest becomes root of copied tree).
copy_tree_contents() {
  local src="$1"
  local dest="$2"
  if [[ ! -d "$src" ]]; then
    echo "Warning: missing source $src — skipping" >&2
    return 0
  fi
  mkdir -p "$dest"
  tar -cf - -C "$src" "${TAR_EXCLUDES[@]}" . | tar -xf - -C "$dest"
}

# Copy directory as a named child (e.g. F2pPlatform -> pilot/F2pPlatform).
copy_tree() {
  local src="$1"
  local dest="$2"
  if [[ ! -e "$src" ]]; then
    echo "Warning: missing source $src — skipping" >&2
    return 0
  fi
  mkdir -p "$(dirname "$dest")"
  rm -rf "$dest"
  mkdir -p "$dest"
  tar -cf - -C "$src" "${TAR_EXCLUDES[@]}" . | tar -xf - -C "$dest"
}

echo "==> Staging handoff bundle at ${STAGE}"
rm -rf "$STAGE"
mkdir -p "$STAGE"

if [[ "$SKIP_PACK" -eq 0 ]] && command -v dotnet >/dev/null 2>&1; then
  echo "==> Packing local NuGet packages"
  "${ROOT}/scripts/pack-local-platform-packages.sh"
else
  if [[ "$SKIP_PACK" -eq 0 ]]; then
    echo "==> dotnet not found — skipping NuGet pack (use --skip-pack or install SDK)" >&2
  fi
fi

# Core docs (monolith path names)
mkdir -p "$STAGE/docs/modularization"
copy_tree_contents "${ROOT}/docs/monolith-modularization" "$STAGE/docs/modularization"

mkdir -p "$STAGE/docs/modularization/examples"
if [[ -d "${ROOT}/docs/Modularization" ]]; then
  copy_tree_contents "${ROOT}/docs/Modularization" "$STAGE/docs/modularization/examples"
fi

mkdir -p "$STAGE/docs"
for doc in floor2plan-v2-read-model-playbook.md floor2plan-v2-connector-architecture.md \
  floor2plan-v2-connector-migration-prompt.md floor2plan-v2-connector-prompt-plm-planning.md \
  floor2plan-v2-connector-prompt-eshare.md floor2plan-planning-approval-data-model.md \
  floor2plan-legacy-connector-submodule-antipattern.md; do
  if [[ -f "${ROOT}/docs/${doc}" ]]; then
    cp "${ROOT}/docs/${doc}" "$STAGE/docs/"
  fi
done

copy_tree "${ROOT}/docs/coding-standards" "$STAGE/docs/coding-standards"

# Build props
mkdir -p "$STAGE/build"
cp "${ROOT}/build/Platform.Logging."*.props "$STAGE/build/"

# Scripts needed on laptop
mkdir -p "$STAGE/scripts"
for script in scaffold-module.sh scaffold-frontend-module.sh scaffold-customization-pack.sh \
  add-platform-logging-to-module.sh pack-platform-logging.sh pack-local-platform-packages.sh \
  pack-import-pipeline-domain.sh pack-platform-control-plane-client.sh sync-agent-skills.sh; do
  if [[ -f "${ROOT}/scripts/${script}" ]]; then
    cp "${ROOT}/scripts/${script}" "$STAGE/scripts/"
  fi
done

# Pilots
mkdir -p "$STAGE/pilot"
copy_tree "${ROOT}/F2pPlatform" "$STAGE/pilot/F2pPlatform"
copy_tree "${ROOT}/PlanningApprovalsPoc" "$STAGE/pilot/PlanningApprovalsPoc"

# Platform packages
copy_tree "${ROOT}/Platform.Serilog.Logging" "$STAGE/platform/Platform.Serilog.Logging"
if [[ -d "${ROOT}/local-feed" ]]; then
  copy_tree "${ROOT}/local-feed" "$STAGE/platform/local-feed"
fi

# Agent rules + skills
mkdir -p "$STAGE/agent/skills"
if [[ -f "${ROOT}/docs/monolith-modularization/agent-instructions-snippet.md" ]]; then
  awk 'BEGIN{p=0} /^You are assisting/{p=1} p' \
    "${ROOT}/docs/monolith-modularization/agent-instructions-snippet.md" \
    > "$STAGE/agent/agent-rules.md"
  cp "$STAGE/agent/agent-rules.md" "$STAGE/agent/copilot-instructions.md"
fi
if [[ -d "${ROOT}/.cursor/skills" ]]; then
  copy_tree_contents "${ROOT}/.cursor/skills" "$STAGE/agent/skills"
fi

# Handoff helpers (from repo scripts/)
cp "${ROOT}/scripts/install-platform-v2-handoff.sh" "$STAGE/install-into-monolith.sh"
cp "${ROOT}/scripts/verify-platform-v2-handoff.sh" "$STAGE/verify-handoff.sh"
chmod +x "$STAGE/install-into-monolith.sh" "$STAGE/verify-handoff.sh"

cp "${ROOT}/docs/monolith-modularization/PLATFORM-V2-LAPTOP-HANDOFF.md" "$STAGE/HANDOFF.md"

{
  echo "bundle=${BUNDLE_NAME}"
  echo "created=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  echo "git_sha=${GIT_SHA}"
  echo "source_repo=SandBox"
  echo "skip_pack=${SKIP_PACK}"
} > "$STAGE/VERSION.txt"

mkdir -p "$OUTPUT_DIR"
echo "==> Creating archives"
tar -czf "${ARCHIVE_BASE}.tar.gz" -C "$OUTPUT_DIR" "$BUNDLE_NAME"

if command -v zip >/dev/null 2>&1; then
  (cd "$OUTPUT_DIR" && zip -rq "${BUNDLE_NAME}.zip" "$BUNDLE_NAME")
  echo "Created ${ARCHIVE_BASE}.zip"
fi

echo "Created ${ARCHIVE_BASE}.tar.gz"
echo "Staging tree: ${STAGE}"
echo ""
echo "Next: copy the .tar.gz to your laptop, extract, then:"
echo "  ./verify-handoff.sh"
echo "  ./install-into-monolith.sh /path/to/Floor2Plan.Core2"
