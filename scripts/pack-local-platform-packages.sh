#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
version="${1:-$(sed -n 's/.*<PlatformSerilogLoggingVersion>\([^<]*\)<\/PlatformSerilogLoggingVersion>.*/\1/p' "${root}/build/Platform.Logging.Versions.props" | head -n 1)}"

if [[ -z "${version}" ]]; then
  echo "Could not determine Platform.Serilog.Logging version." >&2
  exit 1
fi

"${root}/scripts/pack-import-pipeline-domain.sh" "${version}"
"${root}/scripts/pack-platform-control-plane-client.sh" "${version}"
"${root}/scripts/pack-platform-logging.sh" "${version}"

echo "All local platform packages packed to ${root}/local-feed (version ${version})"
