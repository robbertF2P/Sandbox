using Platform.ControlPlane.Contracts;

namespace ControlPlane.Contracts.Messages.Provisioning;

public sealed record ProvisionTenantCommand(ProvisionTenantRequest Request) : IActorSystemMessage;
