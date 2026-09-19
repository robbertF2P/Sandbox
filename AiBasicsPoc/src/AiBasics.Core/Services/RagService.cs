using System.Text;
using AiBasics.Core.Models;

namespace AiBasics.Core.Services;

public sealed class RagService(SemanticSearchService searchService, OllamaChatClient chatClient)
{
    private readonly SemanticSearchService _searchService = searchService;
    private readonly OllamaChatClient _chatClient = chatClient;

    public async Task<RagAnswer> AskAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var sources = await _searchService.SearchAsync(query, topK, cancellationToken);
        if (sources.Count == 0)
        {
            return new RagAnswer(
                query,
                string.Empty,
                "I don't know. No relevant documentation was found.",
                sources);
        }

        var context = string.Join(
            "\n\n---\n\n",
            sources.Select(s => $"[{s.SourcePath}]\n{s.Content}"));

        var prompt = $"""
            You are a strict assistant for the SandBox monorepo.
            Answer ONLY using the provided context.
            If the answer is not in the context, respond with "I don't know."

            Context:
            {context}

            Question: {query}
            """;

        var answer = await _chatClient.GenerateAsync(prompt, cancellationToken);
        return new RagAnswer(query, context, answer, sources);
    }

    public async Task<string> FormatAnswerAsync(string query, CancellationToken cancellationToken = default)
    {
        var result = await AskAsync(query, cancellationToken: cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine($"Question: {result.Query}");
        builder.AppendLine();
        builder.AppendLine("Answer:");
        builder.AppendLine(result.Answer);
        builder.AppendLine();
        builder.AppendLine("Sources:");

        foreach (var source in result.Sources)
        {
            builder.AppendLine($"- {source.SourcePath} (score: {source.Score:F4})");
        }

        return builder.ToString().TrimEnd();
    }
}
