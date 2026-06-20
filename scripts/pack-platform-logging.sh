#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
feed="${root}/local-feed"
version="${1:-1.0.0}"

mkdir -p "${feed}"

pack() {
  local project="$1"
  local package_id="$2"
  local title="$3"
  local description="$4"
  local tags="$5"
  local configfile="${6:-}"

  local args=(
    pack "${project}"
    -c Release
    -o "${feed}"
    "-p:IsPackable=true"
    "-p:PackageVersion=${version}"
    "-p:PackageId=${package_id}"
    "-p:Title=${title}"
    "-p:Description=${description}"
    "-p:Authors=Robbert-Driven-It"
    "-p:RepositoryUrl=https://github.com/Robbert-Driven-It/SandBox"
    "-p:PackageTags=${tags}"
  )

  if [[ -n "${configfile}" ]]; then
    args+=(--configfile "${configfile}")
  fi

  dotnet "${args[@]}"
}

pack \
  "${root}/Platform.Serilog.Logging/src/Platform.Serilog.Logging/Platform.Serilog.Logging.csproj" \
  "Platform.Serilog.Logging" \
  "Platform Serilog Logging" \
  "Central Serilog configuration with Seq in Development and Application Insights in Production" \
  "serilog logging seq application-insights platform"

pack \
  "${root}/Platform.Serilog.Logging/src/Platform.Serilog.Logging.Testing/Platform.Serilog.Logging.Testing.csproj" \
  "Platform.Serilog.Logging.Testing" \
  "Platform Serilog Logging Testing" \
  "xUnit Serilog test sink wired to the shared platform logging pipeline." \
  "serilog logging testing xunit platform" \
  "${root}/build/nuget.pack.config"

echo "Packed Platform.Serilog.Logging packages to ${feed}"
