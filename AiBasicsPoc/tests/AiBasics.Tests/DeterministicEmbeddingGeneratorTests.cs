using AiBasics.Core.Embeddings;
using AiBasics.Core.Models;
using AiBasics.Core.Storage;
using Microsoft.Extensions.AI;
using Xunit;

namespace AiBasics.Tests;

public sealed class DeterministicEmbeddingGeneratorTests
{
    [Fact]
    public async Task Same_text_produces_identical_embeddings()
    {
        var generator = new DeterministicEmbeddingGenerator();
        var first = await GenerateAsync(generator, "platform logging serilog");
        var second = await GenerateAsync(generator, "platform logging serilog");

        Assert.Equal(first.ToArray(), second.ToArray());
    }

    [Fact]
    public async Task Vector_store_ranks_exact_content_match_highest()
    {
        var generator = new DeterministicEmbeddingGenerator();
        var store = new InMemoryVectorStore();
        const string content = "Central Serilog logging for SandBox modules.";

        var embedding = await GenerateAsync(generator, content);
        await store.UpsertAsync(new DocumentChunk("logging", "platform-logging.md", content, embedding));
        await store.UpsertAsync(new DocumentChunk(
            "akka",
            "akka-net.md",
            "Akka.NET actors process messages asynchronously.",
            await GenerateAsync(generator, "Akka.NET actors process messages asynchronously.")));

        var queryEmbedding = await GenerateAsync(generator, content);
        var results = await store.SearchAsync(queryEmbedding, topK: 1);

        Assert.Single(results);
        Assert.Equal("platform-logging.md", results[0].SourcePath);
        Assert.Equal(1d, results[0].Score, precision: 5);
    }

    private static async Task<ReadOnlyMemory<float>> GenerateAsync(
        IEmbeddingGenerator<string, Embedding<float>> generator,
        string text)
    {
        var embeddings = await generator.GenerateAsync([text]);
        return embeddings[0].Vector;
    }
}
