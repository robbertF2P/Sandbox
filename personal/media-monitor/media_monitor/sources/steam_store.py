from __future__ import annotations

import requests

from media_monitor.config import SteamConfig
from media_monitor.models import GameCandidate, LibraryGame, SteamProfile

STEAM_STORE_BASE = "https://store.steampowered.com"


def _get_json(url: str, params: dict | None = None) -> dict:
    response = requests.get(url, params=params, timeout=30)
    response.raise_for_status()
    return response.json()


def load_steam_profile(config: SteamConfig) -> SteamProfile:
    if not config.api_key or not config.steam_id:
        raise ValueError("Steam API key and Steam ID are required (STEAMAPIKEY / STEAMID).")

    owned_response = _get_json(
        "https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/",
        {
            "key": config.api_key,
            "steamid": config.steam_id,
            "include_appinfo": 1,
            "include_played_free_games": 1,
            "format": "json",
        },
    )
    games = owned_response.get("response", {}).get("games", [])
    owned_app_ids = {int(game["appid"]) for game in games}
    top_games = sorted(
        (
            (
                int(game["appid"]),
                game.get("name", f"App {game['appid']}"),
                int(game.get("playtime_forever", 0)),
            )
            for game in games
        ),
        key=lambda item: item[2],
        reverse=True,
    )[:20]

    wishlist_app_ids: set[int] = set()
    try:
        wishlist = _get_json(
            f"{STEAM_STORE_BASE}/wishlist/profiles/{config.steam_id}/wishlistdata/",
            params={"p": 0},
        )
        wishlist_app_ids = {int(app_id) for app_id in wishlist.keys()}
    except requests.RequestException:
        pass

    tag_weights: dict[str, float] = {}
    min_minutes = config.min_playtime_hours_for_tag_weight * 60
    for app_id, _, playtime in top_games:
        if playtime < min_minutes:
            continue
        try:
            details = _get_json(
                f"{STEAM_STORE_BASE}/api/appdetails",
                {"appids": app_id, "cc": config.country_code, "l": "english"},
            )
            app_data = details.get(str(app_id), {}).get("data", {})
            for tag in app_data.get("genres", []):
                name = tag.get("description", "")
                if name:
                    tag_weights[name] = tag_weights.get(name, 0.0) + playtime / 60
            for tag in app_data.get("categories", []):
                name = tag.get("description", "")
                if name:
                    tag_weights[name] = tag_weights.get(name, 0.0) + playtime / 120
        except requests.RequestException:
            continue

    for tag in config.watch_tags:
        tag_weights[tag] = tag_weights.get(tag, 0.0) + 10.0

    for game in config.watch_games:
        for tag in game.tags:
            tag_weights[tag] = tag_weights.get(tag, 0.0) + 15.0

    library_games = [
        LibraryGame(
            app_id=game.app_id,
            name=game.name,
            note=game.note,
            in_library=game.app_id in owned_app_ids,
            tags=game.tags,
        )
        for game in config.watch_games
    ]

    return SteamProfile(
        owned_app_ids=owned_app_ids,
        wishlist_app_ids=wishlist_app_ids,
        top_games=top_games,
        tag_weights=tag_weights,
        library_games=[LibraryGame(**entry) for entry in library_games],
    )


def _app_price_info(app_id: int, country_code: str) -> tuple[str | None, int | None]:
    details = _get_json(
        f"{STEAM_STORE_BASE}/api/appdetails",
        {"appids": app_id, "cc": country_code, "l": "english"},
    )
    data = details.get(str(app_id), {}).get("data", {})
    price = data.get("price_overview") or {}
    if not price:
        return None, None
    final = price.get("final_formatted")
    discount = price.get("discount_percent")
    return final, int(discount) if discount is not None else None


def discover_steam_deals(
    profile: SteamProfile, config: SteamConfig
) -> list[GameCandidate]:
    candidates: list[GameCandidate] = []
    for app_id in sorted(profile.wishlist_app_ids):
        if app_id in profile.owned_app_ids:
            continue
        try:
            price, discount = _app_price_info(app_id, config.country_code)
            if discount is None or discount < config.min_discount_percent:
                continue
            details = _get_json(
                f"{STEAM_STORE_BASE}/api/appdetails",
                {"appids": app_id, "cc": config.country_code, "l": "english"},
            )
            data = details.get(str(app_id), {}).get("data", {})
            name = data.get("name", f"App {app_id}")
            tags = [g.get("description", "") for g in data.get("genres", [])]
            candidates.append(
                GameCandidate(
                    app_id=app_id,
                    name=name,
                    source="wishlist_sale",
                    reason=f"Wishlist item on sale ({discount}% off)",
                    url=f"{STEAM_STORE_BASE}/app/{app_id}",
                    price=price,
                    discount_percent=discount,
                    tags=[t for t in tags if t],
                )
            )
        except requests.RequestException:
            continue
    return candidates


def discover_steam_recommendations(
    profile: SteamProfile, config: SteamConfig
) -> list[GameCandidate]:
    candidates: list[GameCandidate] = []
    seen: set[int] = set(profile.owned_app_ids) | set(profile.wishlist_app_ids)

    top_tags = sorted(profile.tag_weights.items(), key=lambda item: item[1], reverse=True)[:5]
    for tag, weight in top_tags:
        try:
            search = _get_json(
                f"{STEAM_STORE_BASE}/api/storesearch/",
                {
                    "term": tag,
                    "cc": config.country_code,
                    "l": "english",
                    "category1": 998,
                },
            )
            for item in search.get("items", [])[:5]:
                app_id = int(item.get("id", 0))
                if not app_id or app_id in seen:
                    continue
                seen.add(app_id)
                candidates.append(
                    GameCandidate(
                        app_id=app_id,
                        name=item.get("name", f"App {app_id}"),
                        source="tag_recommendation",
                        reason=f"Matches your taste tag '{tag}' (weight {weight:.0f})",
                        url=f"{STEAM_STORE_BASE}/app/{app_id}",
                        price=item.get("price", {}).get("final") if item.get("price") else None,
                        tags=[tag],
                    )
                )
        except requests.RequestException:
            continue
    return candidates
