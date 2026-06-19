namespace ApiImportActorPoc.Contracts.Platform;

/// <summary>
/// Where and how the full legacy product runs for this tenant.
/// Secrets are references only — never store connection strings in the profile document.
/// </summary>
public sealed record LegacyHostedRuntimeProfile(
    string BuildProfileId,
    string RuntimeUrl,
    string DatabaseConnectionRef);
