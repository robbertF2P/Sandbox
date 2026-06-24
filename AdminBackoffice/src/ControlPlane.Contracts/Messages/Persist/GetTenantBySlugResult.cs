using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Persist;

public sealed record GetTenantBySlugResult(TenantRecord? Tenant) : IActorSystemMessage;
