using AiBasics.Core.Models;

namespace AiBasics.Core.Storage;

public interface IVectorStore
{
    Task UpsertAsync(DocumentChunk chunk, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchResult>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
