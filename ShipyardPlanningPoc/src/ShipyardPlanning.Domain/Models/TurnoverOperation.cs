using System.Collections.Immutable;
using ShipyardPlanning.Domain.ValueObjects;

namespace ShipyardPlanning.Domain.Models;

public sealed class TurnoverOperation
{
    public TurnoverOperation(
        string operationCode,
        BlockCode blockCode,
        TurnoverOperationKind kind,
        WorkMinutes duration,
        ImmutableList<string> predecessorCodes,
        CraneTag? crane = null)
    {
        OperationCode = ValidateCode(operationCode);
        BlockCode = blockCode;
        Kind = kind;
        Duration = duration;
        PredecessorCodes = predecessorCodes;
        Crane = ValidateCrane(kind, crane);
    }

    private TurnoverOperation(TurnoverOperation other)
    {
        Id = other.Id;
        OperationCode = other.OperationCode;
        BlockCode = other.BlockCode;
        Kind = other.Kind;
        Duration = other.Duration;
        PredecessorCodes = other.PredecessorCodes;
        Crane = other.Crane;
        ScheduledStart = other.ScheduledStart;
    }

    private int Id { get; init; }

    public string OperationCode { get; init; }

    public BlockCode BlockCode { get; init; }

    public TurnoverOperationKind Kind { get; init; }

    public WorkMinutes Duration { get; init; }

    public ImmutableList<string> PredecessorCodes { get; init; }

    public CraneTag? Crane { get; init; }

    public DateTimeOffset? ScheduledStart { get; init; }

    public DateTimeOffset? ScheduledEnd =>
        ScheduledStart?.Add(Duration.ToTimeSpan());

    public TurnoverOperation WithScheduledStart(DateTimeOffset start) =>
        new(this) { ScheduledStart = start };

    public TurnoverOperation WithDelayed(TimeSpan delay)
    {
        if (ScheduledStart is null)
        {
            return this;
        }

        return new(this) { ScheduledStart = ScheduledStart.Value.Add(delay) };
    }

    private static string ValidateCode(string operationCode) =>
        !string.IsNullOrWhiteSpace(operationCode) ? operationCode
        : throw new ArgumentException("Operation code is required.", nameof(operationCode));

    private static CraneTag? ValidateCrane(TurnoverOperationKind kind, CraneTag? crane) =>
        kind == TurnoverOperationKind.CraneTurnover && crane is null
            ? throw new ArgumentException("Crane turnover operations require a crane tag.", nameof(crane))
            : crane;
}
