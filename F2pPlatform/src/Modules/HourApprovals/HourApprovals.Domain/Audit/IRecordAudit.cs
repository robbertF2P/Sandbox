namespace HourApprovals.Domain.Audit;

/// <summary>
/// Standard audit fields for append-only approval records.
/// </summary>
public interface IRecordAudit
{
    string CreatedBy { get; }

    DateTimeOffset CreatedAtUtc { get; }
}
