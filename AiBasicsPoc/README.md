# AI Basics POC

Practical .NET examples for **semantic search**, **RAG**, and **MCP servers** — extracted from [StefanTheCode](https://github.com/StefanTheCode) learning repos and adapted for SandBox conventions.

| Step | Project | What it demonstrates |
|------|---------|---------------------|
| 1 | `src/SemanticSearch.Console` | Embeddings + cosine similarity over repo docs |
| 2 | `src/RagBasics.Api` | Ingest + ask API (retrieve → generate) |
| 3 | `src/DocsSearch.Mcp` | stdio MCP tools for Cursor/Copilot |

Shared logic lives in `src/AiBasics.Core/`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Ollama](https://ollama.com/) running locally

```bash
ollama pull nomic-embed-text
ollama pull llama3.2
```

## Quick start

```bash
# Semantic search (console REPL)
dotnet run --project src/SemanticSearch.Console

# RAG API
dotnet run --project src/RagBasics.Api
curl -X POST http://localhost:5090/ingest/corpus
curl "http://localhost:5090/ask?query=How%20do%20agent%20skills%20work?"

# MCP server (stdio — configure in Cursor MCP settings)
dotnet run --project src/DocsSearch.Mcp
```

### MCP tools

| Tool | Description |
|------|-------------|
| `index_docs` | Re-index `docs/` and `.cursor/skills/` |
| `search_docs` | Semantic search (no LLM) |
| `ask_docs` | RAG — grounded answer from docs |

## Configuration

`appsettings.json` in each host:

```json
{
  "Ollama": {
    "Url": "http://127.0.0.1:11434",
    "EmbeddingModel": "nomic-embed-text",
    "ChatModel": "llama3.2"
  }
}
```

Env overrides: `OLLAMA__URL`, `OLLAMA__EMBEDDINGMODEL`, `OLLAMA__CHATMODEL`.

## Tests (no Ollama required)

```bash
dotnet test
```

Uses `DeterministicEmbeddingGenerator` for hash-based vectors.

## Agent skills

| Skill | Path |
|-------|------|
| Semantic search | `.cursor/skills/semantic-search-dotnet/` |
| RAG | `.cursor/skills/rag-dotnet/` |
| MCP server | `.cursor/skills/mcp-server-dotnet/` |

## Related SandBox references

- `F2pPlatform/tools/F2pPlatform.McpGateway/` — MCP over Hour Approvals REST API
- [SemanticSearch-AI-Example](https://github.com/StefanTheCode/SemanticSearch-AI-Example)
- [RAG_System_Basics](https://github.com/StefanTheCode/RAG_System_Basics)
- [MCP-Server-in-.NET-for-API-Performance-Analysis](https://github.com/StefanTheCode/MCP-Server-in-.NET-for-API-Performance-Analysis)

## Production path

| POC pattern | Production upgrade |
|-------------|-------------------|
| In-memory vector store | PostgreSQL pgvector or Azure AI Search |
| Ollama local | Azure OpenAI / managed embeddings |
| stdio MCP | HTTP MCP (`ModelContextProtocol.AspNetCore`) + auth |
| Console logging | `Platform.Serilog.Logging` + correlation |
