using System.Collections.Immutable;
using ShipyardPlanning.Domain.Scheduling;
using ShipyardPlanning.Domain.ValueObjects;

namespace ShipyardPlanning.Domain.Models;

public sealed class BlockTurnoverPlan
{
    private List<TurnoverOperation> _operations = [];

    public BlockTurnoverPlan(
        HullNumber hullNumber,
        string berthName,
        DateTimeOffset planningHorizonStart,
        TurnoverPlanStatus status = TurnoverPlanStatus.Draft)
    {
        HullNumber = hullNumber;
        BerthName = ValidateBerth(berthName);
        PlanningHorizonStart = planningHorizonStart;
        Status = status;
    }

    private BlockTurnoverPlan(BlockTurnoverPlan other)
    {
        Id = other.Id;
        PublicId = other.PublicId;
        HullNumber = other.HullNumber;
        BerthName = other.BerthName;
        PlanningHorizonStart = other.PlanningHorizonStart;
        Status = other.Status;
        _operations = [.. other._operations];
        CraneOutages = other.CraneOutages;
    }

    private int Id { get; init; }

    public Guid PublicId { get; private init; } = Guid.NewGuid();

    public HullNumber HullNumber { get; init; }

    public string BerthName { get; init; }

    public DateTimeOffset PlanningHorizonStart { get; init; }

    public TurnoverPlanStatus Status { get; init; }

    public ImmutableList<CraneOutage> CraneOutages { get; init; } = ImmutableList<CraneOutage>.Empty;

    public ImmutableList<TurnoverOperation> Operations => _operations.ToImmutableList();

    public DateTimeOffset? PlanEnd =>
        _operations
            .Select(operation => operation.ScheduledEnd)
            .Where(end => end is not null)
            .DefaultIfEmpty()
            .Max();

    public WorkMinutes CriticalPathLength =>
        TurnoverScheduleRippler.CriticalPathLength(_operations.ToImmutableList());

    public BlockTurnoverPlan WithOperations(ImmutableList<TurnoverOperation> operations)
    {
        BlockTurnoverPlan copy = new BlockTurnoverPlan(this);
        copy._operations = operations.ToList();
        return copy;
    }

    public BlockTurnoverPlan WithStatus(TurnoverPlanStatus status) =>
        new BlockTurnoverPlan(this) { Status = status };

    public BlockTurnoverPlan RecalculateSchedule()
    {
        BlockTurnoverPlan copy = new BlockTurnoverPlan(this);
        copy._operations = TurnoverScheduleRippler.ForwardPass(
            PlanningHorizonStart,
            _operations.ToImmutableList(),
            CraneOutages).ToList();
        return copy;
    }

    public BlockTurnoverPlan WithCraneBreakdown(CraneTag crane, DateTimeOffset breakdownAt, TimeSpan downtime)
    {
        BlockTurnoverPlan withOutage = new BlockTurnoverPlan(this)
        {
            CraneOutages = CraneOutages.Add(new CraneOutage(crane, breakdownAt, downtime)),
        };
        return withOutage.RecalculateSchedule();
    }

    public BlockTurnoverPlan Commit()
    {
        IReadOnlyList<string> violations = ValidateForCommit();
        if (violations.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, violations));
        }

        return WithStatus(TurnoverPlanStatus.Committed);
    }

    public IReadOnlyList<string> ValidateForCommit()
    {
        List<string> violations = [];

        if (_operations.Any(operation => operation.ScheduledStart is null))
        {
            violations.Add("Every turnover operation must be scheduled before commit.");
        }

        ImmutableList<TurnoverOperation> scheduled = _operations.ToImmutableList();
        violations.AddRange(TurnoverScheduleRippler.FindPrecedenceViolations(scheduled));
        violations.AddRange(TurnoverScheduleRippler.FindCraneConflicts(scheduled));

        return violations;
    }

    private static string ValidateBerth(string berthName) =>
        !string.IsNullOrWhiteSpace(berthName) ? berthName
        : throw new ArgumentException("Berth name is required.", nameof(berthName));
}
