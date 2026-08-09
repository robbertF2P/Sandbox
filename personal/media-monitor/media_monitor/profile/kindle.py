from __future__ import annotations

import csv
import glob
import json
import re
from collections import Counter, defaultdict
from pathlib import Path

from media_monitor.config import KindleConfig
from media_monitor.models import KindleProfile

AUTHOR_TAG_PREFIX = "By "
SERIES_PATTERNS = [
    re.compile(r"\(([^)]+?)\s+Book\s+\d+", re.I),
    re.compile(r"\(([^)]+?)\s+Series\s+Book\s+\d+", re.I),
    re.compile(r"\(([^)]+?)\s+series\)", re.I),
    re.compile(r"Book\s+\d+\s+of\s+([^)]+)\)", re.I),
]
KNOWN_SERIES_AUTHORS = {
    "First Colony": "Ken Lozito",
    "The Expanse": "James S. A. Corey",
    "Revelation Space": "Alastair Reynolds",
    "Time's Shadow": "Peter F. Hamilton",
    "Foundation": "Isaac Asimov",
    "Old Man's War": "John Scalzi",
    "Expeditionary Force": "Craig Alanson",
    "The Final Architecture": "Adrian Tchaikovsky",
    "Mote Series": "Larry Niven",
    "CoDominium": "Jerry Pournelle",
    "Commonwealth Saga": "Peter F. Hamilton",
    "Spatterjay": "Neal Asher",
    "Rise of the Jain": "Neal Asher",
    "Salvation Sequence": "Peter F. Hamilton",
    "Three-Body Problem": "Liu Cixin",
}


def _read_csv(path: str) -> list[dict[str, str]]:
    with open(path, newline="", encoding="utf-8-sig", errors="replace") as f:
        return list(csv.DictReader(f))


def _extract_series(title: str) -> str | None:
    for pattern in SERIES_PATTERNS:
        match = pattern.search(title)
        if match:
            return match.group(1).strip()
    if "First Colony" in title:
        return "First Colony"
    if "Expanse" in title and "Prime Original" not in title:
        return "The Expanse"
    if "Prime Original" in title:
        return "The Expanse"
    if "Expeditionary Force" in title:
        return "Expeditionary Force"
    if "Old Man's War" in title or "Old Mans War" in title:
        return "Old Man's War"
    if "Revelation Space" in title:
        return "Revelation Space"
    if "Time's Shadow" in title or "Time's Shadow" in title:
        return "Time's Shadow"
    if "Final Architecture" in title:
        return "The Final Architecture"
    if "Mote Series" in title or "Mote in God" in title:
        return "Mote Series"
    if "Foundation" in title and "Robot" not in title:
        return "Foundation"
    if "CoDominium" in title:
        return "CoDominium"
    if "Commonwealth Saga" in title or "Pandora's Star" in title:
        return "Commonwealth Saga"
    if "Spatterjay" in title:
        return "Spatterjay"
    if "Rise of the Jain" in title:
        return "Rise of the Jain"
    if "Salvation Sequence" in title:
        return "Salvation Sequence"
    return None


def load_kindle_profile(export_dir: str | Path, config: KindleConfig) -> KindleProfile:
    base = Path(export_dir)
    owned_asins: set[str] = set()
    authors: Counter[str] = Counter()
    series: Counter[str] = Counter()
    genres: list[str] = []
    book_hours: dict[str, tuple[str, float]] = {}

    for json_path in glob.glob(str(base / "Digital.Content.Ownership" / "*.json")):
        with open(json_path, encoding="utf-8") as f:
            data = json.load(f)
        asin = data.get("resource", {}).get("ASIN")
        if asin:
            owned_asins.add(asin)

    tag_paths = glob.glob(
        str(base / "**" / "Kindle.UnifiedLibraryIndex.CustomerTags*.csv"), recursive=True
    )
    for tag_path in tag_paths:
        for row in _read_csv(tag_path):
            tag = row.get("Tag Name", "")
            scope = row.get("Tag Source Group", "")
            if tag.startswith(AUTHOR_TAG_PREFIX):
                authors[tag[len(AUTHOR_TAG_PREFIX) :].strip()] += 1.0
            elif scope == "genre":
                genres.append(tag)

    session_paths = glob.glob(
        str(base / "**" / "Kindle.reading-insights-sessions_with_adjustments.csv"),
        recursive=True,
    )
    for session_path in session_paths:
        for row in _read_csv(session_path):
            asin = row.get("ASIN", "")
            title = row.get("product_name", "Unknown")
            if title == "Not Available":
                continue
            hours = int(row.get("total_reading_milliseconds", 0) or 0) / 3_600_000
            if asin:
                prev = book_hours.get(asin, (title, 0.0))
                book_hours[asin] = (title, prev[1] + hours)
            extracted = _extract_series(title)
            if extracted:
                series[extracted] += hours

    top_books = sorted(
        ((asin, title, hours) for asin, (title, hours) in book_hours.items()),
        key=lambda item: item[2],
        reverse=True,
    )

    for _, title, hours in top_books:
        if hours < config.min_reading_hours_for_author_weight:
            continue
        extracted = _extract_series(title)
        if extracted:
            series[extracted] += hours * 0.1

    for author in config.watch_authors:
        authors[author] += 5.0
    for name in config.watch_series:
        series[name] += 5.0

    for series_name, hours in series.items():
        author = KNOWN_SERIES_AUTHORS.get(series_name)
        if author and hours >= config.min_reading_hours_for_author_weight:
            authors[author] += hours

    completed_paths = glob.glob(
        str(base / "**" / "Kindle.UserUniqueTitlesCompleted.csv"), recursive=True
    )
    for completed_path in completed_paths:
        for row in _read_csv(completed_path):
            title = row.get("product_name") or list(row.values())[-1]
            if title and title != "Not Available":
                extracted = _extract_series(title)
                if extracted:
                    author = KNOWN_SERIES_AUTHORS.get(extracted)
                    if author:
                        authors[author] += 2.0

    return KindleProfile(
        owned_asins=owned_asins,
        authors=dict(authors),
        series=dict(series),
        genres=sorted(set(genres)),
        top_books=top_books[:25],
    )
