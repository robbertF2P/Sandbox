using AkkaSignalRVuePoc.Data.Values;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AkkaSignalRVuePoc.Data.Configurations;

public static class MigrationBuilderHoursExtensions
{
    /// <summary>
    /// One-time cleanup for legacy <c>real</c> columns polluted with near-zero float residue.
    /// </summary>
    public static void NormalizeNearZeroHours(
      this MigrationBuilder migrationBuilder,
      string table,
      string column)
    {
        migrationBuilder.Sql(
          $"""
       UPDATE [{table}]
       SET [{column}] = 0
       WHERE ABS([{column}]) < {Hours.Epsilon}
       """);
    }
}
