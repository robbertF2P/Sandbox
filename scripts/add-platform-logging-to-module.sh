#!/usr/bin/env bash
set -euo pipefail

# Scaffold Platform.Serilog.Logging adoption for a new module.
# Usage: ./scripts/add-platform-logging-to-module.sh <ModuleRootRelativePath>
# Example: ./scripts/add-platform-logging-to-module.sh MyNewModule

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <module-root-relative-path>" >&2
  exit 1
fi

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
module="${root}/$1"
build="${root}/build"

if [[ ! -d "${module}" ]]; then
  echo "Module directory not found: ${module}" >&2
  exit 1
fi

props="${module}/Directory.Packages.props"
versions_import='  <Import Project="..\build\Platform.Logging.Versions.props" />'

if [[ ! -f "${props}" ]]; then
  cat > "${props}" <<EOF
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

${versions_import}
</Project>
EOF
  echo "Created ${props}"
elif ! grep -q 'Platform.Logging.Versions.props' "${props}"; then
  # Insert import after opening <Project> or after PropertyGroup
  python3 - <<PY
from pathlib import Path
path = Path("${props}")
text = path.read_text()
import_line = '${versions_import}\n'
if '<PropertyGroup>' in text and import_line.strip() not in text:
    text = text.replace('</PropertyGroup>', f'</PropertyGroup>\n\n{import_line}', 1)
elif import_line.strip() not in text:
    text = text.replace('<Project>', f'<Project>\n{import_line}', 1)
path.write_text(text)
PY
  echo "Updated ${props} with Platform.Logging.Versions import"
else
  echo "${props} already imports Platform.Logging.Versions.props"
fi

depth="$(python3 -c "import os; print(os.path.relpath('${build}', '${module}').replace(os.sep, '/'))")"

echo ""
echo "Add one of these imports to each project .csproj (adjust path if needed):"
echo "  Host:    <Import Project=\"${depth}/Platform.Logging.Host.props\" />"
echo "  Library: <Import Project=\"${depth}/Platform.Logging.Library.props\" />"
echo "  Tests:   <Import Project=\"${depth}/Platform.Logging.Tests.props\" />"
echo ""
echo "Host Program.cs:"
echo "  builder.AddPlatformLogging(\"Your Application Name\");"
echo "  app.UsePlatformRequestLogging();"
echo ""
echo "See docs/monolith-modularization/platform-logging-standard.md"
