using RecordedProgressApprovals.Domain.Enums;
using RecordedProgressApprovals.Domain.ValueObjects;

namespace RecordedProgressApprovals.Tests.Support;

public static class RecordedProgressScenario
{
    public static readonly DateTimeOffset Today = new(2026, 7, 2, 10, 0, 0, TimeSpan.Zero);

    public static AssignmentId WeldingAssignmentId => new(42);

    public static ProgressRevisionId FirstRevisionId => new(1001);

    public static PersonId WorkerPersonId => new(10);

    public static PersonId ForemanPersonId => new(20);

    public static PersonId PlannerPersonId => new(30);

    public static PersonId ControllerPersonId => new(40);

    public static PersonId SystemPersonId => new(0);

    public static RecordedProgressValues WeldingProgress() =>
        new(PercentComplete: 35m, BookedHours: 12.5m, RecordedAtUtc: Today, Source: "floorboard");
}
