using Platform.ControlPlane.Contracts;

namespace ControlPlane.Application.Ports;

public interface IPlatformConfigurationClient
{
    Task PushTenantConfigurationAsync(
        TenantConfigurationPayload configuration,
        CancellationToken cancellationToken);
}
