using Platform.ControlPlane.Contracts;

namespace ControlPlane.Application.Ports;

public interface ITenantProvisioningService
{
    Task<TenantRecord> ProvisionAsync(
        ProvisionTenantRequest request,
        CancellationToken cancellationToken);

    Task<TenantRecord> SyncToPlatformAsync(
        Guid tenantId,
        CancellationToken cancellationToken);
}
