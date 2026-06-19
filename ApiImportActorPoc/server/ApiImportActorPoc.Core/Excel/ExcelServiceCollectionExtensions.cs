using ApiImportActorPoc.Core.Excel.Abstractions;
using ApiImportActorPoc.Core.Excel.Mapping;
using ApiImportActorPoc.Core.Excel.Npoi;
using Microsoft.Extensions.DependencyInjection;

namespace ApiImportActorPoc.Core.Excel;

public static class ExcelServiceCollectionExtensions
{
    public static IServiceCollection AddExcelReader(this IServiceCollection services)
    {
        services.AddSingleton<IExcelWorkbookAccessor, NpoiExcelWorkbookAccessor>();
        services.AddSingleton<IExcelReaderService, ExcelReaderService>();
        return services;
    }
}
