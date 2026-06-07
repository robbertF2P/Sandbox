using AkkaSignalRVuePoc.Data;
using AkkaSignalRVuePoc.Data.Entities;
using AkkaSignalRVuePoc.Data.Values;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Api.Tests.Data;

public sealed class HoursPersistenceTests
{
    [Fact]
    public async Task RoundTrip_NormalizesPollutedNearZeroValues()
    {
        await using var connection = new SqliteConnection("Data Source=hours-persistence;Mode=Memory;Cache=Shared");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
          .UseSqlite(connection)
          .Options;

        await using (var setup = new CatalogDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        var organisationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        await using (var writeContext = new CatalogDbContext(options))
        {
            writeContext.Organisations.Add(new Organisation
            {
                Id = organisationId,
                Name = "Hours Test Org",
                CreatedAt = DateTimeOffset.UtcNow
            });

            writeContext.Projects.Add(new Project
            {
                Id = projectId,
                OrganisationId = organisationId,
                Name = "Hours Test Project",
                CreatedAt = DateTimeOffset.UtcNow,
                EstimatedHours = Hours.FromHours(0f)
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        const float pollutedValue = 2.84217094e-14f;

        await using (var pollutionContext = new CatalogDbContext(options))
        {
            await pollutionContext.Database.ExecuteSqlInterpolatedAsync(
              $"""
         UPDATE Projects
         SET EstimatedHours = {pollutedValue}
         WHERE Id = {projectId}
         """,
              TestContext.Current.CancellationToken);
        }

        await using var readContext = new CatalogDbContext(options);
        var project = await readContext.Projects
          .AsNoTracking()
          .SingleAsync(project => project.Id == projectId, TestContext.Current.CancellationToken);

        Assert.True(project.EstimatedHours.IsZero);
        Assert.Equal(0f, project.EstimatedHours.Value);
    }

    [Fact]
    public async Task SaveChanges_RewritesNearZeroAsTrueZero()
    {
        await using var connection = new SqliteConnection($"Data Source=hours-save-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
          .UseSqlite(connection)
          .Options;

        await using (var setup = new CatalogDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        var organisationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        await using (var writeContext = new CatalogDbContext(options))
        {
            writeContext.Organisations.Add(new Organisation
            {
                Id = organisationId,
                Name = "Hours Save Org",
                CreatedAt = DateTimeOffset.UtcNow
            });

            writeContext.Projects.Add(new Project
            {
                Id = projectId,
                OrganisationId = organisationId,
                Name = "Hours Save Project",
                CreatedAt = DateTimeOffset.UtcNow,
                EstimatedHours = Hours.FromHours(1.5f - 1.5f)
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        float storedValue;
        await using (var rawContext = new CatalogDbContext(options))
        {
            storedValue = await rawContext.Database
              .SqlQuery<float>($"SELECT EstimatedHours AS Value FROM Projects WHERE Id = {projectId}")
              .SingleAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(0f, storedValue);
    }
}
