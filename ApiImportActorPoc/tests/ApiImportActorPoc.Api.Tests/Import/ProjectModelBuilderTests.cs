using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;

namespace ApiImportActorPoc.Api.Tests.Import;

public sealed class ProjectModelBuilderTests
{
    [Fact]
    public void Build_CreatesNestedComponentsActivitiesAndRelations()
    {
        var activityA = "a1";
        var activityB = "a2";

        var payload = new ProjectImportPayload(
            "MV Northern Star — Hull 247",
            [
                new ComponentImportPayload(
                    "c1",
                    "Hull Block 204",
                    [
                        new ComponentImportPayload(
                            "c2",
                            "Engine Room Module",
                            null,
                            [
                                new ActivityImportPayload(
                                    activityA,
                                    "Block Erection",
                                    [new AssignmentImportPayload(null, "Marco van Berg", "Crane supervisor")],
                                    [new ActivityRelationImportPayload(activityB, "Successor")]),
                                new ActivityImportPayload(
                                    activityB,
                                    "Structural Welding",
                                    [new AssignmentImportPayload(null, "Elena Petrov", "Certified welder")],
                                    [new ActivityRelationImportPayload(activityA, "Predecessor")])
                            ])
                    ],
                    null)
            ]);

        var result = ProjectModelBuilder.Build(payload);

        Assert.Equal("MV Northern Star — Hull 247", result.Model.Name);
        Assert.Single(result.Model.Components);
        Assert.Equal("Hull Block 204", result.Model.Components[0].Name);
        Assert.Single(result.Model.Components[0].ChildComponents);
        Assert.Equal(2, result.Model.Components[0].ChildComponents[0].Activities.Count);

        ActivityModel erection = result.Model.Components[0].ChildComponents[0].Activities
            .Single(activity => activity.Name == "Block Erection");
        Assert.Single(erection.Assignments);
        Assert.Equal("Marco van Berg", erection.Assignments[0].PersonName);
        Assert.Contains(
            erection.Relations,
            relation => relation.Type == ActivityRelationType.Successor);
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
                            "known",
                            "Only",
                            null,
                            [new ActivityRelationImportPayload("missing", "Successor")])
                    ])
            ]);

        Assert.Throws<InvalidOperationException>(() => ProjectModelBuilder.Build(payload));
    }
}
