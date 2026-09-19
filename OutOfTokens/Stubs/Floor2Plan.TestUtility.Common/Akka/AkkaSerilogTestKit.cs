using Akka.Configuration;
using Akka.TestKit.Xunit;
using Platform.Serilog.Logging.Testing;
using Xunit;

namespace Floor2Plan.TestUtility.Common.Akka
{
    /// <summary>
    /// Akka TestKit base wired to <see cref="SerilogTestLogging"/> (platform) and Serilog Akka logging.
    /// Uses <see cref="Akka.TestKit.Xunit.TestKit"/> so actor logs appear in xUnit test output.
    /// </summary>
    public abstract class AkkaSerilogTestKit : TestKit
    {
        private static readonly Config SerilogTestConfig = ConfigurationFactory.ParseString(@"
akka {
  loglevel = INFO
  stdout-loglevel = INFO
  loggers = [""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
  logging.formatter = ""Akka.Logger.Serilog.SerilogLogMessageFormatter, Akka.Logger.Serilog""
}");

        protected AkkaSerilogTestKit(string testClassName, ITestOutputHelper output)
            : base(SerilogTestConfig, testClassName, output)
        {
            global::Serilog.Log.Logger = SerilogTestLogging.CreateTestLogger();
        }
    }
}
