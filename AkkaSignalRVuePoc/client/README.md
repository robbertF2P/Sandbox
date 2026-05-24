# Vue SignalR Client

This Vite + Vue client connects to the ASP.NET Core SignalR hub at `http://localhost:5000/hubs/live-messages` by default and displays messages pushed by the Akka.NET actor.

## Commands

```bash
npm install
npm run dev
npm run build
```

Set `VITE_SIGNALR_HUB_URL` in `.env.local` to point at a different API host.
