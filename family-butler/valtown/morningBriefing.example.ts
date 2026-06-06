// Paste into Val Town val "family-butler" — morning cron trigger
// Env: SLACK_WEBHOOK_DAILY, optional FAMILY_ICAL_URL

import sqlite from "https://esm.town/v/std/sqlite/global.ts";
import { email } from "https://esm.town/v/std/email";
import { IncomingWebhook } from "npm:@slack/webhook";
import { OpenAI } from "https://esm.town/v/std/openai";

export default async function (_interval: Interval) {
  const today = new Date().toISOString().slice(0, 10);
  const dow = new Date().getDay();

  await sqlite.execute(`
    CREATE TABLE IF NOT EXISTS config (key TEXT PRIMARY KEY, value TEXT);
    CREATE TABLE IF NOT EXISTS meals (date TEXT PRIMARY KEY, dish TEXT NOT NULL);
    CREATE TABLE IF NOT EXISTS rotation (
      task TEXT, day_of_week INTEGER, assignee TEXT, assist TEXT
    );
  `);

  const meals = await sqlite.execute(
    "SELECT date, dish FROM meals ORDER BY date DESC LIMIT 7",
  );
  const chores = await sqlite.execute({
    sql: "SELECT task, assignee FROM rotation WHERE day_of_week = ?",
    args: [dow],
  });

  const openai = new OpenAI();
  const completion = await openai.chat.completions.create({
    model: "gpt-4o-mini",
    messages: [{
      role: "user",
      content: `Family butler morning brief for ${today}.
Parents: me + Anna. Kids: Milan (14), Eva (13).
Mylo (dog) = Milan walks daily 4-5pm. Cat Pepsi. Horse Kyana (Anna), pony Rosa (Eva) — boarded, no stable chores.
Recent meals: ${JSON.stringify(meals.rows)}
Today's rotated chores: ${JSON.stringify(chores.rows)}
Write a short Slack-friendly briefing. Include dinner suggestion. Note Milan Mylo walk.`,
    }],
  });

  const text = completion.choices[0].message.content ?? "";

  const webhookUrl = Deno.env.get("SLACK_WEBHOOK_DAILY");
  if (webhookUrl) {
    const slack = new IncomingWebhook(webhookUrl);
    await slack.send(text);
  } else {
    await email({ subject: `Family brief ${today}`, text });
  }
}
