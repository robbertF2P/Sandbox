namespace ControlPlane.Contracts.Messages.Provisioning;

public sealed record SyncTenantCommand(Guid TenantId) : IActorSystemMessage;
