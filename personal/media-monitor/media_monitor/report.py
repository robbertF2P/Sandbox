from __future__ import annotations

import json
from dataclasses import asdict
from datetime import datetime, timezone
from pathlib import Path

from media_monitor.models import BookCandidate, GameCandidate, LibraryGame


def _write_markdown_report(
    path: Path,
    books: list[BookCandidate],
    games: list[GameCandidate],
    library_games: list[LibraryGame] | None = None,
) -> None:
    lines = [
        f"# Media Monitor Report",
        "",
        f"Generated: {datetime.now(timezone.utc).isoformat()}",
        "",
    ]

    if library_games:
        lines.extend(["## Your Steam library highlights", ""])
        for game in library_games:
            status = "in library" if game.in_library else "not detected in Steam API yet"
            note = f" ({game.note})" if game.note else ""
            lines.append(f"### {game.name}{note}")
            lines.append(f"- **Status:** {status}")
            if game.tags:
                lines.append(f"- **Tags:** {', '.join(game.tags)}")
            lines.append(f"- **Link:** https://store.steampowered.com/app/{game.app_id}")
            lines.append("")

    lines.extend(["## Kindle books you might like", ""])
    if books:
        for book in books:
            lines.append(f"### {book.title}")
            lines.append(f"- **Author:** {book.author}")
            lines.append(f"- **Reason:** {book.reason}")
            if book.price:
                lines.append(f"- **Price:** {book.price}")
            if book.published_date:
                lines.append(f"- **Published:** {book.published_date}")
            if book.url:
                lines.append(f"- **Link:** {book.url}")
            lines.append("")
    else:
        lines.append("_No new book suggestions this run._")
        lines.append("")

    lines.extend(["## Steam games", ""])
    if games:
        for game in games:
            lines.append(f"### {game.name}")
            lines.append(f"- **Reason:** {game.reason}")
            if game.discount_percent is not None:
                lines.append(f"- **Discount:** {game.discount_percent}%")
            if game.price:
                lines.append(f"- **Price:** {game.price}")
            if game.tags:
                lines.append(f"- **Tags:** {', '.join(game.tags)}")
            lines.append(f"- **Link:** {game.url}")
            lines.append("")
    else:
        lines.append("_No new game suggestions this run._")
        lines.append("")

    path.write_text("\n".join(lines), encoding="utf-8")


def write_report(
    output_dir: str | Path,
    books: list[BookCandidate],
    games: list[GameCandidate],
    library_games: list[LibraryGame] | None = None,
) -> tuple[Path, Path]:
    out = Path(output_dir)
    out.mkdir(parents=True, exist_ok=True)
    stamp = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S")
    json_path = out / f"report-{stamp}.json"
    md_path = out / f"report-{stamp}.md"

    payload = {
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "library_games": [asdict(game) for game in library_games or []],
        "books": [asdict(book) for book in books],
        "games": [asdict(game) for game in games],
    }
    json_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    _write_markdown_report(md_path, books, games, library_games)
    latest_json = out / "latest.json"
    latest_md = out / "latest.md"
    latest_json.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    _write_markdown_report(latest_md, books, games, library_games)
    return json_path, md_path
