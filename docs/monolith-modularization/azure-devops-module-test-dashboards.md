# Azure DevOps — Per-Module Test Dashboards

**Purpose:** Give each bounded-context module a **simple, always-current dashboard** of automated test results in Azure DevOps — linked to use cases (`UC-###`), test cases (`TC-###`), and acceptance criteria (`AC-###`) from the modularization program.

**Audience:** Tech leads, module owners, management.

**Prerequisite:** Tests run in Azure Pipelines on `main` (and ideally on PRs). Test projects align one-to-one with target modules.

---

## What “good” looks like

Each module (e.g. **Import**, **Hours**, **WBS**) has:

1. A **dedicated test job** in CI that publishes results with a stable `testRunTitle`
2. **Traits** on every automated test mapping `Module`, `UC`, `Tier`
3. A **module dashboard** (5–6 widgets) showing pass/fail trend and latest failures
4. An **overview dashboard** rolling up all modules for management

```text
Overview dashboard (management)
├── Import module dashboard
├── Hours module dashboard
├── WBS module dashboard
└── …
```

---

## Step 1 — Align test projects to modules

Target layout (external monolith / Platform 2.0):

```text
tests/
├── Import.Characterization.Tests/
├── Import.Integration.Tests/
├── Hours.Characterization.Tests/
├── Hours.Integration.Tests/
├── Wbs.Integration.Tests/
└── …
```

**Rule:** One bounded context → at least one test project. P0 use cases must have tests in that project before module extraction merges.

---

## Step 2 — Tag tests for Azure DevOps filtering

Use **xUnit traits** (or NUnit `Category`) on every test method. Azure DevOps surfaces these in test results and Analytics.

### Required traits

| Trait | Example | Purpose |
|-------|---------|---------|
| `Module` | `Import` | Dashboard filter per bounded context |
| `Tier` | `P0` | Show critical-path health |
| `Type` | `Characterization` | Distinguish char. vs integration vs unit |
| `UC` | `UC-IMPORT-001` | Traceability to use case |
| `TC` | `TC-IMPORT-001` | Traceability to test plan (optional) |

### C# example

```csharp
[Fact]
[Trait("Module", "Import")]
[Trait("Tier", "P0")]
[Trait("Type", "Characterization")]
[Trait("UC", "UC-IMPORT-001")]
[Trait("TC", "TC-IMPORT-001")]
[Trait("AC", "AC-IMPORT-001-02")]
public async Task Import_SamePlmFileTwice_UpdatesByExternalId()
{
    // ...
}
```

### Assembly-level default (reduce repetition)

In `Import.Integration.Tests/AssemblyInfo.cs` or `ModuleImportTestFramework.cs`:

```csharp
[assembly: AssemblyTrait("Module", "Import")]
```

Method-level traits override or supplement assembly traits.

### Test naming

Keep names aligned with `test-cases.yaml`:

```text
<Context>_<UseCase>_<Scenario>_<ExpectedOutcome>
Import_ImportProject_ValidPlmFile_CreatesExternalIds
```

---

## Step 3 — Pipeline: one test job per module

Use a **single pipeline** on `main` with **parallel jobs** — one per module. Each job publishes results with a distinct `testRunTitle`.

### `azure-pipelines.yml` (excerpt)

