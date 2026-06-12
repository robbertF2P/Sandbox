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
            "Office Fit-out",
            [
                new ComponentImportPayload(
                    "c1",
                    "Building",
                    [
                        new ComponentImportPayload(
                            "c2",
                            "Floor 1",
                            null,
                            [
                                new ActivityImportPayload(
                                    activityA,
                                    "Demolition",
                                    [new AssignmentImportPayload(null, "Alex", "Lead")],
                                    [new ActivityRelationImportPayload(activityB, "Successor")]),
                                new ActivityImportPayload(
                                    activityB,
                                    "Framing",
                                    [new AssignmentImportPayload(null, "Sam", null)],
                                    [new ActivityRelationImportPayload(activityA, "Predecessor")])
                            ])
                    ],
                    null)
            ]);

        var result = ProjectModelBuilder.Build(payload);

        Assert.Equal("Office Fit-out", result.Model.Name);
        Assert.Single(result.Model.Components);
        Assert.Equal("Building", result.Model.Components[0].Name);
        Assert.Single(result.Model.Components[0].ChildComponents);
        Assert.Equal(2, result.Model.Components[0].ChildComponents[0].Activities.Count);

        ActivityModel demolition = result.Model.Components[0].ChildComponents[0].Activities
            .Single(activity => activity.Name == "Demolition");
        Assert.Single(demolition.Assignments);
        Assert.Equal("Alex", demolition.Assignments[0].PersonName);
        Assert.Contains(
            demolition.Relations,
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
