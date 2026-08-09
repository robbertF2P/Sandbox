from __future__ import annotations

from dataclasses import dataclass, field


@dataclass
class BookCandidate:
    title: str
    author: str
    asin: str | None
    source: str
    reason: str
    url: str | None = None
    price: str | None = None
    published_date: str | None = None


@dataclass
class GameCandidate:
    app_id: int
    name: str
    source: str
    reason: str
    url: str
    price: str | None = None
    discount_percent: int | None = None
    tags: list[str] = field(default_factory=list)


@dataclass
class KindleProfile:
    owned_asins: set[str]
    authors: dict[str, float]
    series: dict[str, float]
    genres: list[str]
    top_books: list[tuple[str, str, float]]


@dataclass
class SteamProfile:
    owned_app_ids: set[int]
    wishlist_app_ids: set[int]
    top_games: list[tuple[int, str, int]]
    tag_weights: dict[str, float]
