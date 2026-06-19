#!/usr/bin/env bash
# Sync Agent Skills from .cursor/skills/ to Copilot and shared agent paths.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SRC="$ROOT/.cursor/skills"

for target in "$ROOT/.github/skills" "$ROOT/.agents/skills"; do
  mkdir -p "$target"
  for skill_dir in "$SRC"/*/; do
    name="$(basename "$skill_dir")"
    mkdir -p "$target/$name"
    cp "$skill_dir/SKILL.md" "$target/$name/SKILL.md"
    echo "Synced $name -> $target/$name/SKILL.md"
  done
done

echo "Done."
