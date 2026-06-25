namespace Platform.ControlPlane.Contracts;

public sealed record NativeRuntimeProfile(
    string DatabaseConnectionRef,
    string ApiBaseUrl);
