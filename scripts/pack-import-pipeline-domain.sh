#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
feed="${root}/local-feed"
project="${root}/ImportPipeline/src/ImportPipeline.Domain/ImportPipeline.Domain.csproj"
version="${1:-}"

mkdir -p "${feed}"

args=(
  pack "${project}"
  -c Release
  -o "${feed}"
)

if [[ -n "${version}" ]]; then
  args+=(--property:PackageVersion="${version}")
fi

dotnet "${args[@]}"

echo "Packed ImportPipeline.Domain to ${feed}"
