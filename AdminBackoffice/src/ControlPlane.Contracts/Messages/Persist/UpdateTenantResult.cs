using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Persist;

public sealed record UpdateTenantResult(bool Success, TenantRecord? Tenant, string? ErrorMessage) : IActorSystemMessage;
