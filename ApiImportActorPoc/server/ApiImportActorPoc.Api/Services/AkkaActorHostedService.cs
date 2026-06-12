using Akka.Hosting;

namespace ApiImportActorPoc.Api.Services;

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
