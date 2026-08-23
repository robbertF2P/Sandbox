from __future__ import annotations

from pathlib import Path

from media_monitor.config import load_config


def test_load_watch_games_from_config(tmp_path: Path) -> None:
    config_path = tmp_path / "config.yaml"
    config_path.write_text(
        """
steam:
  watch_games:
    - app_id: 870780
      name: CONTROL Ultimate Edition
      tags: [Action, Adventure]
      note: recently purchased
    - app_id: 918060
      name: "Obstruction : VR"
      tags: [Puzzle, VR]
      note: owned
""",
        encoding="utf-8",
    )

    config = load_config(config_path)

    assert len(config.steam.watch_games) == 2
    assert config.steam.watch_games[0].app_id == 870780
    assert config.steam.watch_games[0].name == "CONTROL Ultimate Edition"
    assert config.steam.watch_games[0].tags == ["Action", "Adventure"]
    assert config.steam.watch_games[1].app_id == 918060
    assert config.steam.watch_games[1].note == "owned"
