using System.Collections.Concurrent;
using Platform.ControlPlane.Contracts;
using PlatformConfig.Application.Ports;

namespace PlatformConfig.Infrastructure;

/// <summary>
/// In-memory tenant registry for the v2 runtime. Production would add cache invalidation
/// or read-through from control-plane DB; backoffice push is the write path for now.
/// </summary>
public sealed class InMemoryTenantConfigurationStore : ITenantConfigurationStore
{
    private readonly ConcurrentDictionary<string, TenantConfigurationPayload> _bySlug = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, string> _slugByTenantId = new();

    public Task UpsertAsync(TenantConfigurationPayload configuration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var slug = configuration.Slug.Trim().ToLowerInvariant();
        _bySlug[slug] = configuration;
        _slugByTenantId[configuration.TenantId] = slug;
        return Task.CompletedTask;
    }

    public Task<TenantConfigurationPayload?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        _bySlug.TryGetValue(slug.Trim().ToLowerInvariant(), out var configuration);
        return Task.FromResult(configuration);
    }

    public Task<IReadOnlyList<TenantConfigurationPayload>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantConfigurationPayload> tenants = _bySlug.Values
            .OrderBy(t => t.Slug)
            .ToArray();
        return Task.FromResult(tenants);
    }
}
