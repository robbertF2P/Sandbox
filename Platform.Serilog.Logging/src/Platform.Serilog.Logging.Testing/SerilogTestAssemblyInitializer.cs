using System.Runtime.CompilerServices;
using Serilog;

namespace Platform.Serilog.Logging.Testing;

public static class SerilogTestAssemblyInitializer
{
    [ModuleInitializer]
    public static void Configure()
    {
        Log.Logger = SerilogTestLogging.CreateTestLogger();
    }
}