```yaml
trigger:
  branches:
    include: [main]

pr:
  branches:
    include: [main]

variables:
  buildConfiguration: Release
  dotnetVersion: '8.0.x'

stages:
- stage: Build
  jobs:
  - job: Build
    pool:
      vmImage: ubuntu-latest
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: sdk
        version: $(dotnetVersion)
    - script: dotnet restore
      displayName: Restore
    - script: dotnet build --configuration $(buildConfiguration) --no-restore
      displayName: Build

- stage: ModuleTests
  dependsOn: Build
  jobs:
  - job: Import_Tests
    displayName: 'Module: Import'
    pool:
      vmImage: ubuntu-latest
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: sdk
        version: $(dotnetVersion)
    - script: |
        dotnet test tests/Import.Integration.Tests/Import.Integration.Tests.csproj \
          --configuration $(buildConfiguration) \
          --no-build \
          --logger "trx;LogFileName=import.trx" \
          --results-directory "$(Agent.TempDirectory)/test-results/import"
      displayName: Run Import tests
    - task: PublishTestResults@2
      condition: always()
      inputs:
        testResultsFormat: VSTest
        testResultsFiles: '$(Agent.TempDirectory)/test-results/import/**/*.trx'
        testRunTitle: 'Module: Import'
        mergeTestResults: false
        failTaskOnFailedTests: true
        publishRunAttachments: true

  - job: Hours_Tests
    displayName: 'Module: Hours'
    pool:
      vmImage: ubuntu-latest
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: sdk
        version: $(dotnetVersion)
    - script: |
        dotnet test tests/Hours.Integration.Tests/Hours.Integration.Tests.csproj \
          --configuration $(buildConfiguration) \
          --logger "trx;LogFileName=hours.trx" \
          --results-directory "$(Agent.TempDirectory)/test-results/hours"
      displayName: Run Hours tests
    - task: PublishTestResults@2
      condition: always()
      inputs:
        testResultsFormat: VSTest
        testResultsFiles: '$(Agent.TempDirectory)/test-results/hours/**/*.trx'
        testRunTitle: 'Module: Hours'
        mergeTestResults: false
        failTaskOnFailedTests: true

  # Repeat for Wbs_Tests, Planning_Tests, etc.
```

### PR validation

On PR pipelines, run **only modules touched** (path filters) plus **Import** if shared infrastructure changed:

```yaml
- job: Import_Tests
  condition: or(
    eq(variables['Build.Reason'], 'PullRequest'),
    contains(variables['Build.SourceVersionMessage'], '[full-test]'))
```

Or use path filters in a separate `azure-pipelines.pr.yml`.

### Build tags (optional)

After each module job succeeds, tag the build for dashboard filters:

```yaml
- script: echo "##vso[build.addbuildtag]module-import"
  displayName: Tag build (Import)
  condition: succeeded()
```

---

## Step 4 — Create per-module dashboards in Azure DevOps

**Project → Overview → Dashboards → New dashboard**

Create one dashboard per module, e.g. `Module — Import`.

### Recommended widgets (simple set)

| Widget | Configuration | Shows |
|--------|---------------|-------|
| **Markdown** | Module name, owner, link to test project repo path, link to `use-cases.yaml` | Context |
| **Test results trend** | Build pipeline = your pipeline; filter **Test run title** contains `Module: Import`; last 14 days | Pass/fail over time |
| **Test failures** | Same pipeline; test run title `Module: Import` | Latest failing tests |
| **Build history** | Pipeline; branch `main`; last 10 builds | Build health |
| **Chart for test results** *(Analytics)* | Pass rate by `Test Run Title` or `Module` trait; last 30 days | Management-friendly % |
| **Query results** | Work items: bugs tagged `module:import` and State = Active | Open defects |

### Test results trend — filter tip

In widget settings:

- **Pipeline:** `F2P-Platform-CI` (your pipeline name)
- **Branch:** `main`
- **Test run for:** Custom → title contains `Module: Import`

Each module uses the same pipeline but a **different test run title** — keeps one pipeline, separate dashboard slices.

### Overview dashboard (management)

Single page with:

- One **Chart for test results** per module (small multiples) or one chart grouped by test run title
- **Markdown** summary table (can be updated by release pipeline or manually):

```markdown
| Module  | P0 pass rate (7d) | Last green main | Open P0 failures |
|---------|-------------------|-----------------|------------------|
| Import  | see widget →    | #12345          | 0                |
| Hours   | see widget →    | #12345          | 1                |
```

---

## Step 5 — Link tests to use cases (optional but valuable)

