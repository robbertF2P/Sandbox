using ApiImportActorPoc.Api.Tests.Infrastructure;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Import;

public sealed class ProjectImportTemplateOrderTests : IAsyncLifetime
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
    public async Task UpsertAsync_PersistsTemplateComponentsBeforeRegularSiblings()
    {
        var payload = new ProjectImportPayload(
            "MV Template Order",
            [
                new ComponentImportPayload(null, "Regular block", null, null, null, null),
                new ComponentImportPayload(null, "Template block", true, null, null, null)
            ]);

        var model = ProjectModelBuilder.Build(payload).Model;
        await _upsertService.UpsertAsync(model);

        await using var db = await _database.Factory.CreateDbContextAsync();
        var components = await db.Components.OrderBy(component => component.Id).ToListAsync();

        Assert.Equal(2, components.Count);
        Assert.True(components[0].IsTemplate);
        Assert.Equal("Template block", components[0].Name);
        Assert.False(components[1].IsTemplate);
        Assert.Equal("Regular block", components[1].Name);
    }
}
