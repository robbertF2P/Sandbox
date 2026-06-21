using Akka.Hosting;

namespace F2pPlatform.Host.Services;

public sealed class F2pPlatformActorHostedService(
    AkkaConfigurationBuilder configurationBuilder,
    IServiceProvider serviceProvider,
    ILogger<F2pPlatformActorHostedService> logger,
    IHostApplicationLifetime applicationLifetime)
    : AkkaHostedService(configurationBuilder, serviceProvider, logger, applicationLifetime);
