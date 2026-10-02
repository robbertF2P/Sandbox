# Platform NuGet feed standard

**Purpose:** Frictionless consumption of **internal Floorganise packages** from **Azure DevOps Artifacts**, using **Microsoft Entra ID** (same accounts as Azure DevOps). No PATs in repos, no per-developer feed secrets.

**Audience:** Platform team, monolith engineers, SandBox maintainers, CI owners.

**Related:** [platform-logging-standard.md](./platform-logging-standard.md) (first consumers), [nuget-vulnerability-jira-workflow.md](./nuget-vulnerability-jira-workflow.md) (audit failures), [foundation-and-pilot-plan.md](./foundation-and-pilot-plan.md) (copy SandBox artifacts into monolith).

---

## Principles

| Principle | Practice |
|-----------|----------|
| **Config in git** | Commit `nuget.config` with feed URL + `packageSourceMapping` only — never passwords or PATs. |
| **Entra for humans** | [Azure Artifacts Credential Provider](https://github.com/microsoft/artifacts-credprovider) + interactive sign-in (or Visual Studio account). |
| **Pipeline auth is automatic** | `NuGetAuthenticate@1` for feeds in the **same** Azure DevOps organization. |
| **One platform feed** | Org-scoped feed for shared packages (`Platform.*`, `ImportPipeline.*`, future `Floorganise.*`). |
| **SandBox publishes; monolith consumes** | Do not project-reference SandBox from production; pin versions in `Directory.Packages.props`. |

---

## Feed layout (recommended)

| Feed | Scope | Contents |
|------|--------|----------|
| **`floorganise-platform`** (name TBD by admins) | **Organization** | `Platform.Serilog.Logging*`, `ImportPipeline.Domain`, other cross-cutting libraries |
| Project feeds | Project | Only when isolation is required (avoid for platform libs) |

**Why organization-scoped:** One URL for every repo; pipelines in any project restore without extra project-level ACL work. See [project-scoped vs organization-scoped feeds](https://learn.microsoft.com/en-us/azure/devops/artifacts/feeds/project-scoped-feeds?view=azure-devops).

**Feed URL shape** (from Artifacts → **Connect to feed** → **dotnet**):

```text
https://pkgs.dev.azure.com/{organization}/_packaging/{feed}/nuget/v3/index.json
```

Replace `{organization}` and `{feed}` with Floorganise values. Do not hand-edit GUIDs; copy from the portal.

---

## Package ID conventions

| Prefix / pattern | Source | Notes |
|------------------|--------|--------|
| `Platform.Serilog.Logging` | SandBox `Platform.Serilog.Logging/` | MSBuild props in monolith still come from copied `build/Platform.Logging.*.props` |
| `Platform.Serilog.Logging.*` | SandBox | Testing, Akka satellite packages |
| `ImportPipeline.Domain` | SandBox `ImportPipeline/` | Import kernel |
| Future bounded-context packages | Monolith extraction | Prefer `Floorganise.{Context}.*` or agreed BC prefix |

Version pins live in **`Directory.Packages.props`** (central package management). Bump feed versions deliberately; see platform logging pack scripts in SandBox.

---

## Developer machine setup

### 1. Install the credential provider (once per machine)

Self-contained install (Windows, macOS, Linux — no separate .NET runtime required for the provider):

**Windows (PowerShell):**

```powershell
iex "& { $(irm https://aka.ms/install-artifacts-credprovider.ps1) }"
```

Add `-AddNetfx` when using **nuget.exe** or older MSBuild paths that do not use the .NET Core plugin.

**macOS / Linux:**

```bash
sh -c "$(curl -fsSL https://aka.ms/install-artifacts-credprovider.sh)"
```

Alternative: `dotnet tool install --global Microsoft.Artifacts.CredentialProvider.NuGet.Tool` (from nuget.org).

Optional: add the Windows/macOS/Linux block to onboarding or a repo script `scripts/bootstrap-nuget-credprovider.sh` / `.ps1` (no secrets).

### 2. Sign in (first restore)

From a solution directory that uses the committed `nuget.config`:

```bash
dotnet restore --interactive
```

Browser or device login uses the developer’s **work Entra** account. Cached session tokens are reused until expiry ([session token cache](https://learn.microsoft.com/en-us/azure/devops/artifacts/nuget/dotnet-setup?view=azure-devops)).

**Visual Studio 2022:** Sign in with the same Entra account under **Account Settings**. NuGet restore should prompt with feed context when needed ([VS NuGet auth improvements](https://devblogs.microsoft.com/visualstudio/connecting-to-private-nuget-feeds-just-got-easier/)).

### 3. Do not use PATs for daily restore

Personal Access Tokens are for legacy tools and exceptional automation only. PATs in `nuget.config` or wiki pages create rotation pain and leak risk.

---

## Committed `nuget.config` (monolith and apps)

Place at repo root (or solution root). Copy from [starter-kit/templates/nuget.config](./starter-kit/templates/nuget.config) and set the feed URL.

**Rules:**

- Register **`nuget.org`** and the **Artifacts** feed.
- Use **`packageSourceMapping`** so only internal IDs resolve from Artifacts; all other packages use nuget.org (avoids spurious auth failures on public packages).
- Never commit `packageSourceCredentials` with PATs.

**Mapping** must include every internal package ID (and glob patterns MSBuild/NuGet accepts), e.g. `Platform.Serilog.Logging*`, `ImportPipeline.*`.

---

## SandBox vs Azure Artifacts

| Environment | Feed | When |
|-------------|------|------|
| **SandBox local dev** | `local-feed/` (see `F2pPlatform/NuGet.config`) | Developing platform packages; run `./scripts/pack-local-platform-packages.sh` |
| **Monolith / CI / other repos** | Azure Artifacts `floorganise-platform` | Normal consumption after publish from SandBox pipeline |
| **Platform.Serilog.Logging solution** | nuget.org only in `Platform.Serilog.Logging/nuget.config` | Avoids ambiguous resolution while authoring the package |

Publishing from SandBox (target state):

1. Merge platform package changes.
2. CI or release pipeline bumps version in `build/Platform.Logging.Versions.props` (and siblings).
3. `./scripts/pack-local-platform-packages.sh` (or dedicated pack job) produces `.nupkg`.
4. `dotnet nuget push` to Artifacts (authenticated pipeline identity with **Feed Publisher**).

Until the publish pipeline exists, monolith laptops may temporarily use bundled `local-feed/` per [PLATFORM-V2-LAPTOP-HANDOFF.md](./PLATFORM-V2-LAPTOP-HANDOFF.md).

---

## Azure DevOps permissions

| Identity | Feed role | Purpose |
|----------|-----------|---------|
| **Project Collection Valid Users** (or org developers group) | **Feed Reader** | Restore on dev machines (via Entra + credential provider) |
| **Platform / release pipeline** | **Feed Publisher (Contributor)** | Push packages from SandBox |
| **Project / collection build service** | **Feed and Upstream Reader** (default) or **Contributor** if pushing | `NuGetAuthenticate@1` in CI |

Grant **Feed Reader** at minimum for all engineers who build code referencing internal packages. Platform admins: **Feed Owner** on the platform feed only.

For a **project-scoped** feed consumed from another project’s pipeline, also add the consuming **Build Service** to the hosting project **Readers** — see [feed permissions](https://learn.microsoft.com/en-us/azure/devops/artifacts/feeds/feed-permissions?view=azure-devops).

---

## CI (Azure Pipelines)

Before `dotnet restore`, `dotnet build`, or `dotnet nuget push`:

```yaml
- task: NuGetAuthenticate@1
  displayName: Authenticate to Azure Artifacts

- script: dotnet restore $(Build.SourcesDirectory)/Your.sln
  displayName: Restore
```

Feeds in the **same organization** as the pipeline require no extra service connection.

**Cross-organization** or external authenticated feeds: set `nuGetServiceConnections` on `NuGetAuthenticate@1`. Prefer [Entra workload identity service connections](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/add-devops-entra-service-connection?view=azure-devops) over long-lived PATs.

**Docker / headless agents:** Use pipeline authentication inside the build that produces the image; do not copy developer tokens into images.

---

## Upstream sources (optional)

On the platform feed, enabling **nuget.org** as an upstream source lets clients authenticate only to Artifacts while still restoring public packages. Tradeoffs:

- **Pros:** Single credential; cached public packages; alignment with org supply-chain policies.
- **Cons:** Feed becomes a choke point; requires upstream and retention policies.

If upstream is **disabled**, keep `nuget.org` in `nuget.config` with `packageSourceMapping` as in the template (recommended default in this standard).

---

## New repo / module checklist

1. Add root `nuget.config` from [starter-kit/templates/nuget.config](./starter-kit/templates/nuget.config).
2. Import `build/Platform.Logging.Versions.props` in `Directory.Packages.props` when using platform logging.
3. Install credential provider (onboarding doc or script).
4. Run `dotnet restore --interactive` once after clone.
5. Ensure pipeline includes `NuGetAuthenticate@1` before restore.
6. Confirm developer is in the org and has **Feed Reader** on the platform feed.

---

## Troubleshooting

| Symptom | Likely cause | Action |
|---------|----------------|--------|
| 401 on restore | Not signed in or wrong account | `dotnet restore -i`; verify Entra account in VS; check [NuGet Artifacts wiki](https://github.com/NuGet/Home/wiki/Azure-DevOps-Artifacts-authentication-issues) |
| 403 on feed URL in browser | No feed permission | Admin adds user or group as **Feed Reader** |
| Public package fails only when Artifacts is configured | Missing `packageSourceMapping` | Map `*` to nuget.org; internal IDs to Artifacts |
| CI fails, local works | Missing `NuGetAuthenticate@1` or build identity not on feed | Add task; grant **Build Service** on feed |
| `Platform.Serilog.Logging` not found | Version not published or wrong feed | Check Artifacts versions; align `Directory.Packages.props` |

---

## Further reading

| Topic | Link |
|-------|------|
| dotnet client setup | [Connect to Azure Artifacts feeds (dotnet)](https://learn.microsoft.com/en-us/azure/devops/artifacts/nuget/dotnet-setup?view=azure-devops) |
| Credential provider | [microsoft/artifacts-credprovider](https://github.com/microsoft/artifacts-credprovider) |
| Pipeline task | [NuGetAuthenticate@1](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/nuget-authenticate-v1?view=azure-pipelines) |
| NuGet audit → Jira | [nuget-vulnerability-jira-workflow.md](./nuget-vulnerability-jira-workflow.md) |

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-10-02 | Initial standard — Artifacts + Entra; SandBox publish / monolith consume |
