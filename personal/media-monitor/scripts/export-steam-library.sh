#!/usr/bin/env bash
set -euo pipefail

# Re-export Steam library (used by media-monitor taste profile).
API_KEY="${Steamapikey:-${STEAMAPIKEY:-}}"
STEAM_ID="${Steamid:-${STEAMID:-}}"
OUTPUT_DIR="${STEAM_LIBRARY_OUTPUT_DIR:-/opt/cursor/artifacts}"
OUTPUT_JSON="${OUTPUT_DIR}/steam-library.json"
OUTPUT_CSV="${OUTPUT_DIR}/steam-library.csv"

if [[ -z "$API_KEY" || -z "$STEAM_ID" ]]; then
  echo "Missing Steam credentials." >&2
  echo "Set environment secrets Steamapikey and Steamid (or STEAMAPIKEY / STEAMID)." >&2
  exit 1
fi

mkdir -p "$OUTPUT_DIR"

url="https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/"
params=(
  "key=${API_KEY}"
  "steamid=${STEAM_ID}"
  "format=json"
  "include_appinfo=1"
  "include_played_free_games=1"
)

query=""
for p in "${params[@]}"; do
  [[ -n "$query" ]] && query+="&"
  query+="$p"
done

response="$(curl -fsSL "${url}?${query}")"
game_count="$(echo "$response" | jq '.response.game_count // 0')"

if [[ "$game_count" -eq 0 ]]; then
  echo "No games returned. Check that your Steam profile game details are public." >&2
  echo "$response" | jq '.' >&2
  exit 1
fi

echo "$response" | jq '.' > "$OUTPUT_JSON"

echo "appid,name,playtime_forever,playtime_2weeks,rtime_last_played" > "$OUTPUT_CSV"
echo "$response" | jq -r '
  .response.games[]
  | [
      .appid,
      (.name | gsub("\""; "\"\"")),
      (.playtime_forever // 0),
      (.playtime_2weeks // 0),
      (.rtime_last_played // 0)
    ]
  | @csv
' >> "$OUTPUT_CSV"

echo "Exported ${game_count} games:"
echo "  JSON: ${OUTPUT_JSON}"
echo "  CSV:  ${OUTPUT_CSV}"
