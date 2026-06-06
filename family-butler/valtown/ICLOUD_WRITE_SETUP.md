# iCloud calendar write path (for Slack → add appointment)

The **public iCal URL** is read-only. To **create** events on your iPhone Family calendar, the butler uses **CalDAV** with an **app-specific password**.

## Overview

```
Slack: "add Eva Rosa Sat 10:00"
    → Val Town Slack bot
    → CalDAV PUT to Family calendar
    → Appears on everyone's iPhone Calendar app
```

---

## Step 1 — Use the right Apple ID

The butler writes through **one iCloud account** that **owns** the shared Family calendar.

- Usually **your** Apple ID (the one that created/shared "Family")
- Events appear on the shared calendar for Anna, Milan, Eva as today

---

## Step 2 — App-specific password

1. Go to [appleid.apple.com](https://appleid.apple.com)
2. **Sign-In and Security** → **App-Specific Passwords**
3. **Generate** → name it e.g. `Val Town Family Butler`
4. Copy the password (format `xxxx-xxxx-xxxx-xxxx`) — shown once

Requirements:
- Two-factor authentication must be on
- This is **not** your normal Apple ID password
- If you change your Apple ID password, generate a new app-specific password

---

## Step 3 — Find your Family calendar URL (one-time)

CalDAV needs the **calendar URL**, not the public iCal link.

### Option A — Val Town discover script (easiest)

1. In your `family-butler` val, add `discoverCalendars.ts` (see `discoverCalendars.example.ts`)
2. Set env vars (Step 4) except `ICLOUD_CALENDAR_URL`
3. Add **HTTP** trigger → open URL once
4. Response lists calendars — copy the **url** for your Family calendar
5. Set `ICLOUD_CALENDAR_URL` to that value

### Option B — Mac Terminal

```bash
export APPLE_ID="you@icloud.com"
export APPLE_APP_PASSWORD="xxxx-xxxx-xxxx-xxxx"

# List calendar names + paths (after CalDAV discovery — use tsdav or the discover val)
```

Using **tsdav** in a one-off Val Town Run is simpler than raw curl.

---

## Step 4 — Val Town environment variables

In the val sidebar → **Environment variables**:

| Variable | Example | Secret? |
|----------|---------|---------|
| `ICLOUD_APPLE_ID` | `you@icloud.com` | Yes |
| `ICLOUD_APP_PASSWORD` | `abcd-efgh-ijkl-mnop` | **Yes — never commit** |
| `ICLOUD_CALENDAR_URL` | `https://pXX-caldav.icloud.com:443/…/calendars/UUID/` | Yes |
| `ICLOUD_CALENDAR_NAME` | `Family` | No (for logs only) |

Server is always: `https://caldav.icloud.com` (tsdav discovers the `pXX-caldav` host).

---

## Step 5 — Wire into the Slack bot

When a parent Slack message matches `add …` or `appointment …`:

1. Parse title, date, time (regex or LLM)
2. Call `createCalendarObject` via tsdav
3. Reply in Slack: `✅ Added to Family calendar: …`

Restrict **who can add** events: check Slack user ID against allowlist (you + Anna).

---

## Security checklist

- [ ] App-specific password only in Val Town env vars
- [ ] Slack `add` commands limited to parents
- [ ] Optional: require `yes` confirmation for ambiguous parses
- [ ] Rotate app-specific password if leaked
- [ ] Do **not** put credentials in Slack messages or git

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| 401 Unauthorized | Wrong Apple ID or app password; regenerate password |
| Calendar not found | Re-run discover; pick correct Family calendar UUID |
| Event created but wrong calendar | Update `ICLOUD_CALENDAR_URL` |
| Works on Mac, not Val Town | Use `npm:tsdav` in Val Town (server-side, not browser) |

---

## Read + write together

| Purpose | Method | Env var |
|---------|--------|---------|
| **Read** (morning brief) | Public iCal URL | `FAMILY_ICAL_URL` |
| **Write** (Slack add) | CalDAV | `ICLOUD_*` vars above |

Both can point at the same Family calendar.
