using F2pPlatform.McpGateway.Services;
using F2pPlatform.McpGateway.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.Configure<F2pApiOptions>(options =>
{
    options.BaseUrl = ReadEnv("F2P_API_BASE_URL") ?? options.BaseUrl;
    options.UserName = ReadEnv("F2P_USER_NAME") ?? options.UserName;
    options.UserPermissions = ReadEnv("F2P_USER_PERMISSIONS") ?? options.UserPermissions;
});

builder.Services.AddHttpClient<F2pPlatformApiClient>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(HourApprovalsMcpTools).Assembly);

await builder.Build().RunAsync();

static string? ReadEnv(string name) =>
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))
        ? null
        : Environment.GetEnvironmentVariable(name);
