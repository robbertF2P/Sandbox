using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Provisioning;

public sealed record SyncTenantResult(
    bool Success,
    TenantRecord? Tenant,
    string? ErrorMessage) : IActorSystemMessage;
