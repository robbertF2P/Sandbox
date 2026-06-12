using ApiImportActorPoc.Api.Services;
using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Progress;

public sealed class HourBookingServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private IDbContextFactory<ImportDbContext> _dbContextFactory = null!;
    private ProjectImportUpsertService _upsertService = null!;
    private HourBookingService _hourBookingService = null!;

    public async ValueTask InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ImportDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using (var db = new ImportDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
        }

        _dbContextFactory = new TestDbContextFactory(options);
        _upsertService = new ProjectImportUpsertService(_dbContextFactory);
        _hourBookingService = new HourBookingService(_dbContextFactory);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task BookHoursAsync_PersistsBookingAndUpdatesList()
    {
        var payload = new ProjectImportPayload(
            "MV Alpha",
            [
                new ComponentImportPayload(
                    null,
                    "Block",
                    null,
                    null,
                    [
                        new ActivityImportPayload(
                            null,
                            "Weld",
                            [new AssignmentImportPayload(null, "Sam", null, 32)],
                            null,
                            null)
                    ],
                    null)
            ]);

        var model = ProjectModelBuilder.Build(payload).Model;
        await _upsertService.UpsertAsync(model);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var assignmentId = await db.Assignments.Select(assignment => assignment.Id).SingleAsync();

        var booking = await _hourBookingService.BookHoursAsync(
            assignmentId,
            new BookHoursRequest(Hours.From(6.5m), "Morning shift"));

        Assert.NotNull(booking);
        Assert.Equal(6.5m, booking.Hours.Value);

        var assignments = await _hourBookingService.ListAssignmentsAsync();
        Assert.Single(assignments);
        Assert.Equal(6.5m, assignments[0].HoursWorked.Value);
        Assert.Equal(32, assignments[0].BudgetedHours.Value);
        Assert.Equal("MV Alpha", assignments[0].ProjectName);
    }

    [Fact]
    public async Task BookHoursAsync_RejectsNonPositiveHours()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _hourBookingService.BookHoursAsync(1, new BookHoursRequest(Hours.Zero, null)));
    }

    private sealed class TestDbContextFactory(DbContextOptions<ImportDbContext> options) : IDbContextFactory<ImportDbContext>
    {
        public ImportDbContext CreateDbContext() => new(options);

        public Task<ImportDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
