namespace ControlPlane.Domain.Tenants;

public sealed record NativeRuntimeProfile(
    string DatabaseConnectionRef,
    string ApiBaseUrl);
