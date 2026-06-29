#!/usr/bin/env bash
# Report NuGet vulnerabilities to Jira (create or comment on existing issues).
#
# Usage:
#   report-nuget-vulnerabilities-to-jira.sh --mode audit|ci-failure \
#     [--restore-log path] [--build-url url] [--solution path]
#
# Required env:
#   JIRA_BASE_URL, JIRA_USER_EMAIL, JIRA_API_TOKEN, JIRA_PROJECT_KEY
#
# Optional env:
#   JIRA_ISSUE_TYPE (default: Task)
#   BUILD_SOURCEBRANCH, BUILD_REPOSITORY_NAME
#
# Requires: bash, curl, jq, dotnet SDK 8+

set -euo pipefail

MODE=""
RESTORE_LOG=""
BUILD_URL=""
SOLUTION=""

usage() {
  echo "Usage: $0 --mode audit|ci-failure [--restore-log path] [--build-url url] [--solution path]" >&2
  exit 1
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --mode) MODE="$2"; shift 2 ;;
    --restore-log) RESTORE_LOG="$2"; shift 2 ;;
    --build-url) BUILD_URL="$2"; shift 2 ;;
    --solution) SOLUTION="$2"; shift 2 ;;
    -h|--help) usage ;;
    *) echo "Unknown argument: $1" >&2; usage ;;
  esac
done

[[ -n "$MODE" ]] || usage

for var in JIRA_BASE_URL JIRA_USER_EMAIL JIRA_API_TOKEN JIRA_PROJECT_KEY; do
  if [[ -z "${!var:-}" ]]; then
    echo "Missing required environment variable: $var" >&2
    exit 1
  fi
done

JIRA_ISSUE_TYPE="${JIRA_ISSUE_TYPE:-Task}"
AUTH="$(printf '%s:%s' "$JIRA_USER_EMAIL" "$JIRA_API_TOKEN" | base64 -w0 2>/dev/null || printf '%s:%s' "$JIRA_USER_EMAIL" "$JIRA_API_TOKEN" | base64)"

jira_request() {
  local method="$1"
  local path="$2"
  local body="${3:-}"
  if [[ -n "$body" ]]; then
    curl -fsS -X "$method" \
      -H "Authorization: Basic $AUTH" \
      -H "Content-Type: application/json" \
      -d "$body" \
      "${JIRA_BASE_URL%/}/rest/api/3${path}"
  else
    curl -fsS -X "$method" \
      -H "Authorization: Basic $AUTH" \
      -H "Content-Type: application/json" \
      "${JIRA_BASE_URL%/}/rest/api/3${path}"
  fi
}

severity_from_code() {
  case "$1" in
    NU1904) echo "Critical" ;;
    NU1903) echo "High" ;;
    NU1902) echo "Moderate" ;;
    NU1901) echo "Low" ;;
    *) echo "Unknown" ;;
  esac
}

jira_priority_from_severity() {
  case "$1" in
    Critical) echo "Highest" ;;
    High) echo "High" ;;
    Moderate) echo "Medium" ;;
    Low) echo "Low" ;;
    *) echo "Medium" ;;
  esac
}

# Parse NU190x lines from restore log (fallback when JSON list is unavailable)
parse_restore_log() {
  local log="$1"
  [[ -f "$log" ]] || return 0
  grep -oE 'NU190[1-4].*' "$log" | while IFS= read -r line; do
    local code
    code="$(echo "$line" | grep -oE 'NU190[1-4]' | head -1)"
    local pkg ver url
    pkg="$(echo "$line" | sed -nE 's/.*Package (.+?) [0-9].*/\1/p' | head -1)"
    ver="$(echo "$line" | sed -nE 's/.*Package [^ ]+ ([0-9][^ ]*).*/\1/p' | head -1)"
    url="$(echo "$line" | grep -oE 'https://[^ ]+' | head -1)"
    if [[ -n "$pkg" ]]; then
      jq -n \
        --arg code "$code" \
        --arg pkg "$pkg" \
        --arg ver "${ver:-unknown}" \
        --arg url "${url:-}" \
        --arg sev "$(severity_from_code "$code")" \
        '{code: $code, packageId: $pkg, resolvedVersion: $ver, advisoryUrl: $url, severity: $sev, projects: []}'
    fi
  done
}

