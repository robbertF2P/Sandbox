namespace ApiImportActorPoc.Contracts.Platform;

public sealed record NativeRuntimeProfile(
    string DatabaseConnectionRef,
    string ApiBaseUrl);
