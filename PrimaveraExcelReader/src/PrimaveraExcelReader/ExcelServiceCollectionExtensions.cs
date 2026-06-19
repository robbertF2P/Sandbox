using Microsoft.Extensions.DependencyInjection;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;

namespace PrimaveraExcelReader;

public static class ExcelServiceCollectionExtensions
{
    public static IServiceCollection AddExcelReader(this IServiceCollection services)
    {
        services.AddSingleton<IExcelWorkbookAccessor, NpoiExcelWorkbookAccessor>();
        services.AddSingleton<IExcelReaderService, ExcelReaderService>();
        return services;
    }
}
