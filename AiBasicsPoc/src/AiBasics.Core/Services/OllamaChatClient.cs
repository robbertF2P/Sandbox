using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AiBasics.Core.Options;
using Microsoft.Extensions.Options;

namespace AiBasics.Core.Services;

public sealed class OllamaChatClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaChatClient(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.Url);
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = new OllamaGenerateRequest(_options.ChatModel, prompt, Stream: false);
        using var response = await _httpClient.PostAsJsonAsync("/api/generate", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return "Error: unable to generate a response from Ollama.";
        }

        var body = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
        return body?.Response ?? "I don't know.";
    }

    private sealed record OllamaGenerateRequest(string Model, string Prompt, bool Stream);

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; init; }
    }
}
