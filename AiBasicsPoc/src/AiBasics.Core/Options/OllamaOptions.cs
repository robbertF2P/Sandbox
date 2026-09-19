namespace AiBasics.Core.Options;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string Url { get; set; } = "http://127.0.0.1:11434";

    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    public string ChatModel { get; set; } = "llama3.2";
}
