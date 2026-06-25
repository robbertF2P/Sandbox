using ControlPlane.Contracts.Messages.Provisioning;
using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Interfaces;

public interface IControlPlaneActorFacade
{
    Task<ProvisionTenantResult> ProvisionTenantAsync(
        ProvisionTenantRequest request,
        CancellationToken cancellationToken = default);

    Task<SyncTenantResult> SyncTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
