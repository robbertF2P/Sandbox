-- Family butler seed data
-- Run after schema.sql. Safe to re-run: clears and re-inserts seed rows.

DELETE FROM rotation;
DELETE FROM config;

-- ── Household ─────────────────────────────────────────────────────────────

INSERT INTO config (key, value) VALUES
  ('parent_2',         'Anna'),
  ('kid_1',            'Milan'),
  ('kid_2',            'Eva'),
  ('dog_name',         'Mylo'),
  ('dog_owner',        'Milan'),
  ('dog_walk_time',    '16:00'),
  ('dog_walk_window',  '16:00-17:00'),
  ('dog_walk_daily',   'true'),
  ('cat_name',         'Pepsi'),
  ('cat_owner',        'Eva'),
  ('horse_name',       'Kyana'),
  ('horse_rider',      'Anna'),
  ('horse_boarded',    'true'),
  ('pony_name',        'Rosa'),
  ('pony_owner',       'Eva'),
  ('pony_boarded',     'true'),
  ('timezone',         'Europe/Amsterdam'),
  ('briefing_language', 'en');

-- ── Dishwasher (alternate daily) ──────────────────────────────────────────
-- Sun=Eva, Mon=Milan, Tue=Eva, Wed=Milan, Thu=Eva, Fri=Milan, Sat=Eva

INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES
  ('dishwasher', 0, 'Eva',   NULL),
  ('dishwasher', 1, 'Milan', NULL),
  ('dishwasher', 2, 'Eva',   NULL),
  ('dishwasher', 3, 'Milan', NULL),
  ('dishwasher', 4, 'Eva',   NULL),
  ('dishwasher', 5, 'Milan', NULL),
  ('dishwasher', 6, 'Eva',   NULL);

-- ── Trash (2×/week — confirm collection day; default Wed + Sun) ───────────

INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES
  ('trash', 3, 'Milan', NULL),  -- Wednesday
  ('trash', 0, 'Eva',   NULL);  -- Sunday

-- ── Room clean (Saturday — each own room) ─────────────────────────────────

INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES
  ('room', 6, 'Milan', NULL),
  ('room', 6, 'Eva',   NULL);

-- ── Mylo walk (Milan every day ~16:00) ────────────────────────────────────

INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES
  ('mylo_walk', 0, 'Milan', NULL),
  ('mylo_walk', 1, 'Milan', NULL),
  ('mylo_walk', 2, 'Milan', NULL),
  ('mylo_walk', 3, 'Milan', NULL),
  ('mylo_walk', 4, 'Milan', NULL),
  ('mylo_walk', 5, 'Milan', NULL),
  ('mylo_walk', 6, 'Milan', NULL);

-- ── Pepsi (Eva — daily feed + litter check) ───────────────────────────────

INSERT INTO rotation (task, day_of_week, assignee, assist) VALUES
  ('pepsi_care', 0, 'Eva', NULL),
  ('pepsi_care', 1, 'Eva', NULL),
  ('pepsi_care', 2, 'Eva', NULL),
  ('pepsi_care', 3, 'Eva', NULL),
  ('pepsi_care', 4, 'Eva', NULL),
  ('pepsi_care', 5, 'Eva', NULL),
  ('pepsi_care', 6, 'Eva', NULL);
