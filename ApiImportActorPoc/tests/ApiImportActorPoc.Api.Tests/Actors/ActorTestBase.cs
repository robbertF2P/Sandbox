using Akka.Hosting;
using Akka.Hosting.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApiImportActorPoc.Api.Tests.Actors;

public abstract class ActorTestBase<TTest> : TestKit
    where TTest : ActorTestBase<TTest>
{
    protected ActorTestBase(ITestOutputHelper output)
        : base(typeof(TTest).Name, output)
    {
    }

    protected override void ConfigureLogging(ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.AddDebug();
    }

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
        builder.ConfigureSerilogLogging();
    }
}
