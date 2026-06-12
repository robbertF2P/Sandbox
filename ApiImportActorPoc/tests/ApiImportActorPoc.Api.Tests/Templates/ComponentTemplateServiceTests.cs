using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Core.Templates;
using ApiImportActorPoc.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Templates;

public sealed class ComponentTemplateServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private IDbContextFactory<ImportDbContext> _dbContextFactory = null!;
    private ProjectImportUpsertService _upsertService = null!;
    private ComponentTemplateService _templateService = null!;

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
        _templateService = new ComponentTemplateService(_dbContextFactory);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task InstantiateAsync_CreatesOpenAssignmentsWithBudgetedHoursAndNoWorkedHours()
    {
        var payload = new ProjectImportPayload(
            "MV Template Test",
            [
                new ComponentImportPayload(
                    null,
                    "Standard Block",
                    true,
                    null,
                    [
                        new ActivityImportPayload(
                            null,
                            "Welding",
                            [
                                new AssignmentImportPayload(null, "Marco", "Lead welder", 24),
                                new AssignmentImportPayload(null, "Elena", "Assistant", 16)
                            ],
                            null,
                            null)
                    ],
                    null)
            ]);

        var model = ProjectModelBuilder.Build(payload).Model;
        await _upsertService.UpsertAsync(model);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var templateId = await db.Components
            .Where(component => component.IsTemplate)
            .Select(component => component.Id)
            .SingleAsync();

        var result = await _templateService.InstantiateAsync(
            templateId,
            new InstantiateComponentFromTemplateRequest("Block 205"));

        Assert.NotNull(result);
        Assert.Equal(1, result.ActivityCount);
        Assert.Equal(2, result.AssignmentCount);

        var newComponent = await db.Components
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.Assignments)
                    .ThenInclude(assignment => assignment.HourBookings)
            .SingleAsync(component => component.Id == result.ComponentId);

        Assert.False(newComponent.IsTemplate);
        Assert.Equal("Block 205", newComponent.Name);

        var assignments = newComponent.Activities.Single().Assignments.OrderBy(assignment => assignment.BudgetedHours).ToList();
        Assert.Equal(2, assignments.Count);
        Assert.All(assignments, assignment => Assert.Equal(PersonName.Open, assignment.PersonName));
        Assert.Equal(16, assignments[0].BudgetedHours.Value);
        Assert.Equal(24, assignments[1].BudgetedHours.Value);
        Assert.All(assignments, assignment => Assert.Empty(assignment.HourBookings));
    }

    [Fact]
    public async Task SetTemplateAsync_UpdatesTemplateFlag()
    {
        var payload = new ProjectImportPayload(
            "MV Flag Test",
            [new ComponentImportPayload(null, "Block", null, null, null)]);

        var model = ProjectModelBuilder.Build(payload).Model;
        await _upsertService.UpsertAsync(model);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var componentId = await db.Components.Select(component => component.Id).SingleAsync();

        var updated = await _templateService.SetTemplateAsync(componentId, true);

        Assert.NotNull(updated);
        Assert.True(updated.IsTemplate);

        var templates = await _templateService.ListTemplatesAsync(
            await db.Projects.Select(project => project.Id).SingleAsync());

        Assert.Single(templates);
        Assert.Equal("Block", templates[0].Name);
    }

    [Fact]
    public async Task InstantiateAsync_ReturnsNullWhenComponentIsNotTemplate()
    {
        var payload = new ProjectImportPayload(
            "MV Non-template",
            [new ComponentImportPayload(null, "Block", null, null, null)]);

        var model = ProjectModelBuilder.Build(payload).Model;
        await _upsertService.UpsertAsync(model);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var componentId = await db.Components.Select(component => component.Id).SingleAsync();

        var result = await _templateService.InstantiateAsync(
            componentId,
            new InstantiateComponentFromTemplateRequest("Copy"));

        Assert.Null(result);
    }

    private sealed class TestDbContextFactory(DbContextOptions<ImportDbContext> options) : IDbContextFactory<ImportDbContext>
    {
        public ImportDbContext CreateDbContext() => new(options);

        public Task<ImportDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
