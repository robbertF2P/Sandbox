using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Platform;

public sealed record PushTenantConfigCommand(TenantRecord Tenant) : IActorSystemMessage;
