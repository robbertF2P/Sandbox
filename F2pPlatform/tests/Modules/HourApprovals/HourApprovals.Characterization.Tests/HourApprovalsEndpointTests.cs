using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HourApprovals.Characterization.Tests;

public sealed class HourApprovalsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Tenant:FeatureFlags:hours-progress-approval", "true");
    }
}

public sealed class HourApprovalsDisabledWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Tenant:FeatureFlags:hours-progress-approval", "false");
    }
}

[Trait("Module", "HourApprovals")]
[Trait("Tier", "Characterization")]
public sealed class HourApprovalsEndpointTests : IClassFixture<HourApprovalsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HourApprovalsEndpointTests(HourApprovalsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCapabilities_ReturnsAcmePackAndPermissions()
    {
        using HttpRequestMessage request = CreateSupervisorRequest(HttpMethod.Get, "/api/hour-approvals/capabilities");
        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CapabilitiesResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.FeatureEnabled);
        Assert.Equal("acme-hour-approvals-v1", payload.CustomizationPackId);
        Assert.True(payload.DisplaySettings.ShowPlannedStart);
        Assert.True(payload.DisplaySettings.ShowPlannedFinish);
        Assert.True(payload.CanApprove);
    }

    [Fact]
    public async Task SaveTask_CreatesApprovalRecord_ForSupervisor()
    {
        Guid taskId = Guid.Parse("11111111-1111-1111-1111-111111111101");

        using HttpRequestMessage saveRequest = CreateSupervisorRequest(
            HttpMethod.Put,
            $"/api/hour-approvals/tasks/{taskId}",
            new
            {
                hoursToGo = 11m,
                progress = 36m,
                workedHours = 49m,
                plannedStart = "2026-06-10",
                plannedFinish = "2026-06-24",
            });

        using HttpResponseMessage saveResponse = await _client.SendAsync(saveRequest);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var saved = await saveResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(saved);
        Assert.Equal("Approved", saved.ApprovalState);
        Assert.NotNull(saved.LastApproval);
        Assert.Equal("supervisor.demo", saved.LastApproval.ApprovedBy);
    }

    [Fact]
    public async Task SaveTask_ReturnsForbidden_ForForemanWithoutPermission()
    {
        Guid taskId = Guid.Parse("11111111-1111-1111-1111-111111111102");

        using HttpRequestMessage saveRequest = CreateForemanRequest(
            HttpMethod.Put,
            $"/api/hour-approvals/tasks/{taskId}",
            new
            {
                hoursToGo = 18m,
                progress = 12m,
                workedHours = 9m,
                plannedStart = "2026-06-12",
                plannedFinish = "2026-07-01",
            });

        using HttpResponseMessage saveResponse = await _client.SendAsync(saveRequest);
        Assert.Equal(HttpStatusCode.Forbidden, saveResponse.StatusCode);
    }

    [Fact]
    public async Task ListTasks_FiltersByApprovalStatus()
    {
        using HttpRequestMessage request = CreateSupervisorRequest(
            HttpMethod.Get,
            "/api/hour-approvals/tasks?approvalStatus=approved");

        using HttpResponseMessage response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();
        Assert.NotNull(tasks);
        Assert.All(tasks, task => Assert.Equal("Approved", task.ApprovalState));
    }

    private static HttpRequestMessage CreateSupervisorRequest(
        HttpMethod method,
        string url,
        object? body = null) =>
        CreateRequest(method, url, "supervisor.demo", ["ApproveHoursProgress"], body);

    private static HttpRequestMessage CreateForemanRequest(
        HttpMethod method,
        string url,
        object? body = null) =>
        CreateRequest(method, url, "foreman.demo", [], body);

    private static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string url,
        string userName,
        string[] permissions,
        object? body)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-User-Name", userName);
        request.Headers.Add("X-User-Permissions", string.Join(',', permissions));

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private sealed record CapabilitiesResponse(
        bool FeatureEnabled,
        string CustomizationPackId,
        DisplaySettingsResponse DisplaySettings,
        bool CanApprove,
        List<string> Permissions);

    private sealed record DisplaySettingsResponse(bool ShowPlannedStart, bool ShowPlannedFinish);

    private sealed record TaskResponse(
        Guid Id,
        string ApprovalState,
        LastApprovalResponse? LastApproval);

    private sealed record LastApprovalResponse(string ApprovedBy, DateTimeOffset ApprovedAtUtc);
}

[Trait("Module", "HourApprovals")]
[Trait("Tier", "Characterization")]
public sealed class HourApprovalsFeatureFlagTests
{
    [Fact]
    public async Task GetCapabilities_ReturnsNotFound_WhenFeatureDisabled()
    {
        await using var factory = new HourApprovalsDisabledWebApplicationFactory();
        using HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Get, "/api/hour-approvals/capabilities");
        request.Headers.Add("X-User-Name", "supervisor.demo");
        using HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
