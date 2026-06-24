using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Persist;

public sealed record GetTenantByIdResult(TenantRecord? Tenant) : IActorSystemMessage;
