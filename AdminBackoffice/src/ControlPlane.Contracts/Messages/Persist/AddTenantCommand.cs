using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Persist;

public sealed record AddTenantCommand(TenantRecord Tenant) : IActorSystemMessage;
