using AiBasics.Core.Models;
using Xunit;
using AiBasics.Core.Storage;

namespace AiBasics.Tests;

public sealed class InMemoryVectorStoreTests
{
    [Fact]
    public async Task Search_returns_highest_similarity_first()
    {
        var store = new InMemoryVectorStore();

        await store.UpsertAsync(new DocumentChunk("a", "a.md", "alpha", new ReadOnlyMemory<float>([1f, 0f, 0f])));
        await store.UpsertAsync(new DocumentChunk("b", "b.md", "beta", new ReadOnlyMemory<float>([0f, 1f, 0f])));
        await store.UpsertAsync(new DocumentChunk("c", "c.md", "gamma", new ReadOnlyMemory<float>([0.9f, 0.1f, 0f])));

        var results = await store.SearchAsync(new ReadOnlyMemory<float>([1f, 0f, 0f]), topK: 2);

        Assert.Equal(2, results.Count);
        Assert.Equal("a.md", results[0].SourcePath);
        Assert.Equal("c.md", results[1].SourcePath);
        Assert.True(results[0].Score >= results[1].Score);
    }
}
