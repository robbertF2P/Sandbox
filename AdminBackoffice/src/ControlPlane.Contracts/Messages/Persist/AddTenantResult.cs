using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Persist;

public sealed record AddTenantResult(bool Success, TenantRecord? Tenant, string? ErrorMessage) : IActorSystemMessage;
