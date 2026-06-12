using ApiImportActorPoc.Contracts.Models;
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
            PersonName = "Marco",
            BudgetedHours = 40,
            Activity = activity,
            HourBookings =
            [
                new HourBookingEntity { Hours = 12 },
                new HourBookingEntity { Hours = 8 }
            ]
        };

        activity.Assignments = [assignment];
        block.Activities = [activity];
        block.Project = project;

        var progress = ProgressCalculator.ToProjectProgress(project, [block]);

        Assert.Equal(40, progress.Progress.BudgetedHours);
        Assert.Equal(20, progress.Progress.HoursWorked);
        Assert.Equal(50, progress.Progress.PercentComplete);
        Assert.Equal(20, progress.Components[0].Activities[0].Assignments[0].Progress.HoursWorked);
    }

    [Fact]
    public void Sum_ReturnsZeroPercentWhenNoBudget()
    {
        var summary = ProgressCalculator.Sum([new ProgressSummary(0, 0)]);
        Assert.Equal(0, summary.PercentComplete);
    }
}
