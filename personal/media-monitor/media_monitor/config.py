from __future__ import annotations

import os
from dataclasses import dataclass, field
from pathlib import Path

import yaml


def _expand_env(value: str) -> str:
    if value.startswith("${") and value.endswith("}"):
        return os.environ.get(value[2:-1], "")
    return value


@dataclass
class SteamConfig:
    api_key: str = ""
    steam_id: str = ""
    country_code: str = "nl"
    watch_tags: list[str] = field(default_factory=list)
    min_discount_percent: int = 25
    min_playtime_hours_for_tag_weight: int = 5


@dataclass
class KindleConfig:
    export_dir: str = "./kindle-data"
    marketplace: str = "www.amazon.nl"
    amazon_domain: str = "www.amazon.nl"
    watch_authors: list[str] = field(default_factory=list)
    watch_series: list[str] = field(default_factory=list)
    watch_keywords: list[str] = field(default_factory=list)
    min_reading_hours_for_author_weight: float = 3.0
    max_results_per_author: int = 5


@dataclass
class MonitorConfig:
    output_dir: str = "/opt/cursor/artifacts/media-monitor"
    state_file: str = "/opt/cursor/artifacts/media-monitor/state.json"
    steam: SteamConfig = field(default_factory=SteamConfig)
    kindle: KindleConfig = field(default_factory=KindleConfig)


def load_config(path: str | Path) -> MonitorConfig:
    with open(path, encoding="utf-8") as f:
        raw = yaml.safe_load(f) or {}

    steam_raw = raw.get("steam", {})
    kindle_raw = raw.get("kindle", {})

    steam = SteamConfig(
        api_key=_expand_env(str(steam_raw.get("api_key", ""))),
        steam_id=_expand_env(str(steam_raw.get("steam_id", ""))),
        country_code=str(steam_raw.get("country_code", "nl")),
        watch_tags=list(steam_raw.get("watch_tags", [])),
        min_discount_percent=int(steam_raw.get("min_discount_percent", 25)),
        min_playtime_hours_for_tag_weight=int(
            steam_raw.get("min_playtime_hours_for_tag_weight", 5)
        ),
    )
    kindle = KindleConfig(
        export_dir=str(kindle_raw.get("export_dir", "./kindle-data")),
        marketplace=str(kindle_raw.get("marketplace", "www.amazon.nl")),
        amazon_domain=str(kindle_raw.get("amazon_domain", "www.amazon.nl")),
        watch_authors=list(kindle_raw.get("watch_authors", [])),
        watch_series=list(kindle_raw.get("watch_series", [])),
        watch_keywords=list(kindle_raw.get("watch_keywords", [])),
        min_reading_hours_for_author_weight=float(
            kindle_raw.get("min_reading_hours_for_author_weight", 3.0)
        ),
        max_results_per_author=int(kindle_raw.get("max_results_per_author", 5)),
    )
    return MonitorConfig(
        output_dir=str(raw.get("output_dir", "/opt/cursor/artifacts/media-monitor")),
        state_file=str(raw.get("state_file", "/opt/cursor/artifacts/media-monitor/state.json")),
        steam=steam,
        kindle=kindle,
    )
