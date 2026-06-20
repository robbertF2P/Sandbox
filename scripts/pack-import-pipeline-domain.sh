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
  -p:IsPackable=true
)

if [[ -n "${version}" ]]; then
  args+=(--property:PackageVersion="${version}")
fi

args+=(
  -p:PackageId=ImportPipeline.Domain
  -p:Title="Import Pipeline Domain"
  -p:Description="Config-driven spreadsheet row mapping domain model (ImportRow, ImportConfigRule, ImportRowMapper)."
  -p:Authors="Robbert-Driven-It"
  -p:RepositoryUrl="https://github.com/Robbert-Driven-It/SandBox"
  -p:PackageTags="import;pipeline;domain;ddd;excel"
)

dotnet "${args[@]}"

echo "Packed ImportPipeline.Domain to ${feed}"
