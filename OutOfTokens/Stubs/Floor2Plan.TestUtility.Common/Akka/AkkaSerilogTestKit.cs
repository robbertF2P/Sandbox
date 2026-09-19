using Akka.Hosting;
using Akka.Hosting.TestKit;
using Infrastructure.Akka;
using Microsoft.Extensions.Logging;
using Platform.Serilog.Logging.Testing;
using Xunit;

namespace Floor2Plan.TestUtility.Common.Akka
{
    public abstract class AkkaSerilogTestKit : TestKit
    {
        protected AkkaSerilogTestKit(string testClassName, ITestOutputHelper output)
            : base(testClassName, output)
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
}
