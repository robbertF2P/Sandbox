#!/usr/bin/env bash
set -euo pipefail

# Verify a Platform 2.0 handoff bundle (extracted tree or SandBox staging folder).
#
# Usage:
#   ./verify-handoff.sh
#   ./verify-handoff.sh /path/to/platform-v2-handoff-*

ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)}"
if [[ -f "${ROOT}/install-into-monolith.sh" ]]; then
  BUNDLE_ROOT="$ROOT"
else
  BUNDLE_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  if [[ ! -f "${BUNDLE_ROOT}/VERSION.txt" ]]; then
    echo "Pass bundle root or run from extracted handoff folder." >&2
    exit 1
  fi
fi

fail=0
check() {
  if [[ -e "$1" ]]; then
    echo "OK  $1"
  else
    echo "MISS $1" >&2
    fail=1
  fi
}

echo "==> Verifying bundle at ${BUNDLE_ROOT}"
check "${BUNDLE_ROOT}/VERSION.txt"
check "${BUNDLE_ROOT}/docs/modularization/foundation-and-pilot-plan.md"
check "${BUNDLE_ROOT}/docs/modularization/starter-kit/README.md"
check "${BUNDLE_ROOT}/build/Platform.Logging.Versions.props"
check "${BUNDLE_ROOT}/pilot/F2pPlatform/F2pPlatform.slnx"
check "${BUNDLE_ROOT}/pilot/PlanningApprovalsPoc"
check "${BUNDLE_ROOT}/agent/agent-rules.md"
check "${BUNDLE_ROOT}/install-into-monolith.sh"

if [[ -d "${BUNDLE_ROOT}/platform/local-feed" ]]; then
  nupkg_count="$(find "${BUNDLE_ROOT}/platform/local-feed" -name '*.nupkg' 2>/dev/null | wc -l)"
  echo "OK  platform/local-feed (${nupkg_count} nupkg files)"
else
  echo "WARN platform/local-feed missing — run pack-local-platform-packages.sh" >&2
fi

if command -v dotnet >/dev/null 2>&1; then
  echo "==> dotnet build pilot/F2pPlatform"
  if [[ -d "${BUNDLE_ROOT}/platform/local-feed" ]]; then
    export NUGET_PACKAGES="${NUGET_PACKAGES:-${HOME}/.nuget/packages}"
  fi
  (cd "${BUNDLE_ROOT}/pilot/F2pPlatform" && dotnet build -v q) && echo "OK  F2pPlatform build" || fail=1
  echo "==> dotnet test PlanningApprovalsPoc"
  (cd "${BUNDLE_ROOT}/pilot/PlanningApprovalsPoc" && dotnet test -v q) && echo "OK  PlanningApprovalsPoc tests" || fail=1
else
  echo "SKIP dotnet checks (SDK not installed)"
fi

if [[ "$fail" -ne 0 ]]; then
  echo "Verification failed." >&2
  exit 1
fi

echo "Verification passed."
