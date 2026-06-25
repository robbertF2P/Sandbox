namespace ControlPlane.Contracts.Messages.Platform;

public sealed record PushTenantConfigResult(bool Success, string? ErrorMessage) : IActorSystemMessage;
