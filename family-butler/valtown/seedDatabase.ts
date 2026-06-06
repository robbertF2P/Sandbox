// AUTO-GENERATED — do not edit. Run: node family-butler/scripts/generate-seed-val.mjs
// Source: database/schema.sql + database/seed.sql
//
// Val Town setup (2 minutes):
//   1. New val → name it "family-butler"
//   2. Paste this entire file as seedDatabase.ts
//   3. Click Run ▶  (or add HTTP trigger and open the URL once)

import sqlite from "https://esm.town/v/std/sqlite/global.ts";

const STATEMENTS: string[] = [
  "CREATE TABLE IF NOT EXISTS config (\n  key   TEXT PRIMARY KEY,\n  value TEXT NOT NULL\n)",
  "CREATE TABLE IF NOT EXISTS meals (\n  date       TEXT PRIMARY KEY,  \n  dish       TEXT NOT NULL,\n  leftovers  INTEGER DEFAULT 0,\n  cooked_by  TEXT\n)",
  "CREATE TABLE IF NOT EXISTS chore_log (\n  id      INTEGER PRIMARY KEY AUTOINCREMENT,\n  date    TEXT NOT NULL,       \n  task    TEXT NOT NULL,\n  person  TEXT NOT NULL,\n  done_at TEXT NOT NULL         \n)",
  "CREATE TABLE IF NOT EXISTS rotation (\n  task          TEXT NOT NULL,\n  day_of_week   INTEGER NOT NULL,\n  assignee      TEXT NOT NULL,\n  assist        TEXT,\n  PRIMARY KEY (task, day_of_week)\n)",
  "CREATE INDEX IF NOT EXISTS idx_chore_log_date ON chore_log (date)",
  "CREATE INDEX IF NOT EXISTS idx_rotation_dow ON rotation (day_of_week)",
  "DELETE FROM rotation",
  "DELETE FROM config",
  "INSERT INTO config (key, value) VALUES\n  ('parent_2',         'Anna'),\n  ('kid_1',            'Milan'),\n  ('kid_2',            'Eva'),\n  ('dog_name',         'Mylo'),\n  ('dog_owner',        'Milan'),\n  ('dog_walk_time',    '16:00'),\n  ('dog_walk_window',  '16:00-17:00'),\n  ('dog_walk_daily',   'true'),\n  ('cat_name',         'Pepsi'),\n  ('cat_owner',        'Eva'),\n  ('horse_name',       'Kyana'),\n  ('horse_rider',      'Anna'),\n  ('horse_boarded',    'true'),\n  ('pony_name',        'Rosa'),\n  ('pony_owner',       'Eva'),\n  ('pony_boarded',     'true'),\n  ('timezone',         'Europe/Amsterdam'),\n  ('briefing_language', 'en')",
  "INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES\n  ('dishwasher', 0, 'Eva',   NULL),\n  ('dishwasher', 1, 'Milan', NULL),\n  ('dishwasher', 2, 'Eva',   NULL),\n  ('dishwasher', 3, 'Milan', NULL),\n  ('dishwasher', 4, 'Eva',   NULL),\n  ('dishwasher', 5, 'Milan', NULL),\n  ('dishwasher', 6, 'Eva',   NULL)",
  "INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES\n  ('trash', 3, 'Milan', NULL),  \n  ('trash', 0, 'Eva',   NULL)",
  "INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES\n  ('room', 6, 'Milan', NULL),\n  ('room', 6, 'Eva',   NULL)",
  "INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES\n  ('mylo_walk', 0, 'Milan', NULL),\n  ('mylo_walk', 1, 'Milan', NULL),\n  ('mylo_walk', 2, 'Milan', NULL),\n  ('mylo_walk', 3, 'Milan', NULL),\n  ('mylo_walk', 4, 'Milan', NULL),\n  ('mylo_walk', 5, 'Milan', NULL),\n  ('mylo_walk', 6, 'Milan', NULL)",
  "INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES\n  ('pepsi_care', 0, 'Eva', NULL),\n  ('pepsi_care', 1, 'Eva', NULL),\n  ('pepsi_care', 2, 'Eva', NULL),\n  ('pepsi_care', 3, 'Eva', NULL),\n  ('pepsi_care', 4, 'Eva', NULL),\n  ('pepsi_care', 5, 'Eva', NULL),\n  ('pepsi_care', 6, 'Eva', NULL)"
];

export async function runSeed(): Promise<{ config: unknown[]; rotation: unknown[] }> {
  for (const sql of STATEMENTS) {
    await sqlite.execute(sql);
  }
  const config = await sqlite.execute("SELECT key, value FROM config ORDER BY key");
  const rotation = await sqlite.execute(
    "SELECT task, day_of_week, assignee FROM rotation ORDER BY task, day_of_week",
  );
  return { config: config.rows, rotation: rotation.rows };
}

/** Click Run in Val Town, or call via HTTP trigger (GET or POST). */
export default async function (req?: Request): Promise<Response | void> {
  const secret = Deno.env.get("SEED_SECRET");
  if (req && secret && req.headers.get("authorization") !== `Bearer ${secret}`) {
    return new Response("Unauthorized", { status: 401 });
  }

  const result = await runSeed();
  const msg = `Family butler DB seeded: ${result.config.length} config rows, ${result.rotation.length} rotation rows`;
  console.log(msg, result);

  if (req) {
    return Response.json({ ok: true, message: msg, ...result });
  }
}
