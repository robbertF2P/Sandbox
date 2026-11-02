namespace AiBasics.Core.Models;

public sealed record SearchResult(
    string SourcePath,
    string Content,
    double Score);
