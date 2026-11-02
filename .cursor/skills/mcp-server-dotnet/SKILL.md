---
name: mcp-server-dotnet
description: |
  Build MCP (Model Context Protocol) servers in .NET for Cursor, GitHub Copilot, and Claude.
  Use when:
  - Exposing domain APIs or diagnostics as AI-callable tools
  - Choosing stdio vs HTTP transport for MCP
  - Structuring tool classes with ModelContextProtocol attributes
  - Keeping business logic out of tool methods (thin MCP layer)
paths:
  - "AiBasicsPoc/src/DocsSearch.Mcp/**"
  - "F2pPlatform/tools/F2pPlatform.McpGateway/**"
metadata:
  version: 1.0.0
---

# MCP server in .NET

**Reference POCs:**

| POC | Transport | Pattern |
|-----|-----------|---------|
| `AiBasicsPoc/src/DocsSearch.Mcp/` | stdio | Doc search + RAG tools |
| `F2pPlatform/tools/F2pPlatform.McpGateway/` | stdio | REST API gateway tools |

**Upstream inspiration:** [MCP-Server-in-.NET-for-API-Performance-Analysis](https://github.com/StefanTheCode/MCP-Server-in-.NET-for-API-Performance-Analysis) (HTTP transport + load-test tools)

## Concept

MCP lets AI clients discover and invoke **tools** you define. The server does not replace your API — it is a **thin adapter** for agents.

```
Cursor / Copilot / Claude
        │  MCP (stdio or HTTP)
        ▼
   McpServer host
        │  injects services
        ▼
   [McpServerTool] methods  →  domain services / HTTP clients
```

## Package

```xml
<PackageReference Include="ModelContextProtocol" />
<!-- HTTP transport only -->
<PackageReference Include="ModelContextProtocol.AspNetCore" />
```

SandBox `F2pPlatform` centralizes version in `Directory.Packages.props` (`ModelContextProtocol` 2.x).

## Tool class pattern

```csharp
[McpServerToolType]
public sealed class DocsSearchMcpTools(RagService rag, SemanticSearchService search)
{
    [McpServerTool]
    [Description("Semantic search over SandBox docs and agent skills.")]
    public async Task<string> SearchDocs(
        [Description("Natural language search query.")]
        string query,
        CancellationToken cancellationToken) =>
        await search.FormatResultsAsync(query, topK: 5, cancellationToken);

    [McpServerTool]
    [Description("Ask a question grounded in SandBox documentation (RAG).")]
    public async Task<string> AskDocs(string question, CancellationToken cancellationToken) =>
        await rag.FormatAnswerAsync(question, cancellationToken);
}
```

**Rules:**

- `[Description]` on class methods and parameters — agents use these to pick tools.
- Return `string` or JSON string for agent-readable output.
- Keep logic in injectable services; tools orchestrate only.
- Use `CancellationToken` on async tools.

## Host registration

### Stdio (Cursor, VS Code Copilot local)

Matches `F2pPlatform.McpGateway` and `DocsSearch.Mcp`:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddAiBasicsCore(builder.Configuration);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(DocsSearchMcpTools).Assembly);
await builder.Build().RunAsync();
```

Log to **stderr** in stdio mode so stdout stays clean for MCP protocol:

```csharp
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
```

### HTTP (Copilot remote / Performance Lab style)

```csharp
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();
app.MapMcp("/mcp");
```

Example `mcp.json` (HTTP):

```json
{
  "servers": {
    "docs-search": { "type": "http", "url": "http://localhost:5091/mcp" }
  }
}
```

## F2pPlatform gateway pattern (production-adjacent)

`F2pPlatform.McpGateway` — stdio MCP that calls existing REST APIs:

| Tool | Backing API |
|------|-------------|
| `get_hour_approvals_capabilities` | `GET /api/hour-approvals/capabilities` |
| `list_hour_approval_tasks` | `GET /api/hour-approvals/tasks` |
| `approve_hour_approval_task` | `POST /api/hour-approvals/tasks/{id}/approve` |

Env: `F2P_API_BASE_URL`, `F2P_USER_NAME`, `F2P_USER_PERMISSIONS`.

**No duplicate business logic** — MCP is a transport adapter.

## Performance Lab patterns (optional)

Stefan's repo adds **deterministic analysis** behind MCP tools (not LLM-in-the-server):

- `LoadTestRunner` — concurrent `HttpClient` workers, percentiles
- `ResultAnalyzer` — rule-based ThreadPool / GC / latency diagnosis
- `IResultStore` — swappable persistence

Use when exposing ops/diagnostics tools. MCP tools return formatted markdown reports.

## SandBox checklist

| Item | Guidance |
|------|----------|
| Transport | stdio for local agents; HTTP when dashboard or remote clients need it |
| Auth | POC uses headers; production follows `platform-authentication-standard` |
| Secrets | env vars only — never hardcode in `mcp.json` committed to repo |
| Tests | Unit-test services; optional integration test with `McpClient` |
| Logging | `Platform.Serilog.Logging` outside reference POCs |

## Common mistakes

- Putting business rules inside `[McpServerTool]` methods — hard to test.
- Logging to stdout in stdio MCP — breaks protocol.
- Too many coarse tools — prefer focused tools with good descriptions.
- Building agents before MCP tools work — validate tools in Cursor Agent mode first.

## Related skills

- `semantic-search-dotnet` — retrieval for `search_docs`
- `rag-dotnet` — grounded answers for `ask_docs`
- `sandbox-starter-kit` — repo map

## Run DocsSearch MCP

```bash
ollama pull nomic-embed-text && ollama pull llama3.2
dotnet run --project AiBasicsPoc/src/DocsSearch.Mcp
# Configure in Cursor MCP settings as stdio command pointing to the built DLL
```

## Run F2pPlatform MCP gateway

```bash
dotnet run --project F2pPlatform/host/F2pPlatform.Host   # :5080
dotnet run --project F2pPlatform/tools/F2pPlatform.McpGateway
```
