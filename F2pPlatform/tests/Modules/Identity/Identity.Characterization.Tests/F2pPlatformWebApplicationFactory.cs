using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Identity.Characterization.Tests;

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
        builder.UseSetting("Tenant:FeatureFlags:hours-progress-approval", "true");
        builder.UseSetting("Tenant:PackEntitlements:customizationPacks:0", "acme-hour-approvals-v1");
        builder.UseSetting("HourApprovals:SqlitePath", _hourApprovalsSqlitePath);
        builder.UseSetting("PlatformConfig:SqlitePath", _platformConfigSqlitePath);
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
