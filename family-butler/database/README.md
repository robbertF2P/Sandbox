# Family butler — database seed

SQL files for the Val Town SQLite database (`std/sqlite/global.ts`).

| File | Purpose |
|------|---------|
| `schema.sql` | Creates `config`, `meals`, `chore_log`, `rotation` |
| `seed.sql` | Household names, animals, chore rotations |

## Apply on Val Town

**Option A — paste in Val Town SQL sidebar**  
Run `schema.sql`, then `seed.sql`.

**Option B — HTTP seed val**  
Copy `valtown/seedDatabase.example.ts` into your val, set env `SEED_SECRET`, hit the endpoint once.

**Option C — from Cursor with Val Town MCP**  
Ask the agent to run the contents of these files against your `family-butler` val database.

## Edit before go-live

- `seed.sql` → `timezone`, `briefing_language`, trash days
- Add your name to `config` when ready (`parent_1`)
