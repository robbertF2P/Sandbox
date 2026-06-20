using Akka.Actor;
using Akka.Hosting;
using Akka.Hosting.TestKit;
using AkkaSignalRVuePoc.Api.Hubs;
using AkkaSignalRVuePoc.Api.Services;
using AkkaSignalRVuePoc.Api.Tests.TestDoubles;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Actors;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Serilog.Logging.Testing;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public abstract class ActorTestBase<TTest> : TestKit
    where TTest : ActorTestBase<TTest>
{
    protected ActorTestBase(ITestOutputHelper output)
        : base(typeof(TTest).Name, output)
    {
    }

    protected RecordingHubContext HubContext { get; private set; } = null!;

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        HubContext = new RecordingHubContext();
        services.AddSingleton<IHubContext<LiveMessagesHub>>(HubContext);
    }

    protected override void ConfigureLogging(ILoggingBuilder builder)
    {
        global::Serilog.ILogger logger = SerilogTestLogging.CreateTestLogger();
        global::Serilog.Log.Logger = logger;

        builder.AddPlatformSerilog(logger);
    }

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
        builder.ConfigureSerilogLogging();
    }

    protected IActorRef CreateHubPushActor(string name = "signalr-hub-push")
    {
        return Sys.ActorOf(SignalRHubActor.Props(new SignalRLiveMessageClientPublisher(HubContext)), name);
    }

    protected static PushMessage GetPushMessage(RecordedHubCall call)
    {
        return Assert.IsType<PushMessage>(Assert.Single(call.Arguments));
    }
}
