from __future__ import annotations

import json
from pathlib import Path

from media_monitor.profile.kindle import _extract_series, load_kindle_profile
from media_monitor.config import KindleConfig


def test_extract_series_patterns() -> None:
    assert _extract_series("Sanctuary (First Colony Book 4)") == "First Colony"
    assert _extract_series("Abaddon's Gate: Book 3 of the Expanse") == "The Expanse"
    assert _extract_series("Dark Agent (Time's Shadow Book 2)") == "Time's Shadow"


def test_load_kindle_profile_from_export() -> None:
    export_dir = Path(__file__).resolve().parents[3] / "kindle-data"
    if not export_dir.exists():
        return
    profile = load_kindle_profile(export_dir, KindleConfig())
    assert len(profile.owned_asins) > 100
    assert len(profile.top_books) > 0
    assert any("First Colony" in name for _, name, _ in profile.top_books)


def test_kindle_profile_authors_from_tags(tmp_path: Path) -> None:
    tag_dir = tmp_path / "Kindle.UnifiedLibraryIndex" / "datasets" / "tags"
    tag_dir.mkdir(parents=True)
    tag_file = tag_dir / "Kindle.UnifiedLibraryIndex.CustomerTags.2.1.csv"
    tag_file.write_text(
        '"Tag Name","Tag Source Group"\n"By Alastair Reynolds","author"\n',
        encoding="utf-8",
    )
    ownership = tmp_path / "Digital.Content.Ownership"
    ownership.mkdir()
    (ownership / "Digital.Content.Ownership.1.json").write_text(
        json.dumps(
            {
                "resource": {"ASIN": "B123", "Product Name": "Test Book"},
                "rights": [{"rightStatus": "Active"}],
            }
        ),
        encoding="utf-8",
    )
    profile = load_kindle_profile(tmp_path, KindleConfig())
    assert "Alastair Reynolds" in profile.authors
    assert "B123" in profile.owned_asins
