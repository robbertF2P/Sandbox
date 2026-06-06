// One-time: discover iCloud calendars and find Family calendar URL.
// Env: ICLOUD_APPLE_ID, ICLOUD_APP_PASSWORD
// HTTP trigger → open URL → copy calendar url into ICLOUD_CALENDAR_URL

import { createDAVClient } from "npm:tsdav";

export default async function (): Promise<Response> {
  const username = Deno.env.get("ICLOUD_APPLE_ID");
  const password = Deno.env.get("ICLOUD_APP_PASSWORD");
  if (!username || !password) {
    return Response.json({ error: "Set ICLOUD_APPLE_ID and ICLOUD_APP_PASSWORD" }, { status: 400 });
  }

  const client = await createDAVClient({
    serverUrl: "https://caldav.icloud.com",
    credentials: { username, password },
    authMethod: "Basic",
    defaultAccountType: "caldav",
  });

  const calendars = await client.fetchCalendars();

  const list = calendars.map((c) => ({
    name: c.displayName ?? "(unnamed)",
    url: c.url,
  }));

  return Response.json({
    message: "Copy the url for your Family calendar into ICLOUD_CALENDAR_URL",
    calendars: list,
  });
}
