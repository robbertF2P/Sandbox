namespace ControlPlane.Domain.Tenants;

public sealed record LegacyHostedRuntimeProfile(
    string BuildProfileId,
    string RuntimeUrl,
    string DatabaseConnectionRef);
