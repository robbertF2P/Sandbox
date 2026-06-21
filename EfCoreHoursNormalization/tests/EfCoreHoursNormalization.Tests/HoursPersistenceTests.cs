using EfCoreHoursNormalization.Entities;
using EfCoreHoursNormalization.Values;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCoreHoursNormalization.Tests;

public sealed class HoursPersistenceTests
{
    [Fact]
    public async Task RoundTrip_NormalizesPollutedNearZeroValues()
    {
        await using var connection = new SqliteConnection("Data Source=hours-persistence;Mode=Memory;Cache=Shared");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = new DbContextOptionsBuilder<HoursDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new HoursDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        var entryId = Guid.NewGuid();

        await using (var writeContext = new HoursDbContext(options))
        {
            writeContext.TimeEntries.Add(new TimeEntry
            {
                Id = entryId,
                Description = "Test",
                Hours = Hours.FromHours(0f)
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        const float pollutedValue = 2.84217094e-14f;

        await using (var pollutionContext = new HoursDbContext(options))
        {
            await pollutionContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 UPDATE TimeEntries
                 SET Hours = {pollutedValue}
                 WHERE Id = {entryId}
                 """,
                TestContext.Current.CancellationToken);
        }

        await using var readContext = new HoursDbContext(options);
        var entry = await readContext.TimeEntries
            .AsNoTracking()
            .SingleAsync(e => e.Id == entryId, TestContext.Current.CancellationToken);

        Assert.True(entry.Hours.IsZero);
        Assert.Equal(0f, entry.Hours.Value);
    }

    [Fact]
    public async Task SaveChanges_RewritesNearZeroAsTrueZero()
    {
        await using var connection = new SqliteConnection($"Data Source=hours-save-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = new DbContextOptionsBuilder<HoursDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new HoursDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        var entryId = Guid.NewGuid();

        await using (var writeContext = new HoursDbContext(options))
        {
            writeContext.TimeEntries.Add(new TimeEntry
            {
                Id = entryId,
                Description = "Test",
                Hours = Hours.FromHours(1.5f - 1.5f)
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        float storedValue;
        await using (var rawContext = new HoursDbContext(options))
        {
            storedValue = await rawContext.Database
                .SqlQuery<float>($"SELECT Hours AS Value FROM TimeEntries WHERE Id = {entryId}")
                .SingleAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(0f, storedValue);
    }
}
