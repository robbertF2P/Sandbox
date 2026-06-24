using Platform.ControlPlane.Contracts;

namespace PlatformConfig.Application.Ports;

public interface ITenantConfigurationStore
{
    Task UpsertAsync(TenantConfigurationPayload configuration, CancellationToken cancellationToken);

    Task<TenantConfigurationPayload?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantConfigurationPayload>> ListAsync(CancellationToken cancellationToken);
}
