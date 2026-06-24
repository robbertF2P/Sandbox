using Identity.Application;
using Identity.Application.Models;
using Identity.Application.Ports;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddIdentityApplication();
        services.AddIdentityInfrastructure();
        return services;
    }

    public static WebApplication MapIdentityModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.MapIdentityEndpoints();
        return app;
    }
}

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity")
            .WithTags("Identity");

        group.MapPost("/login", async (
                LoginRequestBody body,
                IIdentityLoginService loginService,
                CancellationToken cancellationToken) =>
            {
                LoginResponse response = await loginService.LoginAsync(
                    new LoginRequest(body.UserName, body.Password, body.RememberMe),
                    cancellationToken);

                return Results.Ok(response);
            })
            .WithName("IdentityLogin")
            .WithSummary("POC login — accepts any credentials and returns a session token.");

        return app;
    }

    private sealed record LoginRequestBody(
        string UserName,
        string Password,
        bool RememberMe);
}
