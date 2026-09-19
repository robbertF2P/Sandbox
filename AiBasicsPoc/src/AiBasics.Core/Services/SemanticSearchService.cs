using System.Text;
using AiBasics.Core.Models;
using AiBasics.Core.Storage;
using Microsoft.Extensions.AI;

namespace AiBasics.Core.Services;

public sealed class SemanticSearchService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStore vectorStore)
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
    private readonly IVectorStore _vectorStore = vectorStore;

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await _embeddingGenerator.GenerateAsync(query, cancellationToken: cancellationToken);
        return await _vectorStore.SearchAsync(queryEmbedding.Vector, topK, cancellationToken);
    }

    public async Task<string> FormatResultsAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var results = await SearchAsync(query, topK, cancellationToken);
        if (results.Count == 0)
        {
            return "No matching documents found. Run ingest/corpus first.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Query: {query}");
        builder.AppendLine();

        foreach (var result in results)
        {
            builder.AppendLine($"## {result.SourcePath} (score: {result.Score:F4})");
            builder.AppendLine(result.Content);
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }
}
