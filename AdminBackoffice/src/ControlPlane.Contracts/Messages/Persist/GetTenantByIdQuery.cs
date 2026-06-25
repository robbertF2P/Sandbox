namespace ControlPlane.Contracts.Messages.Persist;

public sealed record GetTenantByIdQuery(Guid TenantId) : IActorSystemMessage;
