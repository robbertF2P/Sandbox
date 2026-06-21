using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Platform.Serilog.Logging;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;
using Serilog;

namespace PrimaveraExcelReader;

public static class ExcelServiceCollectionExtensions
{
    public static IServiceCollection AddExcelReader(this IServiceCollection services)
    {
        services.AddSingleton<IExcelWorkbookAccessor, NpoiExcelWorkbookAccessor>();
        services.AddSingleton<IExcelReaderService, ExcelReaderService>();
        return services;
    }

    public static IServiceCollection AddPrimaveraSerilogLogging(
        this IServiceCollection services,
        Serilog.ILogger? logger = null)
    {
        Serilog.ILogger applicationLogger = logger ?? CreatePrimaveraLogger();

        Log.Logger = applicationLogger;

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(applicationLogger, dispose: false);
        });

        return services;
    }

    private static Serilog.ILogger CreatePrimaveraLogger() =>
        SerilogLogging.ConfigureShared(
            new LoggerConfiguration(),
            configuration => configuration.Enrich.WithProperty("Application", "PrimaveraExcelReader"))
            .WriteTo.Console()
            .CreateLogger();
}
