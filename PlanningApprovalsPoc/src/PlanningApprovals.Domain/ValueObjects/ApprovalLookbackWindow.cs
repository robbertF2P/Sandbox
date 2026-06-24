namespace PlanningApprovals.Domain.ValueObjects;

/// <summary>
/// How far back to resolve progress and assignment plan state for foreman comparison.
/// </summary>
public sealed record ApprovalLookbackWindow(TimeSpan Duration)
{
    public static ApprovalLookbackWindow OneWeek { get; } = new(TimeSpan.FromDays(7));

    public DateTimeOffset BaselineCutoff(DateTimeOffset asOf) => asOf - Duration;
}
