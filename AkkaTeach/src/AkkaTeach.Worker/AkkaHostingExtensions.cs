using Akka.Actor;
using Akka.Hosting;
using AkkaTeach.Contracts;
using AkkaTeach.Core.Actors;
using AkkaTeach.Core.Clients;
using AkkaTeach.Worker.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AkkaTeach.Worker;

public static class AkkaHostingExtensions
{
    public const string WorkCoordinatorActorName = "work-coordinator";
    public const string SessionActorName = "session";
    public const string DataIngestionActorName = "data-ingestion";

    public static AkkaConfigurationBuilder AddAkkaTeachActors(this AkkaConfigurationBuilder builder)
    {
        return builder.WithActors((system, registry, resolver) =>
        {
            var coordinator = system.ActorOf(WorkCoordinatorActor.Props(), WorkCoordinatorActorName);
            var session = system.ActorOf(SessionActor.Props(), SessionActorName);
            var ingestion = system.ActorOf(resolver.Props<DataIngestionActor>(), DataIngestionActorName);

            registry.Register<WorkCoordinatorActor>(coordinator);
            registry.Register<SessionActor>(session);
            registry.Register<DataIngestionActor>(ingestion);
        });
    }

    public static IServiceCollection AddAkkaTeachActors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DataIngestionOptions>(configuration.GetSection(DataIngestionOptions.SectionName));
        services.AddSingleton<IDataApiClient, MockDataApiClient>();
        services.AddAkka("AkkaTeach", (builder, _) => builder.AddAkkaTeachActors());
        return services;
    }
}

/// <summary>
/// Background worker that periodically triggers paginated API collection and worker-pool processing.
/// </summary>
public sealed class TeachingBackgroundWorker : BackgroundService
{
    private readonly IActorRegistry _actorRegistry;
    private readonly IOptions<DataIngestionOptions> _options;
    private readonly ILogger<TeachingBackgroundWorker> _logger;

    public TeachingBackgroundWorker(
        IActorRegistry actorRegistry,
        IOptions<DataIngestionOptions> options,
        ILogger<TeachingBackgroundWorker> logger)
    {
        _actorRegistry = actorRegistry;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ingestion = await _actorRegistry.GetAsync<DataIngestionActor>(stoppingToken);

        var expectedRecords = _options.Value.TotalPages * _options.Value.PageSize;
        _logger.LogInformation(
            "Data ingestion worker ready — mock API: {Pages} pages × {PageSize} records = {Total} items, pool size {Pool}",
            _options.Value.TotalPages,
            _options.Value.PageSize,
            expectedRecords,
            _options.Value.WorkerPoolSize);

        var cycle = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            cycle++;
            _logger.LogInformation("Starting ingestion cycle {Cycle}", cycle);
            ingestion.Tell(new CollectDataCommand($"cycle-{cycle}"));

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
