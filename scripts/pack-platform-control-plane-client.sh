#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
version="${1:-1.0.0}"
output="${root}/local-feed"

mkdir -p "${output}"

dotnet pack "${root}/Platform.ControlPlane.Client/Platform.ControlPlane.Client.csproj" \
  -c Release \
  -o "${output}" \
  /p:PackageVersion="${version}"

echo "Packed Platform.ControlPlane.Client ${version} to ${output}"
