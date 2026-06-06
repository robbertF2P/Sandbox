// Slack: add Eva Rosa riding Sat 10:00
// Requires: ICLOUD_APPLE_ID, ICLOUD_APP_PASSWORD, ICLOUD_CALENDAR_URL

import { createDAVClient } from "npm:tsdav";
import ICAL from "npm:ical.js";

function buildIcs(summary: string, start: Date, end: Date): string {
  const comp = new ICAL.Component(["vcalendar", [], []]);
  comp.updatePropertyWithValue("prodid", "-//Family Butler//EN");
  comp.updatePropertyWithValue("version", "2.0");
  const vevent = new ICAL.Component("vevent");
  const uid = crypto.randomUUID();
  vevent.updatePropertyWithValue("uid", uid);
  vevent.updatePropertyWithValue("summary", summary);
  vevent.updatePropertyWithValue("dtstamp", ICAL.Time.fromJSDate(new Date(), true));
  vevent.updatePropertyWithValue("dtstart", ICAL.Time.fromJSDate(start, true));
  vevent.updatePropertyWithValue("dtend", ICAL.Time.fromJSDate(end, true));
  comp.addSubcomponent(vevent);
  return comp.toString();
}

export async function createFamilyEvent(
  summary: string,
  start: Date,
  end: Date,
): Promise<void> {
  const username = Deno.env.get("ICLOUD_APPLE_ID")!;
  const password = Deno.env.get("ICLOUD_APP_PASSWORD")!;
  const calendarUrl = Deno.env.get("ICLOUD_CALENDAR_URL")!;

  const client = await createDAVClient({
    serverUrl: "https://caldav.icloud.com",
    credentials: { username, password },
    authMethod: "Basic",
    defaultAccountType: "caldav",
  });

  const iCalString = buildIcs(summary, start, end);
  await client.createCalendarObject({
    calendar: { url: calendarUrl },
    filename: `${crypto.randomUUID()}.ics`,
    iCalString,
  });
}

// Example parse: "add Eva Rosa riding Sat 10:00" — wire to Slack bot next
export default async function (req: Request): Promise<Response> {
  const secret = Deno.env.get("SEED_SECRET");
  if (secret && req.headers.get("authorization") !== `Bearer ${secret}`) {
    return new Response("Unauthorized", { status: 401 });
  }

  const { summary, start, end } = await req.json();
  await createFamilyEvent(summary, new Date(start), new Date(end));
  return Response.json({ ok: true, summary });
}
