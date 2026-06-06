#!/usr/bin/env node
/**
 * Regenerates valtown/seedDatabase.ts from database/schema.sql + database/seed.sql
 * Run: node family-butler/scripts/generate-seed-val.mjs
 */
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");

function parseSql(sql) {
  const withoutComments = sql.replace(/--[^\n]*/g, "");
  return withoutComments
    .split(";")
    .map((s) => s.trim())
    .filter((s) => s.length > 0);
}

const schema = readFileSync(join(root, "database/schema.sql"), "utf8");
const seed = readFileSync(join(root, "database/seed.sql"), "utf8");
const statements = [...parseSql(schema), ...parseSql(seed)];

const out = `// AUTO-GENERATED — do not edit. Run: node family-butler/scripts/generate-seed-val.mjs
// Source: database/schema.sql + database/seed.sql
//
// Val Town setup (2 minutes):
//   1. New val → name it "family-butler"
//   2. Paste this entire file as seedDatabase.ts
//   3. Click Run ▶  (or add HTTP trigger and open the URL once)

import sqlite from "https://esm.town/v/std/sqlite/global.ts";

const STATEMENTS: string[] = ${JSON.stringify(statements, null, 2)};

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
  if (req && secret && req.headers.get("authorization") !== \`Bearer \${secret}\`) {
    return new Response("Unauthorized", { status: 401 });
  }

  const result = await runSeed();
  const msg = \`Family butler DB seeded: \${result.config.length} config rows, \${result.rotation.length} rotation rows\`;
  console.log(msg, result);

  if (req) {
    return Response.json({ ok: true, message: msg, ...result });
  }
}
`;

writeFileSync(join(root, "valtown/seedDatabase.ts"), out);
console.log(`Wrote seedDatabase.ts (${statements.length} statements)`);
