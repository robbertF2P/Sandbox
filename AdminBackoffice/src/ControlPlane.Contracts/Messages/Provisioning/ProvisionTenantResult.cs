using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Provisioning;

public enum ProvisionErrorKind
{
    Validation,
    Conflict,
    PlatformSync
}

public sealed record ProvisionTenantResult(
    bool Success,
    TenantRecord? Tenant,
    string? ErrorMessage,
    ProvisionErrorKind? ErrorKind) : IActorSystemMessage;
