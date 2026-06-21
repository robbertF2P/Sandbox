namespace ApiImportActorPoc.Contracts.Platform;

/// <summary>
/// SQL Server hosting tier. Every tenant always has its own database;
/// this enum describes whether that database lives on a shared server instance or a dedicated one.
/// </summary>
public enum TenantDataTier
{
    SharedSqlServer,
    DedicatedSqlServer
}
