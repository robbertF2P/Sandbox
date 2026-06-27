using Akka.Actor;
using Akka.Hosting;
using ControlPlane.Application.Ports;
using ControlPlane.Contracts.Interfaces;
using ControlPlane.Contracts.Messages.Provisioning;
using ControlPlane.Core.Actors;
using ControlPlane.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.ControlPlane.Client;
using Platform.ControlPlane.Contracts;
using Platform.Serilog.Logging.Akka;

namespace ControlPlane.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddControlPlaneActors(this IServiceCollection services)
    {
        services.AddAkka<ControlPlaneActorHostedService>("admin-backoffice", (builder, serviceProvider) =>
        {
            builder
                .ConfigureSerilogLogging()
                .WithActorSystemLivenessCheck()
                .WithActors((system, registry) =>
                {
                    IDbContextFactory<ControlPlaneDbContext> dbContextFactory =
                        serviceProvider.GetRequiredService<IDbContextFactory<ControlPlaneDbContext>>();
                    IPlatformConfigurationClient platformClient =
                        serviceProvider.GetRequiredService<IPlatformConfigurationClient>();

                    IActorRef rootActor = system.ActorOf(
                        RootActor.Props(dbContextFactory, platformClient),
                        "control-plane-root");
                    registry.Register<RootActor>(rootActor);
                });
        });

        services.AddSingleton<IControlPlaneActorFacade>(sp =>
        {
            IActorRef rootActor = sp.GetRequiredService<IRequiredActor<RootActor>>().ActorRef;
            return new ControlPlaneActorFacade(rootActor);
        });

        return services;
    }
}

internal sealed class ControlPlaneActorHostedService : AkkaHostedService
{
    public ControlPlaneActorHostedService(
        AkkaConfigurationBuilder configurationBuilder,
        IServiceProvider serviceProvider,
        ILogger<ControlPlaneActorHostedService> logger,
        IHostApplicationLifetime applicationLifetime)
        : base(configurationBuilder, serviceProvider, logger, applicationLifetime)
    {
    }
}

public sealed class ControlPlaneActorFacade : IControlPlaneActorFacade
{
    private static readonly TimeSpan _askTimeout = TimeSpan.FromSeconds(60);
    private readonly IActorRef _rootActor;

    public ControlPlaneActorFacade(IActorRef rootActor)
    {
        _rootActor = rootActor;
    }

    public Task<ProvisionTenantResult> ProvisionTenantAsync(
        ProvisionTenantRequest request,
        CancellationToken cancellationToken = default) =>
        _rootActor.AskCorrelated<ProvisionTenantResult>(
            new ProvisionTenantCommand(request),
            "Tenant.Provision",
            _askTimeout,
            cancellationToken);

    public Task<SyncTenantResult> SyncTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        _rootActor.AskCorrelated<SyncTenantResult>(
            new SyncTenantCommand(tenantId),
            "Tenant.Sync",
            _askTimeout,
            cancellationToken);
}
