using System.Collections.Concurrent;
using ControlPlane.Application.Ports;
using ControlPlane.Domain.Tenants;

namespace ControlPlane.Infrastructure.Repositories;

public sealed class InMemoryTenantRepository : ITenantRepository
{
    private readonly ConcurrentDictionary<string, TenantRecord> _tenants = new(StringComparer.Ordinal);

    public Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<TenantRecord> tenants = _tenants.Values
            .OrderBy(tenant => tenant.CreatedAtUtc)
            .ToList();
        return Task.FromResult(tenants);
    }

    public Task<TenantRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _tenants.TryGetValue(slug, out TenantRecord? tenant);
        return Task.FromResult(tenant);
    }

    public Task AddAsync(TenantRecord tenant, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(tenant);

        if (!_tenants.TryAdd(tenant.Slug, tenant))
        {
            throw new InvalidOperationException($"Tenant slug '{tenant.Slug}' already exists.");
        }

        return Task.CompletedTask;
    }
}
