using Platform.ControlPlane.Contracts;

namespace PlatformConfig.Application.Ports;

/// <summary>
/// Active tenant on this runtime instance (single-tenant POC; production resolves by host/slug).
/// </summary>
public interface ITenantRuntimeContext
{
    TenantConfigurationPayload? Current { get; }

    void SetCurrent(TenantConfigurationPayload configuration);
}
