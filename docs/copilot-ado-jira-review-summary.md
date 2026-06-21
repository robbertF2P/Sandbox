# GitHub Copilot + Azure DevOps + Jira Cloud — Team Summary

**Prepared for:** robbert@floorganise.com  
**Date:** June 9, 2026

---

## 1. GitHub Copilot for team code reviews

**Recommended workflow:** Copilot as a fast first reviewer; humans as the final gate.

| Stage | Who | Action |
|-------|-----|--------|
| 1 | Author | Open PR; fix Copilot findings before requesting humans |
| 2 | Copilot | Automated/on-demand review for bugs, security, standards |
| 3 | Human | Architecture, business logic, trade-offs |
| 4 | CI | Build, test, lint as required checks |

**Encode team standards** in instruction files (on GitHub): `.github/copilot-instructions.md` and path-specific `.github/instructions/*.instructions.md`.

**Review effort levels (GitHub):** Low for routine changes; Medium for security-sensitive or cross-service work.

---

## 2. Azure DevOps — automatic vs on-demand Copilot review

You have requested **Azure DevOps on-demand Copilot Code Review** (technical preview).

| Approach | Automatic on every PR? | Status |
|----------|------------------------|--------|
| **ADO on-demand Copilot review** | On-demand — user requests from PR UI | Preview (waitlist) |
| **Pipeline + branch policy** | Yes — runs on PR create/update | Available now |
| **GitHub.com auto-review** | Yes (org/repo settings) | Only if repos are on GitHub |

**On-demand preview (what you requested):**
- Inline comments in the Azure DevOps PR UI
- No per-developer Copilot license required
- Billed via GitHub AI credits on Azure subscription
- Does **not** block merge or count as required reviewer
- Billed from June 2, 2026 (preview pricing)

**For fully automatic reviews on ADO today:** Use Azure Pipelines + **Build validation** branch policy + a marketplace extension or custom agent (e.g. Copilot Code Review extension, Microsoft ADO Pull Request Agent).

---

## 3. Jira Cloud + MCP

**Official:** Atlassian Rovo MCP Server (GA early 2026) — for Jira Cloud, Confluence, Compass.

- **Endpoint:** `https://mcp.atlassian.com/v1/mcp/authv2`
- **Auth:** OAuth 2.1 (org admin can optionally allow API tokens)
- **Setup (Cursor / VS Code):**

```json
{
  "mcpServers": {
    "Atlassian-Rovo-MCP": {
      "command": "npx",
      "args": ["-y", "mcp-remote@latest", "https://mcp.atlassian.com/v1/mcp/authv2"]
    }
  }
}
```

**Admin checklist:** Enable Rovo MCP; configure allowed AI client domains; update IP allowlists if used.

---

## 4. Jira MCP + ADO on-demand Copilot review — do they connect?

**Short answer:** Jira MCP is still useful, but it does **not** plug into ADO on-demand Copilot review today.

| Layer | Jira context? | Role |
|-------|---------------|------|
| ADO on-demand Copilot review | Only what's **in the PR** (title, description, diff) | Code-quality pass |
| Jira MCP (Rovo) | **Live** issue data | IDE/agent workflows |
| Azure DevOps for Jira (Marketplace) | Links PR ↔ issue in Jira UI | Traceability |

**Bridge them:** Put Jira key + acceptance criteria in the PR description (use a PR template). On-demand Copilot reads that text; Jira MCP helps authors gather it before opening the PR.

**Pipeline-based auto-review** can wire ADO MCP + Jira MCP together (custom agents) — that's where MCP integration is real for ADO today.

---

## 5. Creating PRs from a laptop with Copilot

**Yes** — via **Copilot Agent Mode + Azure DevOps MCP** (VS Code or Cursor).

**Flow:**
1. Dev commits and pushes branch
2. Copilot Agent Mode + ADO MCP configured
3. Prompt: e.g. "Create PR from `feature/PROJ-123-x` to `main` with Jira context"
4. Dev approves MCP tool calls → PR created in ADO
5. Optionally request on-demand Copilot review in PR UI

