using AkkaSignalRVuePoc.Api.Tests.Data;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

/// <summary>
/// Provides a fresh in-memory SQLite catalog database for each test method.
/// Do not use <see cref="IClassFixture{T}"/> for database state: xUnit shares class fixtures across tests.
/// </summary>
public abstract class ActorDatabaseTestBase<TTest> : ActorTestBase<TTest>
    where TTest : ActorDatabaseTestBase<TTest>
{
    protected ActorDatabaseTestBase(ITestOutputHelper output)
        : base(output)
    {
    }

    protected SqliteCatalogDbContextFactory DatabaseFactory { get; private set; } = null!;

    protected override async Task BeforeTestStart()
    {
        DatabaseFactory = await SqliteCatalogDbContextFactory.CreateAsync(TestContext.Current.CancellationToken);
        await base.BeforeTestStart();
    }

    protected override async Task AfterAllAsync()
    {
        await DatabaseFactory.DisposeAsync();
        await base.AfterAllAsync();
    }
}
