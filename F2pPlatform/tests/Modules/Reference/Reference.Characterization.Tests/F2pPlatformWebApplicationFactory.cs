using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Reference.Characterization.Tests;

public sealed class F2pPlatformWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _hourApprovalsSqlitePath = Path.Combine(
        Path.GetTempPath(),
        $"hour-approvals-{Guid.NewGuid():N}.db");
    private readonly string _platformConfigSqlitePath = Path.Combine(
        Path.GetTempPath(),
        $"platform-config-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        ConfigureTestingHost(builder, _hourApprovalsSqlitePath, _platformConfigSqlitePath);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DeleteIfExists(_hourApprovalsSqlitePath);
            DeleteIfExists(_platformConfigSqlitePath);
        }

        base.Dispose(disposing);
    }

    internal static void ConfigureTestingHost(
        IWebHostBuilder builder,
        string hourApprovalsSqlitePath,
        string platformConfigSqlitePath)
    {
        builder.UseSetting("Tenant:FeatureFlags:hours-progress-approval", "true");
        builder.UseSetting("Tenant:PackEntitlements:customizationPacks:0", "acme-hour-approvals-v1");
        builder.UseSetting("HourApprovals:SqlitePath", hourApprovalsSqlitePath);
        builder.UseSetting("PlatformConfig:SqlitePath", platformConfigSqlitePath);
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

public sealed class LegacyAdapterWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _hourApprovalsSqlitePath = Path.Combine(
        Path.GetTempPath(),
        $"hour-approvals-{Guid.NewGuid():N}.db");
    private readonly string _platformConfigSqlitePath = Path.Combine(
        Path.GetTempPath(),
        $"platform-config-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Reference:UseLegacyAdapter", "true");
        F2pPlatformWebApplicationFactory.ConfigureTestingHost(
            builder,
            _hourApprovalsSqlitePath,
            _platformConfigSqlitePath);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (File.Exists(_hourApprovalsSqlitePath))
            {
                File.Delete(_hourApprovalsSqlitePath);
            }

            if (File.Exists(_platformConfigSqlitePath))
            {
                File.Delete(_platformConfigSqlitePath);
            }
        }

        base.Dispose(disposing);
    }
}
