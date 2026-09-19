using Akka.Configuration;
using Akka.TestKit.Xunit;
using Floor2Plan.TestUtility.Common.Logging;
using Xunit;

namespace Floor2Plan.TestUtility.Common.Akka
{
    /// <summary>
    /// Akka TestKit base that routes actor logs to Serilog (console) and xUnit test output
    /// so developers see pipeline activity during <c>dotnet test</c>.
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
            global::Serilog.Log.Logger = AkkaTestLogging.CreateLogger();
        }
    }
}
