#!/usr/bin/env bash
set -euo pipefail

# One-shot: push the driven-it export branch to robbertF2P/Sandbox (main).
# Run from any machine where YOU have push access to https://github.com/robbertF2P/Sandbox
#
#   ./scripts/push-export-to-floorganise-sandbox.sh
#
# Optional: FLOORORGANISE_SANDBOX_REMOTE=git@github.com:robbertF2P/Sandbox.git

EXPORT_BRANCH="${EXPORT_BRANCH:-cursor/export-f2p-refactor-for-floorganise-7eea}"
SOURCE_REPO="${SOURCE_REPO:-https://github.com/Robbert-Driven-It/SandBox.git}"
TARGET_REMOTE="${FLOORORGANISE_SANDBOX_REMOTE:-https://github.com/robbertF2P/Sandbox.git}"
TARGET_BRANCH="${TARGET_BRANCH:-main}"

WORKDIR="$(mktemp -d)"
trap 'rm -rf "$WORKDIR"' EXIT

git clone --branch "$EXPORT_BRANCH" --single-branch "$SOURCE_REPO" "$WORKDIR"
cd "$WORKDIR"
git remote set-url origin "$TARGET_REMOTE"
git push origin "HEAD:$TARGET_BRANCH"

echo "Done. Floorganise sandbox updated at $TARGET_REMOTE ($TARGET_BRANCH)."
