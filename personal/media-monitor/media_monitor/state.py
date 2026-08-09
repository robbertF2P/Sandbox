from __future__ import annotations

import json
from pathlib import Path


class MonitorState:
    def __init__(self, path: str | Path) -> None:
        self.path = Path(path)
        self._data: dict = {"reported_books": [], "reported_games": []}
        if self.path.exists():
            self._data = json.loads(self.path.read_text(encoding="utf-8"))

    def seen_book(self, key: str) -> bool:
        return key in self._data["reported_books"]

    def seen_game(self, key: str) -> bool:
        return key in self._data["reported_games"]

    def mark_book(self, key: str) -> None:
        if key not in self._data["reported_books"]:
            self._data["reported_books"].append(key)

    def mark_game(self, key: str) -> None:
        if key not in self._data["reported_games"]:
            self._data["reported_games"].append(key)

    def save(self) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        self.path.write_text(json.dumps(self._data, indent=2), encoding="utf-8")
