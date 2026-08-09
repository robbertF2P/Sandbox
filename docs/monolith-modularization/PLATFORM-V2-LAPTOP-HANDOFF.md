# Platform 2.0 — laptop handoff pack

Portable bundle of **plan documents**, **starter kit**, **pilot code**, and **agent rules** from SandBox so you can get a running start on the real Floor2Plan monolith (or run pilots standalone on your work laptop).

## Create the bundle (SandBox repo)

From the SandBox root:

```bash
./scripts/bundle-platform-v2-handoff.sh
```

Output:

```text
dist/platform-v2-handoff-<date>-<git-sha>.tar.gz
dist/platform-v2-handoff-<date>-<git-sha>.zip   # when zip is available
```

Options:

| Flag / env | Purpose |
|------------|---------|
| `--skip-pack` | Do not run `pack-local-platform-packages.sh` (faster; you pack NuGets yourself) |
| `--output-dir dist` | Where archives are written |
| `HANDOFF_PACK_NUGETS=0` | Same as `--skip-pack` |

The script packs `Platform.Serilog.Logging` (and related) into `local-feed/` when `dotnet` is available, then copies everything into a self-contained folder and archives it.

## On your work laptop

### Option A — Run pilots standalone (fastest feedback loop)

1. Copy or extract the archive anywhere (e.g. `~/floorganise/platform-v2-handoff/`).
2. Verify:

   ```bash
   cd platform-v2-handoff
   ./verify-handoff.sh
   ```

3. Build and test the V2 host:

   ```bash
   cd pilot/F2pPlatform
   ../../scripts/pack-local-platform-packages.sh   # once, if local-feed is empty
   dotnet build
   dotnet test
   dotnet run --project host/F2pPlatform.Host
   ```

4. Thin domain POC (no HTTP host):

   ```bash
   cd pilot/PlanningApprovalsPoc
   dotnet test
   ```

5. Full stack with UI (Podman/Docker): see `pilot/F2pPlatform/README.md`.

### Option B — Install into the monolith repo

From the extracted bundle root:

```bash
./install-into-monolith.sh /path/to/Floor2Plan.Core2
```

This copies:

| Into monolith | From bundle |
|---------------|-------------|
| `docs/modularization/` | Plan docs, starter kit, templates, quality framework |
| `docs/modularization/examples/` | Sample Phase 0 inventory outputs |
| `docs/coding-standards/` | C# / Angular standards (if not already present) |
| `Build/Platform/*.props` | Serilog MSBuild imports |
| Selected `scripts/` | Scaffold + pack helpers |
| `docs/modularization/agent-rules.md` | Agent snippet |
| `.github/copilot-instructions.md` | Same snippet for Copilot |

Optional pilot reference (off by default):

```bash
./install-into-monolith.sh /path/to/Floor2Plan.Core2 --with-pilot-reference
```

Copies `pilot/F2pPlatform` → `<monolith>/Src/Platform/F2pPlatform` (adjust layout in the script if your monolith uses a different path).

### Option C — Git (if you have SandBox access)

Clone or pull SandBox on the laptop and run the bundle script there, or work directly in SandBox for POCs and copy outcomes into the monolith via `install-into-monolith.sh`.

## What is in the bundle

```text
platform-v2-handoff/
├── HANDOFF.md                 ← quick start (copy of this doc, trimmed)
├── VERSION.txt                ← SandBox git SHA + date
├── verify-handoff.sh
├── install-into-monolith.sh
├── docs/
│   ├── modularization/        ← all plan + starter-kit docs (monolith path names)
│   ├── coding-standards/
│   └── floor2plan-*.md        ← connector / read-model playbooks
├── build/                     ← Platform.Logging.*.props
├── scripts/                   ← scaffold-module, pack-*, add-platform-logging
├── pilot/
│   ├── F2pPlatform/           ← runnable V2 host + HourApprovals slice + web
│   └── PlanningApprovalsPoc/  ← thin domain POC
├── platform/
│   ├── Platform.Serilog.Logging/
│   └── local-feed/            ← packed NuGets (when pack step ran)
└── agent/
    ├── agent-rules.md
    ├── copilot-instructions.md
    └── skills/                ← .cursor/skills snapshot for Cursor on laptop
```

## First week on the monolith (Phase A)

1. Run `install-into-monolith.sh`.
2. Read `docs/modularization/foundation-and-pilot-plan.md` (Phase A).
3. Paste agent rules — already at `docs/modularization/agent-rules.md` and `.github/copilot-instructions.md`.
4. Point `nuget.config` at your feed **or** the bundled `platform/local-feed` for `Platform.Serilog.Logging`.
5. Agent prompt for Phase 0 only (inventory) — see foundation plan § A1.
6. Prove the harness: one smoke characterization test before extracting production code.

## AI verification loop (why this pack exists)

Agents should verify with **module-scoped** commands, not full monolith boot:

```bash
dotnet test tests/Modules/<Context>/<Context>.Characterization.Tests/
dotnet build src/Modules/<Context>/<Context>.Domain/
```

The V2 host uses `Testing` environment + stub identity + SQLite — see `pilot/F2pPlatform/tests/Modules/*/`.

## Troubleshooting

| Issue | Fix |
|-------|-----|
| NuGet restore fails for `Platform.Serilog.Logging` | Run `scripts/pack-local-platform-packages.sh` from bundle root; add `platform/local-feed` to `nuget.config` |
| `dotnet` not found | Install .NET 10 SDK (see `F2pPlatform` `global.json` if present) |
| Frontend `npm install` slow | Use `pilot/F2pPlatform/web`; exclude `node_modules` from sync — run `npm ci` on laptop |
| Path layout differs in monolith | Edit `install-into-monolith.sh` `MONO_PLATFORM_DIR` before running |

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-08-08 | Initial handoff bundle script + installer |
