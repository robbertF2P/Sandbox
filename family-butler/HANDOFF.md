# Family Butler — Handoff

Continue this project on your laptop. Paste the **Context block** below into a new Cursor chat, or point the agent at this file.

## Context block (paste into Cursor)

```
We're building a "family butler" on Val Town + Slack.

Household:
- Parents: me + Anna (wife, Eva & Milan's mum). Anna rides horse Kyana.
- Milan (14): walks Mylo (his dog) daily ~16:00–17:00. Not shared rotation.
- Eva (13): owns pony Rosa.
- Cat: Pepsi. Dog: Mylo (Milan's).

Horses Kyana + Rosa are BOARDED — stable feeds/cleans daily. No AM/PM stable chore reminders.
Only riding/lesson/appointment reminders for Anna (Kyana) and Eva (Rosa).

Home chores for kids:
- Clean own room (weekly, Saturday)
- Unload dishwasher (alternate daily Milan/Eva)
- Take out trash (alternate ~2x/week — confirm collection day)
- Milan: Mylo walk daily 4–5pm
- Eva: Pepsi feed + litter (default)

Notifications: Slack (not WhatsApp). Channels: #daily, #chores, #meals.
Val Town: cron + SQLite + Slack webhooks + optional Slack bot for "done mylo" replies.

Stack: Val Town (https://docs.val.town), Slack incoming webhooks + bot.
See family-butler/HANDOFF.md in repo for full spec.
```

## Your laptop setup

### 1. Get this spec

If this was pushed to git:
```bash
git pull
```
Or copy `family-butler/HANDOFF.md` from the cloud agent session.

### 2. Cursor MCP (optional but useful)

**Val Town** — deploy vals from Cursor:
- Docs: https://docs.val.town/guides/prompting/mcp/
- Add: `https://api.val.town/v3/mcp` → OAuth in Cursor Settings → MCP

**Slack** — post/read Slack from Cursor:
- Cursor → Settings → MCP → add Slack (`https://mcp.slack.com/mcp` or bot token)
- Docs: https://docs.slack.dev/ai/slack-mcp-server/connect-to-cursor/

Cloud Agents only see MCPs configured in that environment; your laptop MCPs are separate.

### 3. Val Town account

1. Sign in at https://www.val.town
2. Create val: `family-butler`
3. Add env vars (sidebar): `SLACK_WEBHOOK_DAILY`, `SLACK_WEBHOOK_CHORES`, etc.

### 4. Slack workspace

1. Create or use existing workspace; invite Anna, Milan, Eva
2. Channels: `#daily`, `#chores`, `#meals`
3. Incoming webhooks: https://docs.val.town/guides/slack/send-messages-to-slack/
4. (Later) Slack bot for inbound `done mylo`, `done dishwasher`, `dinner: pasta`

### 5. Calendar (optional v1)

Google Calendar → secret iCal URL → Val Town env `FAMILY_ICAL_URL`

---

## Build order

| Step | What |
|------|------|
| 1 | Slack webhooks + test post from Val Town |
| 2 | SQLite schema + seed config (names, rotations) |
| 3 | Morning cron → `#daily` briefing |
| 4 | 15:45 cron → DM or @Milan Mylo reminder |
| 5 | Dishwasher/trash rotation in `#chores` |
| 6 | Slack bot webhook → parse `done …` / `dinner: …` |
| 7 | OpenAI via std/openai for dinner suggestions |

---

## Database seed (SQL files)

Stored in **`family-butler/sql/`**:

| File | Contents |
|------|----------|
| `schema.sql` | Table definitions |
| `seed.sql` | Config + chore rotations (Milan, Eva, Anna, Mylo, Pepsi, Kyana, Rosa) |
| `README.md` | How to apply on Val Town |

One-shot Val Town runner: `valtown/seedDatabase.example.ts`

## Cron schedule (adjust timezone → UTC via crongpt.com)

| Job | Local intent | Notes |
|-----|--------------|-------|
| Morning brief | ~07:00 | `#daily` |
| Dishwasher | ~07:45 | `#chores` |
| Mylo walk | 15:45 | @Milan |
| Evening wrap | ~20:30 | open chores |
| Room check | Sat 10:00 | `#chores` |

---

## Still TBD

- [ ] Your name (for briefings)
- [ ] Trash collection day
- [ ] Timezone (e.g. Europe/Amsterdam)
- [ ] Val Town Free vs Pro
- [ ] Briefing language: Dutch / English / mixed
- [ ] Dishwasher: morning vs after dinner

---

## Useful links

- Val Town cron: https://docs.val.town/guides/first-cron/
- Val Town Slack: https://docs.val.town/guides/slack/send-messages-to-slack/
- Val Town Slack bot: https://docs.val.town/guides/slack/bot/
- Val Town SQLite: https://docs.val.town/reference/std/sqlite/
- Val Town OpenAI: https://docs.val.town/reference/std/openai/
