// Val Town — run once to init DB (HTTP trigger).
// Paste schema + seed SQL from family-butler/sql/ into your val, or use this file as-is.
// Optional env: SEED_SECRET — require Authorization: Bearer <secret>

import sqlite from "https://esm.town/v/std/sqlite/global.ts";

const STATEMENTS = [
  `CREATE TABLE IF NOT EXISTS config (
  key TEXT PRIMARY KEY, value TEXT NOT NULL)`,

  `CREATE TABLE IF NOT EXISTS meals (
  date TEXT PRIMARY KEY, dish TEXT NOT NULL,
  leftovers INTEGER DEFAULT 0, cooked_by TEXT)`,

  `CREATE TABLE IF NOT EXISTS chore_log (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  date TEXT NOT NULL, task TEXT NOT NULL,
  person TEXT NOT NULL, done_at TEXT NOT NULL)`,

  `CREATE TABLE IF NOT EXISTS rotation (
  task TEXT NOT NULL, day_of_week INTEGER NOT NULL,
  assignee TEXT NOT NULL, assist TEXT,
  PRIMARY KEY (task, day_of_week))`,

  `DELETE FROM rotation`,
  `DELETE FROM config`,

  `INSERT INTO config (key, value) VALUES
  ('parent_2', 'Anna'),
  ('kid_1', 'Milan'),
  ('kid_2', 'Eva'),
  ('dog_name', 'Mylo'),
  ('dog_owner', 'Milan'),
  ('dog_walk_time', '16:00'),
  ('dog_walk_window', '16:00-17:00'),
  ('dog_walk_daily', 'true'),
  ('cat_name', 'Pepsi'),
  ('cat_owner', 'Eva'),
  ('horse_name', 'Kyana'),
  ('horse_rider', 'Anna'),
  ('horse_boarded', 'true'),
  ('pony_name', 'Rosa'),
  ('pony_owner', 'Eva'),
  ('pony_boarded', 'true'),
  ('timezone', 'Europe/Amsterdam'),
  ('briefing_language', 'en')`,

  // dishwasher — alternate daily (0=Sun)
  `INSERT INTO rotation (task, day_of_week, assignee) VALUES
  ('dishwasher', 0, 'Eva'), ('dishwasher', 1, 'Milan'),
  ('dishwasher', 2, 'Eva'), ('dishwasher', 3, 'Milan'),
  ('dishwasher', 4, 'Eva'), ('dishwasher', 5, 'Milan'),
  ('dishwasher', 6, 'Eva')`,

  // trash — Wed + Sun (adjust when collection day confirmed)
  `INSERT INTO rotation (task, day_of_week, assignee) VALUES
  ('trash', 3, 'Milan'), ('trash', 0, 'Eva')`,

  // room — Saturday, each own room
  `INSERT INTO rotation (task, day_of_week, assignee) VALUES
  ('room', 6, 'Milan'), ('room', 6, 'Eva')`,

  // mylo walk — Milan every day
  `INSERT INTO rotation (task, day_of_week, assignee) VALUES
  ('mylo_walk', 0, 'Milan'), ('mylo_walk', 1, 'Milan'),
  ('mylo_walk', 2, 'Milan'), ('mylo_walk', 3, 'Milan'),
  ('mylo_walk', 4, 'Milan'), ('mylo_walk', 5, 'Milan'),
  ('mylo_walk', 6, 'Milan')`,

  // pepsi — Eva daily
  `INSERT INTO rotation (task, day_of_week, assignee) VALUES
  ('pepsi_care', 0, 'Eva'), ('pepsi_care', 1, 'Eva'),
  ('pepsi_care', 2, 'Eva'), ('pepsi_care', 3, 'Eva'),
  ('pepsi_care', 4, 'Eva'), ('pepsi_care', 5, 'Eva'),
  ('pepsi_care', 6, 'Eva')`,
];

export default async function (req: Request): Promise<Response> {
  const secret = Deno.env.get("SEED_SECRET");
  if (secret && req.headers.get("authorization") !== `Bearer ${secret}`) {
    return new Response("Unauthorized", { status: 401 });
  }

  for (const sql of STATEMENTS) {
    await sqlite.execute(sql);
  }

  const config = await sqlite.execute("SELECT key, value FROM config ORDER BY key");
  return Response.json({ ok: true, config: config.rows });
}
