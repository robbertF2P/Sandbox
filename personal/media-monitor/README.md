# Media Monitor

Personal tool to monitor **Steam games** and **Kindle books** you might like, based on your library, playtime, and reading history.

## What it does

### Steam
- Loads your owned games and wishlist via the Steam Web API
- Surfaces **wishlist sales** above a discount threshold
- Recommends games matching tags/genres from your most-played titles

### Kindle
- Builds a taste profile from your **Amazon Kindle data export**
  - Owned ASINs, author tags, genres, series, and reading time
- Suggests books via:
  - **Open Library** (new works by authors you read)
  - **Amazon.nl Kindle search** (series continuations and keyword matches)
- Skips books you already own

## Setup

```bash
cd personal/media-monitor
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cp config.example.yaml config.yaml
```

Edit `config.yaml`:
- `kindle.export_dir` — path to extracted Kindle export (default `./kindle-data`)
- Steam secrets via environment: `STEAMAPIKEY`, `STEAMID`

## Usage

```bash
# Show Kindle taste profile from export
./run-monitor.sh profile-kindle

# Show Steam taste profile (needs API key)
./run-monitor.sh profile-steam

# Run both monitors and write report to artifacts
./run-monitor.sh run --all

# Kindle only
./run-monitor.sh run --kindle

# Steam only
./run-monitor.sh run --steam

# Mark suggestions as seen (won't repeat next run)
./run-monitor.sh run --all --mark-seen
```

Reports are written to `/opt/cursor/artifacts/media-monitor/` as `latest.md` and `latest.json`.

## Kindle export

Request your data from Amazon: **Account → Privacy → Download your data** → select Kindle content.

Extract the zip and point `kindle.export_dir` at the folder containing `Digital.Content.Ownership/`, reading insights CSVs, etc.

## Steam library export

Optional standalone export (also used for reference):

```bash
./scripts/export-steam-library.sh
```

Requires `Steamapikey` and `Steamid` Cursor environment secrets.

## Cursor environment

Add to `.cursor/environment.json`:

```json
{
  "install": "cd personal/media-monitor && python3 -m venv .venv && .venv/bin/pip install -r requirements.txt",
  "start": "./personal/media-monitor/run-monitor.sh run --all --mark-seen"
}
```

## Tests

```bash
cd personal/media-monitor
.venv/bin/python -m pytest tests/ -q
```
