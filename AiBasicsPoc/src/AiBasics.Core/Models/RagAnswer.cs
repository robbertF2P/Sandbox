namespace AiBasics.Core.Models;

public sealed record RagAnswer(
    string Query,
    string Context,
    string Answer,
    IReadOnlyList<SearchResult> Sources);
