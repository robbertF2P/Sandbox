using ControlPlane.Domain.Tenants;

namespace ControlPlane.Application.Ports;

public interface ITenantRepository
{
    Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken);

    Task<TenantRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task AddAsync(TenantRecord tenant, CancellationToken cancellationToken);
}
