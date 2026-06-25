using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Persist;

public sealed record UpdateTenantCommand(TenantRecord Tenant) : IActorSystemMessage;
