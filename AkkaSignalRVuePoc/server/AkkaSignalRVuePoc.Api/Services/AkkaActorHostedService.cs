using Akka.Hosting;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class AkkaActorHostedService : AkkaHostedService
{
    public AkkaActorHostedService(
        AkkaConfigurationBuilder configurationBuilder,
        IServiceProvider serviceProvider,
        ILogger<AkkaActorHostedService> logger,
        IHostApplicationLifetime applicationLifetime)
        : base(configurationBuilder, serviceProvider, logger, applicationLifetime)
    {
    }
}
