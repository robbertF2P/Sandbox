using Platform.ControlPlane.Contracts;
using PlatformConfig.Application.Ports;

namespace PlatformConfig.Application.Services;

public interface ITenantConfigurationService
{
    Task<TenantConfigurationPayload> RegisterAsync(
        TenantConfigurationPayload configuration,
        CancellationToken cancellationToken);

    Task<TenantConfigurationPayload?> ResolveBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantConfigurationPayload>> ListAsync(CancellationToken cancellationToken);
}

public sealed class TenantConfigurationService(ITenantConfigurationStore store) : ITenantConfigurationService
{
    public async Task<TenantConfigurationPayload> RegisterAsync(
        TenantConfigurationPayload configuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(configuration.Slug))
        {
            throw new ArgumentException("Slug is required.", nameof(configuration));
        }

        await store.UpsertAsync(configuration, cancellationToken);
        return configuration;
    }

    public Task<TenantConfigurationPayload?> ResolveBySlugAsync(string slug, CancellationToken cancellationToken) =>
        store.GetBySlugAsync(slug.Trim().ToLowerInvariant(), cancellationToken);

    public Task<IReadOnlyList<TenantConfigurationPayload>> ListAsync(CancellationToken cancellationToken) =>
        store.ListAsync(cancellationToken);
}
