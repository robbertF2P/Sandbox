using System.Collections.Concurrent;
using System.Numerics.Tensors;
using AiBasics.Core.Models;

namespace AiBasics.Core.Storage;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, DocumentChunk> _chunks = new();

    public Task UpsertAsync(DocumentChunk chunk, CancellationToken cancellationToken = default)
    {
        _chunks[chunk.Id] = chunk;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SearchResult>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        var results = _chunks.Values
            .Select(chunk => new SearchResult(
                chunk.SourcePath,
                chunk.Content,
                TensorPrimitives.CosineSimilarity(
                    chunk.Embedding.Span,
                    queryEmbedding.Span)))
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .ToList();

        return Task.FromResult<IReadOnlyList<SearchResult>>(results);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_chunks.Count);

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _chunks.Clear();
        return Task.CompletedTask;
    }
}
