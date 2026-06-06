# Family butler — database seed

SQL source files (edit these, then regenerate the Val Town runner):

| File | Purpose |
|------|---------|
| `schema.sql` | Table definitions |
| `seed.sql` | Household config + chore rotations |

## Apply on Val Town — no manual SQL

**Use [`../valtown/seedDatabase.ts`](../valtown/seedDatabase.ts)** — paste into a Val Town val and click **Run**.

Full steps: [`../valtown/SETUP.md`](../valtown/SETUP.md)

## Regenerate after editing SQL

```bash
node family-butler/scripts/generate-seed-val.mjs
```

This rewrites `valtown/seedDatabase.ts` from `schema.sql` + `seed.sql`.
