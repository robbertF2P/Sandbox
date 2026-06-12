using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Core.Progress;
using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Api.Tests.Progress;

public sealed class ProgressCalculatorTests
{
    [Fact]
    public void ToProjectProgress_RollsUpBudgetedAndWorkedHours()
    {
        var project = new ProjectEntity { Id = 1, Name = "MV Alpha" };
        var block = new ComponentEntity { Id = 10, ProjectId = 1, Name = "Block 204" };
        var activity = new ActivityEntity { Id = 100, ComponentId = 10, Name = "Erection", Component = block };
        var assignment = new AssignmentEntity
        {
            Id = 1000,
            ActivityId = 100,
            PersonName = PersonName.From("Marco"),
            BudgetedHours = Hours.From(40),
            Activity = activity,
            HourBookings =
            [
                new HourBookingEntity { Hours = Hours.From(12) },
                new HourBookingEntity { Hours = Hours.From(8) }
            ]
        };

        activity.Assignments = [assignment];
        block.Activities = [activity];
        block.Project = project;

        var progress = ProgressCalculator.ToProjectProgress(project, [block]);

        Assert.Equal(40, progress.Progress.BudgetedHours.Value);
        Assert.Equal(20, progress.Progress.HoursWorked.Value);
        Assert.Equal(50, progress.Progress.PercentComplete);
        Assert.Equal(20, progress.Components[0].Activities[0].Assignments[0].Progress.HoursWorked.Value);
    }

    [Fact]
    public void Sum_ReturnsZeroPercentWhenNoBudget()
    {
        var summary = ProgressCalculator.Sum([new ProgressSummary(Hours.Zero, Hours.Zero)]);
        Assert.Equal(0, summary.PercentComplete);
    }
}
