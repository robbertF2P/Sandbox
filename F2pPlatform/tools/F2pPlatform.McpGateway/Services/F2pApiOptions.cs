namespace F2pPlatform.McpGateway.Services;

public sealed class F2pApiOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5080";

    public string UserName { get; set; } = "supervisor.demo";

    public string UserPermissions { get; set; } = "ApproveHoursProgress";
}
