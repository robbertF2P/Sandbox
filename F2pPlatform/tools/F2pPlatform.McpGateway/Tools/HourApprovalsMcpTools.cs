using System.ComponentModel;
using F2pPlatform.McpGateway.Services;
using ModelContextProtocol.Server;

namespace F2pPlatform.McpGateway.Tools;

[McpServerToolType]
public sealed class HourApprovalsMcpTools(F2pPlatformApiClient apiClient)
{
    private readonly F2pPlatformApiClient _apiClient = apiClient;

    [McpServerTool]
    [Description("Check whether the Hour Approvals feature is enabled and what the current user can do.")]
    public Task<string> GetHourApprovalsCapabilities(CancellationToken cancellationToken) =>
        _apiClient.GetJsonAsync("api/hour-approvals/capabilities", cancellationToken);

    [McpServerTool]
    [Description(
        "List hour approval tasks. Optional filter: all (default), approved, or not_approved.")]
    public Task<string> ListHourApprovalTasks(
        [Description("Approval filter: all, approved, or not_approved.")]
        string approvalStatus = "all",
        CancellationToken cancellationToken = default)
    {
        string filter = NormalizeApprovalFilter(approvalStatus);
        string path = filter == "all"
            ? "api/hour-approvals/tasks"
            : $"api/hour-approvals/tasks?approvalStatus={filter}";

        return _apiClient.GetJsonAsync(path, cancellationToken);
    }

    [McpServerTool]
    [Description("Get one hour approval task by id, including current values and last approval metadata.")]
    public Task<string> GetHourApprovalTask(
        [Description("Task id (GUID).")]
        Guid taskId,
        CancellationToken cancellationToken) =>
        _apiClient.GetJsonAsync($"api/hour-approvals/tasks/{taskId}", cancellationToken);

    [McpServerTool]
    [Description(
        "Approve the current values for one hour approval task. Requires ApproveHoursProgress permission.")]
    public Task<string> ApproveHourApprovalTask(
        [Description("Task id (GUID) to approve.")]
        Guid taskId,
        CancellationToken cancellationToken) =>
        _apiClient.PostJsonAsync($"api/hour-approvals/tasks/{taskId}/approve", cancellationToken);

    [McpServerTool]
    [Description("Check F2P Platform host health.")]
    public Task<string> GetPlatformHealth(CancellationToken cancellationToken) =>
        _apiClient.GetJsonAsync("health", cancellationToken);

    private static string NormalizeApprovalFilter(string approvalStatus) =>
        approvalStatus.Trim().ToLowerInvariant() switch
        {
            "approved" => "approved",
            "not_approved" or "not-approved" or "notapproved" => "not_approved",
            _ => "all",
        };
}
