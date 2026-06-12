using ApiImportActorPoc.Api.Tests.Infrastructure;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Import;

public sealed class ProjectImportUpsertServiceTests : IAsyncLifetime
{
    private SqlServerTestDatabase _database = null!;
    private ProjectImportUpsertService _upsertService = null!;

    public async ValueTask InitializeAsync()
    {
        _database = new SqlServerTestDatabase();
        await _database.InitializeAsync();
        _upsertService = new ProjectImportUpsertService(_database.Factory);
    }

    public async ValueTask DisposeAsync()
    {
        await _database.DisposeAsync();
    }

    [Fact]
    public async Task UpsertAsync_CreatesThenUpdatesByExternalId()
    {
        var initial = ProjectModelBuilder.Build(CreatePayload(
            projectName: "MV Alpha",
            blockName: "Hull Block 204",
            activityName: "Block Erection",
            personName: "Marco van Berg"));

        var createResult = await _upsertService.UpsertAsync(initial.Model);
        Assert.True(createResult.Created);

        var updatedPayload = CreatePayload(
            projectName: "MV Alpha Updated",
            blockName: "Hull Block 204 Renamed",
            activityName: "Block Erection Revised",
            personName: "Elena Petrov");

        var updatedModel = ProjectModelBuilder.Build(updatedPayload).Model;
        var updateResult = await _upsertService.UpsertAsync(updatedModel);

        Assert.False(updateResult.Created);
        Assert.Equal(createResult.ProjectId, updateResult.ProjectId);
        Assert.True(createResult.ProjectId > 0);

        await using var db = await _database.Factory.CreateDbContextAsync();
        var project = await db.Projects.SingleAsync();
        Assert.Equal("MV Alpha Updated", project.Name);

        var block = await db.Components.SingleAsync(component => component.Name == "Hull Block 204 Renamed");
        var activity = await db.Activities.SingleAsync();
        Assert.Equal("Block Erection Revised", activity.Name);

        var assignment = await db.Assignments.SingleAsync();
        Assert.Equal(Contracts.Values.PersonName.From("Elena Petrov"), assignment.PersonName);

        var externalIds = await db.EntityExternalIds.CountAsync();
        Assert.Equal(4, externalIds);
    }

    [Fact]
    public async Task UpsertAsync_ThrowsWhenExternalIdBelongsToDifferentEntityType()
    {
        var initial = ProjectModelBuilder.Build(CreatePayload(
            projectName: "MV Beta",
            blockName: "Block",
            activityName: "Weld",
            personName: "Sam"));

        await _upsertService.UpsertAsync(initial.Model);

        var conflictingPayload = new ProjectImportPayload(
            "MV Beta 2",
            [
                new ComponentImportPayload(
                    null,
                    "Other block",
                    null,
                    null,
                    null,
                    new Dictionary<string, string> { ["PLM"] = "HULL-247" })
            ]);

        var conflictingModel = ProjectModelBuilder.Build(conflictingPayload).Model;
        await Assert.ThrowsAsync<InvalidOperationException>(() => _upsertService.UpsertAsync(conflictingModel));
    }

    private static ProjectImportPayload CreatePayload(
        string projectName,
        string blockName,
        string activityName,
        string personName) =>
        new(
            projectName,
            [
                new ComponentImportPayload(
                    null,
                    blockName,
                    null,
                    null,
                    [
                        new ActivityImportPayload(
                            null,
                            activityName,
                            [
                                new AssignmentImportPayload(
                                    null,
                                    personName,
                                    "Trade",
                                    40,
                                    new Dictionary<string, string> { ["HR"] = "PERSON-1" })
                            ],
                            null,
                            new Dictionary<string, string> { ["PLM"] = "ACT-ERECT" })
                    ],
                    new Dictionary<string, string> { ["PLM"] = "BLOCK-204" })
            ],
            new Dictionary<string, string> { ["PLM"] = "HULL-247" });
}
