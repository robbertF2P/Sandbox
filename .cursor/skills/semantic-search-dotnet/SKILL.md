---
name: semantic-search-dotnet
description: |
  Semantic search in .NET using Microsoft.Extensions.AI embeddings and cosine similarity.
  Use when:
  - Implementing meaning-based search (not keyword matching)
  - Choosing embedding providers (Ollama, Azure OpenAI, local models)
  - Ranking documents by vector similarity in memory or pgvector
  - Building the retrieval layer for RAG or MCP doc-search tools
paths:
  - "AiBasicsPoc/**"
  - "docs/ai-starter-kit.md"
metadata:
  version: 1.0.0
---

# Semantic search in .NET

**Reference POC:** `AiBasicsPoc/src/SemanticSearch.Console/`  
**Upstream inspiration:** [SemanticSearch-AI-Example](https://github.com/StefanTheCode/SemanticSearch-AI-Example) (StefanTheCode)

## Concept

Semantic search matches by **meaning**, not keywords. Text is converted to a numeric **embedding** (vector). Similar questions point in similar directions in vector space.

```
Corpus text → embed → vector store
User query  → embed → compare (cosine similarity) → top-K results
```

RAG and MCP doc tools build on this retrieval step.

## Stack (SandBox POC)

| Package | Role |
|---------|------|
| `Microsoft.Extensions.AI` | `IEmbeddingGenerator<string, Embedding<float>>` abstraction |
| `OllamaSharp` | Local Ollama implementation for dev |
| `System.Numerics.Tensors` | SIMD `TensorPrimitives.CosineSimilarity` |

Production swaps Ollama for Azure OpenAI / OpenAI via the same `IEmbeddingGenerator` registration.

## Core pattern

```csharp
// 1. Register generator (Ollama local dev)
IEmbeddingGenerator<string, Embedding<float>> generator =
    new OllamaApiClient(new Uri(ollamaUrl), embeddingModel);

// 2. Index corpus (batch)
var indexed = await generator.GenerateAndZipAsync(documents);

// 3. Query
var queryVector = await generator.GenerateAsync(userQuery);
var top = indexed
    .Select(d => new {
        d.Value,
        Score = TensorPrimitives.CosineSimilarity(
            d.Embedding.Vector.Span, queryVector.Vector.Span)
    })
    .OrderByDescending(x => x.Score)
    .Take(topK);
```

**Cosine similarity:** 1.0 = identical direction; 0 = orthogonal. Use for normalized embedding models.

## SandBox conventions

| Topic | Do |
|-------|-----|
| Config | `Ollama:Url`, `Ollama:EmbeddingModel` in appsettings + env `OLLAMA__URL` |
| DI | `AddAiBasicsCore(services, configuration)` in `AiBasics.Core` |
| Small corpora | In-memory scan (`InMemoryVectorStore`) — fine for docs/skills |
| Large corpora | pgvector, Azure AI Search, or dedicated vector DB |
| Tests | `DeterministicEmbeddingGenerator` — hash-based vectors, no Ollama |
| Logging | Prefer `Platform.Serilog.Logging` in non-POC modules |

## When to use what

| Corpus size | Store | Search |
|-------------|-------|--------|
| < ~5k chunks | In-memory list + cosine | LINQ + `TensorPrimitives` |
| Shared / persistent | PostgreSQL + pgvector | `<->` or `<=>` operators |
| Managed cloud | Azure AI Search | hybrid semantic + keyword |

## Common mistakes

- Jumping to agents before retrieval works — validate search quality first.
- Using different embedding models for index vs query — dimensions must match.
- Storing raw `float[]` without normalization strategy — document model choice.
- Keyword-only search when users ask natural-language questions.

## Related skills

- `rag-dotnet` — retrieval + LLM generation
- `mcp-server-dotnet` — expose search/ask as MCP tools for Cursor/Copilot
- `sandbox-starter-kit` — repo map and POC locations

## Run the POC

```bash
# Requires Ollama: ollama pull nomic-embed-text
dotnet run --project AiBasicsPoc/src/SemanticSearch.Console
```