### Option A — Traits only (lightweight)

Traits `UC` / `TC` / `AC` on tests; traceability lives in repo YAML + ADO test result filters. **Recommended to start.**

### Option B — Azure Test Plans

1. Create test plan **F2P Modularization — Automated**
2. One **test suite per module** (Import, Hours, …)
3. For each P0 `TC-###`, create a **linked automated test** (ADO links to pipeline test by name)
4. Module dashboard adds **Test plan progress** widget filtered to that suite

Use when domain experts want a familiar Test Plans UI; traits remain the source of truth for CI.

### Option C — Work item links

Create User Story work items `US-###` / Test Case `TC-###` in ADO. In PR template, require:

```markdown
- Related: AB#12345 (TC-IMPORT-001)
```

Query widget on module dashboard: `Tags Contains module:import AND Work Item Type = Test Case`.

---

## Step 6 — Reporting P0 / AC coverage on the dashboard

Add a **pipeline job** that publishes a small markdown artifact from `test-cases.yaml` + TRX parse (optional script):

```yaml
- job: TestCoverageReport
  dependsOn: ModuleTests
  steps:
  - script: dotnet run --project tools/TestCoverageReport -- --module Import
  - publish: $(Build.ArtifactStagingDirectory)/module-import-coverage.md
    artifact: module-import-coverage
```

Keep the script simple: count `TC-###` with `status: implemented` vs total P0 for module. Display via Markdown widget linking to latest artifact — or check into `docs/modularization/` on scheduled build.

**Minimum without scripting:** Markdown widget manually updated weekly until script exists — acceptable for pilot.

---

## Step 7 — Alerts

**Project Settings → Notifications** (or Pipeline alerts):

| Alert | When | Audience |
|-------|------|----------|
| Build fails on `main` | Module test job fails | Module owner |
| Test regression | P0 test newly failing (compare to previous run) | Tech lead |
| Nightly summary | Scheduled multi-module run | Management distro list |

Enable **"A build that I started"** and **"A build completed"** for module owners on their pipeline.

---

## Module dashboard checklist (Definition of Done)

- [ ] Test project exists: `<Module>.Integration.Tests` (and Characterization if P0)
- [ ] All P0 tests have `[Trait("Module", "<Module>")]` and `[Trait("Tier", "P0")]`
- [ ] P0 tests link `UC` / `TC` / `AC` traits where known
- [ ] Pipeline job publishes `testRunTitle: 'Module: <Module>'`
- [ ] Dashboard `Module — <Module>` created with trend + failures widgets
- [ ] Overview dashboard lists all modules
- [ ] Module owner named on dashboard Markdown widget
- [ ] PR to `main` cannot merge if module test job fails (`failTaskOnFailedTests: true`)

---

## Folder template for SandBox / instruction repos

Copy into the **external** repo when ready:

```text
.azuredevops/
├── pipelines/
│   └── platform-module-tests.yml
├── dashboards/
│   ├── README.md                    # links to ADO dashboard URLs (fill after create)
│   └── module-widget-layout.md      # copy-paste widget config per module
└── scripts/
    └── TestCoverageReport/          # optional P0 coverage counter
```

Store dashboard URLs in `docs/modularization/test-dashboards.md` after creation (one link per module).

**NuGet vulnerability → Jira → AI agent:** see `nuget-vulnerability-jira-workflow.md` and `templates/azure-pipelines-nuget-vulnerability-audit.yml`.

---

## Relationship to quality framework

| Quality framework item | Dashboard support |
|------------------------|-------------------|
| G5 — tests green on merge | Pipeline `failTaskOnFailedTests` |
| Characterization before extract | `Type=Characterization` trend widget |
| IG2 — expert-validated ACs | `AC` trait on tests; Test Plan optional |
| Management metrics | Overview dashboard pass-rate charts |

See `ai-assisted-delivery-quality-framework.md`.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-18 | Initial ADO per-module dashboard guide |
