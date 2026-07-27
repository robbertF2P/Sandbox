namespace RecordedProgressApprovals.Domain.ValueObjects;

public sealed record RecordedProgressValues(
    decimal PercentComplete,
    decimal BookedHours,
    DateTimeOffset RecordedAtUtc,
    string Source);
