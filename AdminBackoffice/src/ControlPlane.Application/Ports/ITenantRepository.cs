using Platform.ControlPlane.Contracts;

namespace ControlPlane.Application.Ports;

public interface ITenantRepository
{
    Task<TenantRecord?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<TenantRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken);

    Task<TenantRecord> AddAsync(TenantRecord tenant, CancellationToken cancellationToken);

    Task<TenantRecord> UpdateAsync(TenantRecord tenant, CancellationToken cancellationToken);
}
