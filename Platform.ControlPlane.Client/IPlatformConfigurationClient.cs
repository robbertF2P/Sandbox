using Platform.ControlPlane.Contracts;

namespace Platform.ControlPlane.Client;

public interface IPlatformConfigurationClient
{
    Task PushTenantConfigurationAsync(
        TenantConfigurationPayload configuration,
        CancellationToken cancellationToken);
}
