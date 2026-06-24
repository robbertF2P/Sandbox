namespace ControlPlane.Contracts.Messages.Persist;

public sealed record GetTenantBySlugQuery(string Slug) : IActorSystemMessage;