collect_vulnerabilities_json() {
  local tmp
  tmp="$(mktemp)"
  if [[ -n "$SOLUTION" ]]; then
    dotnet list "$SOLUTION" package --vulnerable --include-transitive --format json >"$tmp" 2>/dev/null || true
  else
    # Find first solution in repo root
    local sln
    sln="$(find . -maxdepth 3 -name '*.sln' -o -name '*.slnx' 2>/dev/null | head -1 || true)"
    if [[ -n "$sln" ]]; then
      dotnet list "$sln" package --vulnerable --include-transitive --format json >"$tmp" 2>/dev/null || true
    else
      echo '{"projects":[]}' >"$tmp"
    fi
  fi

  if jq -e '.projects' "$tmp" >/dev/null 2>&1; then
  jq -c '
    .projects[]? as $p
    | ($p.frameworks[]? // empty) as $fw
    | ($fw.topLevelPackages[]? // empty) as $tl
    | ($tl.vulnerabilities[]? // empty) as $v
    | {
        packageId: $tl.id,
        resolvedVersion: $tl.resolvedVersion,
        severity: $v.severity,
        advisoryUrl: $v.advisoryUrl,
        code: (
          if $v.severity == "Critical" then "NU1904"
          elif $v.severity == "High" then "NU1903"
          elif $v.severity == "Moderate" then "NU1902"
          else "NU1901" end
        ),
        projects: [$p.path]
      }
    ' "$tmp" 2>/dev/null || true
  fi
  rm -f "$tmp"
}

collect_vulnerabilities() {
  local from_json=()
  while IFS= read -r line; do
    [[ -n "$line" ]] && from_json+=("$line")
  done < <(collect_vulnerabilities_json)

  if [[ ${#from_json[@]} -gt 0 ]]; then
    printf '%s\n' "${from_json[@]}"
    return
  fi

  if [[ -n "$RESTORE_LOG" ]]; then
    parse_restore_log "$RESTORE_LOG"
  fi
}

advisory_id_from_url() {
  local url="$1"
  if [[ "$url" =~ GHSA-[a-z0-9-]+ ]]; then
    echo "${BASH_REMATCH[0]}"
  elif [[ "$url" =~ CVE-[0-9]+-[0-9]+ ]]; then
    echo "${BASH_REMATCH[0]}"
  else
    echo "$url" | sed 's|/$||' | awk -F/ '{print $NF}'
  fi
}

find_existing_issue() {
  local pkg="$1"
  local advisory_id="$2"
  local jql
  jql=$(jq -n \
    --arg project "$JIRA_PROJECT_KEY" \
    --arg pkg "$pkg" \
    --arg adv "$advisory_id" \
    '"project = \($project) AND labels = nuget-audit AND status != Done AND summary ~ \"\($pkg)\" AND text ~ \"\($adv)\""' \
    -r)
  local encoded
  encoded="$(printf '%s' "$jql" | jq -sRr @uri)"
  jira_request GET "/search?jql=${encoded}&maxResults=1&fields=key" | jq -r '.issues[0].key // empty'
}

build_description_adf() {
  local vuln_json="$1"
  local pkg ver sev url adv projects_json agent_json
  pkg="$(echo "$vuln_json" | jq -r '.packageId')"
  ver="$(echo "$vuln_json" | jq -r '.resolvedVersion')"
  sev="$(echo "$vuln_json" | jq -r '.severity')"
  url="$(echo "$vuln_json" | jq -r '.advisoryUrl // ""')"
  adv="$(advisory_id_from_url "$url")"
  projects_json="$(echo "$vuln_json" | jq -c '.projects')"
  agent_json="$(jq -n \
    --arg ecosystem "nuget" \
    --arg packageId "$pkg" \
    --arg resolvedVersion "$ver" \
    --arg severity "$sev" \
    --arg advisoryUrl "$url" \
    --arg advisoryId "$adv" \
    --argjson projects "$projects_json" \
    --arg buildUrl "${BUILD_URL:-}" \
    --arg repo "${BUILD_REPOSITORY_NAME:-}" \
    --arg branch "${BUILD_SOURCEBRANCH:-}" \
    --arg mode "$MODE" \
    '{ecosystem: $ecosystem, packageId: $packageId, resolvedVersion: $resolvedVersion, severity: $severity, advisoryUrl: $advisoryUrl, advisoryId: $advisoryId, projects: $projects, buildUrl: $buildUrl, repo: $repo, branch: $branch, mode: $mode}')"

  local md
  md="Restore audit detected vulnerable package **${pkg} ${ver}** (${sev}).

## Affected projects
$(echo "$vuln_json" | jq -r '.projects[]? // empty' | sed 's/^/- /')

## Links
- Build: ${BUILD_URL:-n/a}
- Advisory: ${url:-n/a}

## Agent task
1. Upgrade or replace the vulnerable package (prefer Directory.Packages.props).
2. Run dotnet list package --vulnerable until clean.
3. Run dotnet build and tests for affected modules.
4. Do not suppress NU190x without a separate security review ticket.

\`\`\`agent-context
${agent_json}
\`\`\`"

  jq -n --arg md "$md" '{
    version: 1,
    type: "doc",
    content: [
      {
        type: "paragraph",
        content: [{type: "text", text: $md}]
      }
    ]
  }'
}

create_issue() {
  local vuln_json="$1"
  local pkg ver sev url adv priority summary description fields
  pkg="$(echo "$vuln_json" | jq -r '.packageId')"
  ver="$(echo "$vuln_json" | jq -r '.resolvedVersion')"
  sev="$(echo "$vuln_json" | jq -r '.severity')"
  url="$(echo "$vuln_json" | jq -r '.advisoryUrl // ""')"
  adv="$(advisory_id_from_url "$url")"
  priority="$(jira_priority_from_severity "$sev")"
  summary="[NuGet] ${sev} — ${pkg}@${ver} (${adv})"
  description="$(build_description_adf "$vuln_json")"

  fields="$(jq -n \
    --arg project "$JIRA_PROJECT_KEY" \
    --arg type "$JIRA_ISSUE_TYPE" \
    --arg summary "$summary" \
    --arg priority "$priority" \
    --argjson description "$description" \
    '{
      fields: {
        project: {key: $project},
        issuetype: {name: $type},
        summary: $summary,
        priority: {name: $priority},
        labels: ["nuget-audit", "security", "ai-actionable", "ado-detected"],
        description: $description
      }
    }')"

  jira_request POST "/issue" "$fields" | jq -r '.key'
}

comment_issue() {
  local issue_key="$1"
  local vuln_json="$2"
  local body
  body="$(jq -n \
    --arg build "${BUILD_URL:-}" \
    --arg mode "$MODE" \
    --arg when "$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
  '{
    body: {
      version: 1,
      type: "doc",
      content: [{
        type: "paragraph",
        content: [{type: "text", text: ("NuGet audit still failing (" + $mode + ") at " + $when + ". Build: " + $build)}]
      }]
    }
  }')"
  jira_request POST "/issue/${issue_key}/comment" "$body" >/dev/null
  echo "$issue_key"
}

main() {
  local count=0
  local vulns=()

  while IFS= read -r line; do
    [[ -n "$line" ]] && vulns+=("$line")
  done < <(collect_vulnerabilities)

  if [[ ${#vulns[@]} -eq 0 ]]; then
    echo "No vulnerable packages detected."
    exit 0
  fi

  echo "Found ${#vulns[@]} vulnerable package(s)."

  for vuln_json in "${vulns[@]}"; do
  local pkg url adv existing
  pkg="$(echo "$vuln_json" | jq -r '.packageId')"
  url="$(echo "$vuln_json" | jq -r '.advisoryUrl // ""')"
  adv="$(advisory_id_from_url "$url")"
  existing="$(find_existing_issue "$pkg" "$adv")"

  if [[ -n "$existing" ]]; then
    echo "Updating existing issue $existing for $pkg ($adv)"
    comment_issue "$existing" "$vuln_json"
  else
    echo "Creating issue for $pkg ($adv)"
    key="$(create_issue "$vuln_json")"
    echo "Created $key"
  fi
  count=$((count + 1))
  done

  echo "Processed $count vulnerability record(s)."
}

main
