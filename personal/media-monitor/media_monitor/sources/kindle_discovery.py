from __future__ import annotations

import re
from urllib.parse import quote_plus

import requests
from bs4 import BeautifulSoup

from media_monitor.config import KindleConfig
from media_monitor.models import BookCandidate, KindleProfile

USER_AGENT = (
    "Mozilla/5.0 (compatible; MediaMonitor/1.0; +https://github.com/robbertF2P/Sandbox)"
)


def _amazon_kindle_search_url(domain: str, query: str) -> str:
    encoded = quote_plus(query)
    return f"https://{domain}/s?k={encoded}&i=digital-text"


def _parse_amazon_results(html: str, domain: str) -> list[BookCandidate]:
    soup = BeautifulSoup(html, "html.parser")
    results: list[BookCandidate] = []
    for item in soup.select('[data-component-type="s-search-result"]')[:10]:
        title_el = item.select_one("h2 a span")
        if not title_el:
            continue
        title = title_el.get_text(strip=True)
        link = item.select_one("h2 a")
        href = link.get("href", "") if link else ""
        url = f"https://{domain}{href}" if href.startswith("/") else href
        asin = None
        match = re.search(r"/dp/([A-Z0-9]{10})", href or "")
        if match:
            asin = match.group(1)
        author_el = item.select_one(".a-row.a-size-base.a-color-secondary")
        author = author_el.get_text(strip=True) if author_el else "Unknown"
        price_el = item.select_one(".a-price .a-offscreen")
        price = price_el.get_text(strip=True) if price_el else None
        results.append(
            BookCandidate(
                title=title,
                author=author,
                asin=asin,
                source="amazon_search",
                reason="",
                url=url or None,
                price=price,
            )
        )
    return results


def _open_library_author_works(author: str, limit: int) -> list[BookCandidate]:
    search_url = "https://openlibrary.org/search.json"
    try:
        response = requests.get(
            search_url,
            params={"author": author, "limit": limit, "sort": "new"},
            timeout=20,
            headers={"User-Agent": USER_AGENT},
        )
        response.raise_for_status()
    except requests.RequestException:
        return []
    docs = response.json().get("docs", [])
    results: list[BookCandidate] = []
    for doc in docs:
        title = doc.get("title")
        if not title:
            continue
        first_publish = doc.get("first_publish_year")
        key = doc.get("key", "")
        results.append(
            BookCandidate(
                title=title,
                author=author,
                asin=None,
                source="open_library",
                reason=f"New/recent work by watched author ({author})",
                url=f"https://openlibrary.org{key}" if key else None,
                published_date=str(first_publish) if first_publish else None,
            )
        )
    return results


def discover_kindle_books(
    profile: KindleProfile, config: KindleConfig
) -> list[BookCandidate]:
    candidates: list[BookCandidate] = []
    seen_titles: set[str] = set()

    ranked_authors = sorted(profile.authors.items(), key=lambda item: item[1], reverse=True)
    for author, weight in ranked_authors[:15]:
        for book in _open_library_author_works(author, config.max_results_per_author):
            key = book.title.lower()
            if key in seen_titles:
                continue
            seen_titles.add(key)
            book.reason = f"Author match: {author} (weight {weight:.1f})"
            candidates.append(book)

    for keyword in config.watch_keywords:
        query = f"{keyword} kindle ebook"
        url = _amazon_kindle_search_url(config.amazon_domain, query)
        try:
            response = requests.get(
                url,
                timeout=20,
                headers={"User-Agent": USER_AGENT},
            )
            response.raise_for_status()
            for book in _parse_amazon_results(response.text, config.amazon_domain):
                key = book.title.lower()
                if key in seen_titles:
                    continue
                seen_titles.add(key)
                book.reason = f"Keyword match: {keyword}"
                candidates.append(book)
        except requests.RequestException:
            continue

    for series_name, hours in sorted(profile.series.items(), key=lambda i: i[1], reverse=True)[
        :10
    ]:
        query = f"{series_name} kindle book"
        url = _amazon_kindle_search_url(config.amazon_domain, query)
        try:
            response = requests.get(
                url,
                timeout=20,
                headers={"User-Agent": USER_AGENT},
            )
            response.raise_for_status()
            for book in _parse_amazon_results(response.text, config.amazon_domain)[:3]:
                key = book.title.lower()
                if key in seen_titles:
                    continue
                seen_titles.add(key)
                book.reason = f"Series continuation watch: {series_name} ({hours:.1f}h read)"
                candidates.append(book)
        except requests.RequestException:
            continue

    filtered: list[BookCandidate] = []
    for book in candidates:
        if book.asin and book.asin in profile.owned_asins:
            continue
        filtered.append(book)
    return filtered
