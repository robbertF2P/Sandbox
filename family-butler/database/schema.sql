-- Family butler SQLite schema (Val Town: std/sqlite/global.ts)

CREATE TABLE IF NOT EXISTS config (
  key   TEXT PRIMARY KEY,
  value TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS meals (
  date       TEXT PRIMARY KEY,  -- YYYY-MM-DD
  dish       TEXT NOT NULL,
  leftovers  INTEGER DEFAULT 0,
  cooked_by  TEXT
);

CREATE TABLE IF NOT EXISTS chore_log (
  id      INTEGER PRIMARY KEY AUTOINCREMENT,
  date    TEXT NOT NULL,       -- YYYY-MM-DD
  task    TEXT NOT NULL,
  person  TEXT NOT NULL,
  done_at TEXT NOT NULL         -- ISO timestamp
);

-- day_of_week: 0 = Sunday … 6 = Saturday
CREATE TABLE IF NOT EXISTS rotation (
  task          TEXT NOT NULL,
  day_of_week   INTEGER NOT NULL,
  assignee      TEXT NOT NULL,
  assist        TEXT,
  PRIMARY KEY (task, day_of_week)
);

CREATE INDEX IF NOT EXISTS idx_chore_log_date ON chore_log (date);
CREATE INDEX IF NOT EXISTS idx_rotation_dow ON rotation (day_of_week);
