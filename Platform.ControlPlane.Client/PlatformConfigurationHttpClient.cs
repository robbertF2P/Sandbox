using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.ControlPlane.Contracts;

namespace Platform.ControlPlane.Client;

public sealed class PlatformConfigurationHttpClient(
    HttpClient httpClient,
    IOptions<PlatformConfigurationOptions> options,
    ILogger<PlatformConfigurationHttpClient> logger) : IPlatformConfigurationClient
{
    public async Task PushTenantConfigurationAsync(
        TenantConfigurationPayload configuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var requestUri = new Uri(new Uri(options.Value.BaseUrl.TrimEnd('/')), "/api/v1/platform/tenant-config");
        using var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(configuration),
        };
        request.Headers.Add("X-Platform-Config-Key", options.Value.ConfigurationApiKey);

        logger.LogInformation(
            "Pushing tenant configuration for {TenantId} ({Slug}) to {Url}",
            configuration.TenantId,
            configuration.Slug,
            requestUri);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Platform configuration API returned {(int)response.StatusCode}: {body}");
        }
    }
}
