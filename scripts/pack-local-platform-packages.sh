#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
version="${1:-1.0.0}"

"${root}/scripts/pack-import-pipeline-domain.sh" "${version}"
"${root}/scripts/pack-platform-logging.sh" "${version}"

echo "All local platform packages packed to ${root}/local-feed"
