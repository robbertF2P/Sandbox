namespace ControlPlane.Domain.Tenants;

public sealed record TenantDeploymentProfile(
    TenantDeploymentMode Mode,
    TenantDataTier DataTier,
    string Region,
    LegacyHostedRuntimeProfile? Legacy,
    NativeRuntimeProfile? Native)
{
    public static TenantDeploymentProfile CreateLegacyHosted(
        TenantDataTier dataTier,
        string region,
        LegacyHostedRuntimeProfile legacy) =>
        new(TenantDeploymentMode.LegacyHosted, dataTier, region, legacy, null);

    public static TenantDeploymentProfile CreateNative(
        TenantDataTier dataTier,
        string region,
        NativeRuntimeProfile native) =>
        new(TenantDeploymentMode.Native, dataTier, region, null, native);
}
