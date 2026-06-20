using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Logging;
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
        Serilog.ILogger applicationLogger = logger ?? SerilogLogging.CreateApplicationLogger();
        Log.Logger = applicationLogger;

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(applicationLogger, dispose: false);
        });

        return services;
    }
}
