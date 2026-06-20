using System.Runtime.CompilerServices;
using Serilog;

namespace PrimaveraExcelReader.Tests;

internal static class SerilogTestAssemblyInitializer
{
    [ModuleInitializer]
    internal static void Configure()
    {
        Log.Logger = SerilogTestLogging.CreateTestLogger();
    }
}
