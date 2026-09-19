---
name: rag-dotnet
description: |
  Retrieval-Augmented Generation (RAG) in .NET: ingest documents, retrieve by embedding similarity,
  generate grounded answers with an LLM. Use when:
  - Connecting AI to your own docs, APIs, or domain data
  - Designing ingest + ask endpoints or services
  - Choosing in-memory vs pgvector storage
  - Writing strict context-only prompts to reduce hallucination
paths:
  - "AiBasicsPoc/**"
  - "docs/**"
metadata:
  version: 1.0.0
---

# RAG in .NET

**Reference POC:** `AiBasicsPoc/src/RagBasics.Api/`  
**Upstream inspiration:** [RAG_System_Basics](https://github.com/StefanTheCode/RAG_System_Basics) (StefanTheCode)

## Concept

RAG = **Retrieve** relevant chunks, then **Augment** the LLM prompt with that context, then **Generate** an answer.

Without retrieval, the model only knows its training data. With RAG, answers cite *your* docs.

```
POST /ingest  → chunk → embed → store
GET  /ask     → embed query → top-K chunks → prompt + LLM → answer
```

## Two-phase pipeline

### Phase 1 — Retrieval

1. Embed the user query (`IEmbeddingGenerator`).
2. Find top-K similar chunks (`InMemoryVectorStore` or pgvector).
3. Join chunks into a single context block (separate with `---`).

### Phase 2 — Generation

1. Build a **strict** system/user prompt: answer only from context.
2. Call chat/completion API (`IChatClient` or Ollama `/api/generate`).
3. Return `{ context, answer }` so callers can audit grounding.

```csharp
var prompt = $"""
    Answer ONLY using the context below. If unknown, say "I don't know."

    Context:
    {combinedContext}

    Question: {query}
    """;
```

## SandBox POC architecture

| Type | Responsibility |
|------|----------------|
| `IVectorStore` | `UpsertAsync`, `SearchAsync` — swappable in-memory / pgvector |
| `InMemoryVectorStore` | Cosine similarity; no external DB for local dev |
| `DocumentCorpus` | Load markdown from `docs/` and `.cursor/skills/` |
| `RagService` | Orchestrates retrieve → generate |
| `AddRagBasicsApi` | `IServiceCollection` extension |

## API shape (minimal)

| Endpoint | Purpose |
|----------|---------|
| `POST /ingest` | `{ "content": "..." }` — embed and store |
| `POST /ingest/corpus` | Re-index bundled SandBox docs |
| `GET /ask?query=` | Full RAG response |

## Configuration

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

**Never commit** database connection strings. Use user secrets or env for pgvector.

## pgvector (production path)

From Stefan's repo — adapt for SandBox:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
CREATE TABLE text_contexts (
  id serial PRIMARY KEY,
  content text NOT NULL,
  embedding vector(768) NOT NULL
);
-- ORDER BY embedding <=> query_vector LIMIT 5  (cosine distance)
```

Fix from upstream: filter with **distance threshold** using `<` (closer = more similar), not `>`.

| Operator | Meaning |
|----------|---------|
| `<->` | L2 distance |
| `<=>` | Cosine distance |

Prefer `Microsoft.Extensions.AI` over raw Ollama HTTP for provider swap.

## Quality practices

- **Chunking:** split long docs (~500–1000 tokens) with overlap for better recall.
- **Metadata:** store source path + heading for citations in MCP/UI.
- **Eval:** keep a small set of question → expected-doc pairs; run in tests.
- **Correlation:** add `UseCase` / `CorrelationId` when RAG is behind HTTP (see `platform-correlation`).

## Common mistakes

- Skipping semantic search fundamentals — see `semantic-search-dotnet`.
- Letting the LLM answer without context constraints.
- Same model for embed + chat without checking Ollama supports both.
- No "I don't know" escape hatch — causes confident hallucinations.

## Related skills

- `semantic-search-dotnet` — embedding + similarity (phase 1 only)
- `mcp-server-dotnet` — expose `ask_docs` as an MCP tool
- `dotnet-core-csharp-development` — minimal APIs, DI, tests

## Run the POC

```bash
ollama pull nomic-embed-text && ollama pull llama3.2
dotnet run --project AiBasicsPoc/src/RagBasics.Api
curl -X POST http://localhost:5090/ingest/corpus
curl "http://localhost:5090/ask?query=How%20do%20I%20add%20logging?"
```
