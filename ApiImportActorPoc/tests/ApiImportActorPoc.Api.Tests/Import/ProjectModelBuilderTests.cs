using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;

namespace ApiImportActorPoc.Api.Tests.Import;

public sealed class ProjectModelBuilderTests
{
    [Fact]
    public void Build_CreatesNestedComponentsActivitiesRelationsAndExternalIds()
    {
        var payload = new ProjectImportPayload(
            "MV Northern Star — Hull 247",
            [
                new ComponentImportPayload(
                    null,
                    "Hull Block 204",
                    [
                        new ComponentImportPayload(
                            null,
                            "Engine Room Module",
                            null,
                            [
                                new ActivityImportPayload(
                                    null,
                                    "Block Erection",
                                    [new AssignmentImportPayload(null, "Marco van Berg", "Crane supervisor")],
                                    [new ActivityRelationImportPayload("PLM:ACT-WELD", "Successor")],
                                    new Dictionary<string, string> { ["PLM"] = "ACT-ERECT" }),
                                new ActivityImportPayload(
                                    null,
                                    "Structural Welding",
                                    [new AssignmentImportPayload(null, "Elena Petrov", "Certified welder")],
                                    [new ActivityRelationImportPayload("PLM:ACT-ERECT", "Predecessor")],
                                    new Dictionary<string, string> { ["PLM"] = "ACT-WELD" })
                            ],
                            new Dictionary<string, string> { ["PLM"] = "MOD-ERM" })
                    ],
                    null,
                    new Dictionary<string, string> { ["PLM"] = "BLOCK-204" })
            ],
            new Dictionary<string, string> { ["PLM"] = "HULL-247" });

        var result = ProjectModelBuilder.Build(payload);

        Assert.Equal("MV Northern Star — Hull 247", result.Model.Name);
        Assert.Equal("HULL-247", result.Model.ExternalIds["PLM"]);
        Assert.Single(result.Model.Components);
        Assert.Equal("BLOCK-204", result.Model.Components[0].ExternalIds["PLM"]);

        ActivityModel erection = result.Model.Components[0].ChildComponents[0].Activities
            .Single(activity => activity.Name == "Block Erection");
        Assert.Equal("ACT-ERECT", erection.ExternalIds["PLM"]);
        Assert.Contains(erection.Relations, relation => relation.Type == ActivityRelationType.Successor);
    }

    [Fact]
    public void Build_ThrowsWhenRelationReferencesUnknownActivity()
    {
        var payload = new ProjectImportPayload(
            "Invalid",
            [
                new ComponentImportPayload(
                    null,
                    "Root",
                    null,
                    [
                        new ActivityImportPayload(
                            null,
                            "Only",
                            null,
                            [new ActivityRelationImportPayload("missing", "Successor")])
                    ])
            ]);

        Assert.Throws<InvalidOperationException>(() => ProjectModelBuilder.Build(payload));
    }

    [Fact]
    public void Build_ThrowsWhenExternalIdIsDuplicatedAcrossEntities()
    {
        var payload = new ProjectImportPayload(
            "Invalid",
            [
                new ComponentImportPayload(
                    null,
                    "Root",
                    null,
                    [
                        new ActivityImportPayload(
                            null,
                            "A",
                            null,
                            null,
                            new Dictionary<string, string> { ["PLM"] = "DUP-1" })
                    ],
                    new Dictionary<string, string> { ["PLM"] = "DUP-1" })
            ]);

        Assert.Throws<InvalidOperationException>(() => ProjectModelBuilder.Build(payload));
    }
}
