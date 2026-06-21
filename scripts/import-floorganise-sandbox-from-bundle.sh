#!/usr/bin/env bash
set -euo pipefail

# Import the export bundle into a local robbertF2P/Sandbox clone and push.
# Requires push access to https://github.com/robbertF2P/Sandbox
#
#   ./scripts/import-floorganise-sandbox-from-bundle.sh

BUNDLE_URL="${BUNDLE_URL:-https://github.com/Robbert-Driven-It/SandBox/releases/download/floorganise-sandbox-export/floorganise-sandbox.bundle}"
EXPORT_REF="${EXPORT_REF:-cursor/export-f2p-refactor-for-floorganise-7eea}"
TARGET_REMOTE="${FLOORORGANISE_SANDBOX_REMOTE:-https://github.com/robbertF2P/Sandbox.git}"
TARGET_BRANCH="${TARGET_BRANCH:-main}"

WORKDIR="$(mktemp -d)"
trap 'rm -rf "$WORKDIR"' EXIT

curl -fsSL -o "$WORKDIR/export.bundle" "$BUNDLE_URL"
git clone "$TARGET_REMOTE" "$WORKDIR/target"
cd "$WORKDIR/target"
git pull "$WORKDIR/export.bundle" "${EXPORT_REF}:${TARGET_BRANCH}"
git push origin "${TARGET_BRANCH}"

echo "Done. Imported ${EXPORT_REF} into ${TARGET_REMOTE} (${TARGET_BRANCH})."
