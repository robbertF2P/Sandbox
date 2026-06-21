using Akka.Hosting;
using Akka.Hosting.TestKit;
using ApiImportActorPoc.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Serilog.Logging.Testing;

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
        global::Serilog.ILogger logger = SerilogTestLogging.CreateTestLogger();
        global::Serilog.Log.Logger = logger;

        builder.AddPlatformSerilog(logger);
    }

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
        builder.ConfigureSerilogLogging();
    }
}
