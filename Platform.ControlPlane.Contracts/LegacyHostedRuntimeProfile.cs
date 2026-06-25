namespace Platform.ControlPlane.Contracts;

public sealed record LegacyHostedRuntimeProfile(
    string BuildProfileId,
    string RuntimeUrl,
    string DatabaseConnectionRef);
