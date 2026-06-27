using Platform.ControlPlane.Contracts;
using PlatformConfig.Application.Ports;

namespace PlatformConfig.Infrastructure.Runtime;

public sealed class TenantRuntimeContext : ITenantRuntimeContext
{
    private TenantConfigurationPayload? _current;

    public TenantConfigurationPayload? Current => _current;

    public void SetCurrent(TenantConfigurationPayload configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _current = configuration;
    }
}
