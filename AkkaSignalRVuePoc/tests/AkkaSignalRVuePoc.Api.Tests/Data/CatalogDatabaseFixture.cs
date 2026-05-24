namespace AkkaSignalRVuePoc.Api.Tests.Data;

public sealed class CatalogDatabaseFixture : IAsyncLifetime
{
    public SqliteCatalogDbContextFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Factory = await SqliteCatalogDbContextFactory.CreateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}
