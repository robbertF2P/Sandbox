namespace AiBasics.Core.Models;

public sealed record DocumentChunk(
    string Id,
    string SourcePath,
    string Content,
    ReadOnlyMemory<float> Embedding);
