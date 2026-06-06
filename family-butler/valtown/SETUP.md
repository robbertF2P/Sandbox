# Val Town — family butler setup

## Seed the database (one click)

1. Go to [val.town](https://www.val.town) → **New val** → name it `family-butler`
2. Create a file **`seedDatabase.ts`**
3. Copy the full contents of [`seedDatabase.ts`](./seedDatabase.ts) from this repo into that file
4. Click **Run ▶**

Done. Open the **SQLite** tab in the val sidebar to verify `config` and `rotation` tables.

Safe to re-run — seed clears and re-inserts config + rotation (meals and chore_log are kept).

### Optional: seed via URL

1. On `seedDatabase.ts`, add trigger **HTTP** (GET)
2. Open the val URL once in your browser → `{ "ok": true, ... }`
3. Optional env `SEED_SECRET` + header `Authorization: Bearer <secret>` if you want to lock it down

---

## After seeding

Add other files to the same val (same SQLite via `global.ts`):

| File | Trigger | Purpose |
|------|---------|---------|
| `morningBriefing.ts` | CRON 07:00 | Slack `#daily` brief |
| `myloReminder.ts` | CRON 15:45 | Ping Milan |
| `slackWebhook.ts` | HTTP | Handle `done mylo`, `dinner: …` |

See [`morningBriefing.example.ts`](./morningBriefing.example.ts) for a starter cron.

---

## Edit household data later

1. Edit `database/seed.sql` in this repo
2. Regenerate: `node family-butler/scripts/generate-seed-val.mjs`
3. Re-paste `seedDatabase.ts` into Val Town → **Run** again

Or use Val Town MCP in Cursor to push the updated file.

---

## Env vars (add in val sidebar as you build)

| Variable | When needed |
|----------|-------------|
| `SLACK_WEBHOOK_DAILY` | Morning brief |
| `SLACK_WEBHOOK_CHORES` | Chore nudges |
| `SEED_SECRET` | Optional — protect seed HTTP endpoint |
| `FAMILY_ICAL_URL` | Optional — Google Calendar iCal |
