using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace F2pPlatform.McpGateway.Services;

public sealed class F2pPlatformApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly HttpClient _httpClient;
    private readonly F2pApiOptions _options;

    public F2pPlatformApiClient(HttpClient httpClient, IOptions<F2pApiOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        }
    }

    public async Task<string> GetJsonAsync(string relativePath, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, relativePath);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"F2P API GET {relativePath} failed with {(int)response.StatusCode}: {body}");
        }

        return FormatJson(body);
    }

    public async Task<string> PostJsonAsync(string relativePath, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, relativePath);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"F2P API POST {relativePath} failed with {(int)response.StatusCode}: {body}");
        }

        return FormatJson(body);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativePath)
    {
        var request = new HttpRequestMessage(method, relativePath.TrimStart('/'));
        request.Headers.TryAddWithoutValidation("X-User-Name", _options.UserName);
        request.Headers.TryAddWithoutValidation("X-User-Permissions", _options.UserPermissions);
        return request;
    }

    private static string FormatJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "{}";
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            return JsonSerializer.Serialize(document.RootElement, JsonOptions);
        }
        catch (JsonException)
        {
            return body;
        }
    }
}