**Requirements:** Node.js 20+, Agent Mode, ADO MCP (`@azure-devops/mcp`), Azure auth, PR permissions.

**Optional:** Add Jira Rovo MCP alongside ADO MCP to fetch ticket title/AC when creating the PR.

**Tips:** Push branch before creating PR; use repo GUID if PR creation fails; branch names like `feature/PROJ-123-desc`.

---

## 6. Recommended team setup

**Do now:**
1. PR template with Jira key + acceptance criteria
2. Branch naming with issue keys
3. Azure DevOps for Jira app (optional — visibility in Jira)
4. Jira MCP for developers in VS Code/Cursor
5. ADO MCP for PR creation from laptop

**When ADO preview access lands:**
1. Request Copilot review when PR is ready (draft → open)
2. Authors fix Copilot findings before human review
3. Keep Jira context in PR description

**Consider later:**
- Pipeline auto-review with MCP-enabled agents
- Watch for Microsoft adding repo-level MCP to ADO Copilot review (GitHub already has this)

---

## 7. End-to-end workflow

| Step | Who | Tool |
|------|-----|------|
| Pick up ticket | Dev | Jira |
| Implement + check AC | Dev | Jira MCP in IDE |
| Commit + push | Dev | Git |
| Create PR | Dev | Copilot + ADO MCP (optional) |
| Request Copilot review | Dev | ADO on-demand (preview) |
| Fix code issues | Dev | ADO PR comments |
| Approve design + ticket fit | Human | Jira + PR |
| Merge | Human | Azure DevOps |

---

## 8. Slack

Slack fits the stack in three ways: notifications, Copilot agent triggers, and MCP for AI context.

### Native Slack apps (recommended for team visibility)

| App | Purpose |
|-----|---------|
| **Azure Repos for Slack** | PR created/updated/merged, push events, PR link previews |
| **Azure Pipelines for Slack** | Build pass/fail, deployment approvals |
| **Azure Boards for Slack** | Work item updates |
| **Jira Cloud for Slack** | Issue updates, link unfurling |

Setup: install apps → `/azrepos signin` → `/azrepos subscribe [repo URL]` → tune with `/azrepos subscriptions`.

### GitHub Copilot + Slack

The GitHub for Slack app can trigger **Copilot coding agent** from threads and draft GitHub issues. This is mainly for **GitHub repos**. With Azure Repos, Slack is for **notifications**; ADO PR creation stays in the IDE via ADO MCP.

Copilot also integrates with **Jira** and **Azure Boards** to trigger the coding agent from work items (GitHub-centric workflow).

### Slack MCP server

Official hosted endpoint: `https://mcp.slack.com/mcp`

- Search messages, channels, files; read threads; send messages
- Works in Cursor, Claude, Perplexity (admin approval required)
- Does **not** connect to ADO on-demand Copilot review (IDE context only)

### Slack + on-demand Copilot review workflow

1. Dev opens PR → Slack notifies channel (Azure Repos app)
2. Dev requests Copilot review in ADO
3. Dev posts “ready for human review” in Slack thread
4. Human reviewer uses Jira + ADO

---

## Key links

- [GitHub Copilot code review](https://docs.github.com/en/copilot/concepts/agents/code-review)
- [Customize Copilot code review](https://docs.github.com/en/copilot/tutorials/customize-code-review)
- [ADO Copilot review preview changelog](https://github.blog/changelog/2026-06-02-github-copilot-code-review-for-azure-repos-is-now-in-technical-preview/)
- [Azure DevOps AI blog](https://devblogs.microsoft.com/devops/azure-devops-and-github-journeying-into-the-ai-era/)
- [Azure DevOps MCP](https://learn.microsoft.com/en-us/azure/devops/mcp-server/mcp-server-overview)
- [Atlassian Rovo MCP](https://support.atlassian.com/atlassian-rovo-mcp-server/docs/getting-started-with-the-atlassian-remote-mcp-server/)
- [ADO Pull Request Agent (sample)](https://github.com/dstamand-msft/AzureDevOpsPullRequestAgent)
