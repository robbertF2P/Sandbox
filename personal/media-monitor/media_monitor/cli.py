from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from media_monitor.config import load_config
from media_monitor.profile.kindle import load_kindle_profile
from media_monitor.report import write_report
from media_monitor.sources.kindle_discovery import discover_kindle_books
from media_monitor.sources.steam_store import (
    discover_steam_deals,
    discover_steam_recommendations,
    load_steam_profile,
)
from media_monitor.state import MonitorState


def _default_config_path() -> Path:
    local = Path("personal/media-monitor/config.yaml")
    if local.exists():
        return local
    return Path("personal/media-monitor/config.example.yaml")


def cmd_profile_kindle(config_path: Path) -> int:
    config = load_config(config_path)
    profile = load_kindle_profile(config.kindle.export_dir, config.kindle)
    payload = {
        "owned_books": len(profile.owned_asins),
        "authors": profile.authors,
        "series": profile.series,
        "genres": profile.genres[:20],
        "top_books": [
            {"asin": asin, "title": title, "hours": round(hours, 1)}
            for asin, title, hours in profile.top_books[:15]
        ],
    }
    print(json.dumps(payload, indent=2))
    return 0


def cmd_profile_steam(config_path: Path) -> int:
    config = load_config(config_path)
    profile = load_steam_profile(config.steam)
    payload = {
        "owned_games": len(profile.owned_app_ids),
        "wishlist_games": len(profile.wishlist_app_ids),
        "tag_weights": profile.tag_weights,
        "top_games": [
            {"app_id": app_id, "name": name, "hours": round(minutes / 60, 1)}
            for app_id, name, minutes in profile.top_games[:15]
        ],
    }
    print(json.dumps(payload, indent=2))
    return 0


def cmd_run(config_path: Path, steam: bool, kindle: bool, mark_seen: bool) -> int:
    config = load_config(config_path)
    state = MonitorState(config.state_file)
    books = []
    games = []

    if kindle:
        export_path = Path(config.kindle.export_dir)
        if not export_path.exists():
            print(f"Kindle export not found: {export_path}", file=sys.stderr)
            return 1
        profile = load_kindle_profile(export_path, config.kindle)
        discovered = discover_kindle_books(profile, config.kindle)
        for book in discovered:
            key = book.asin or book.title.lower()
            if state.seen_book(key):
                continue
            books.append(book)
            if mark_seen:
                state.mark_book(key)

    if steam:
        try:
            profile = load_steam_profile(config.steam)
        except ValueError as exc:
            print(str(exc), file=sys.stderr)
            return 1
        discovered = discover_steam_deals(profile, config.steam)
        discovered.extend(discover_steam_recommendations(profile, config.steam))
        for game in discovered:
            key = str(game.app_id)
            if state.seen_game(key):
                continue
            games.append(game)
            if mark_seen:
                state.mark_game(key)

    json_path, md_path = write_report(config.output_dir, books, games)
    if mark_seen:
        state.save()
    print(f"Report written:\n  {json_path}\n  {md_path}")
    print(f"New suggestions: {len(books)} books, {len(games)} games")
    return 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Monitor Steam games and Kindle books.")
    parser.add_argument(
        "--config",
        type=Path,
        default=_default_config_path(),
        help="Path to config YAML",
    )
    sub = parser.add_subparsers(dest="command", required=True)

    run = sub.add_parser("run", help="Run monitors and write a report")
    run.add_argument("--steam", action="store_true", help="Include Steam monitor")
    run.add_argument("--kindle", action="store_true", help="Include Kindle monitor")
    run.add_argument(
        "--all",
        action="store_true",
        help="Run both Steam and Kindle monitors",
    )
    run.add_argument(
        "--mark-seen",
        action="store_true",
        help="Persist seen suggestions so they are not reported again",
    )

    sub.add_parser("profile-kindle", help="Print Kindle taste profile from export")
    sub.add_parser("profile-steam", help="Print Steam taste profile from API")
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    if args.command == "profile-kindle":
        return cmd_profile_kindle(args.config)
    if args.command == "profile-steam":
        return cmd_profile_steam(args.config)
    if args.command == "run":
        steam = args.steam or args.all
        kindle = args.kindle or args.all
        if not steam and not kindle:
            steam = kindle = True
        return cmd_run(args.config, steam=steam, kindle=kindle, mark_seen=args.mark_seen)
    parser.error(f"Unknown command: {args.command}")
    return 2


if __name__ == "__main__":
    raise SystemExit(main())
