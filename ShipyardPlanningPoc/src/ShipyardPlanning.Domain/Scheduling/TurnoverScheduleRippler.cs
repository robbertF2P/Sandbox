using System.Collections.Immutable;
using ShipyardPlanning.Domain.Models;
using ShipyardPlanning.Domain.ValueObjects;

namespace ShipyardPlanning.Domain.Scheduling;

public static class TurnoverScheduleRippler
{
    public static ImmutableList<TurnoverOperation> ForwardPass(
        DateTimeOffset horizonStart,
        ImmutableList<TurnoverOperation> operations,
        ImmutableList<CraneOutage> outages)
    {
        if (operations.IsEmpty)
        {
            return operations;
        }

        Dictionary<string, TurnoverOperation> byCode = operations.ToDictionary(operation => operation.OperationCode);
        List<TurnoverOperation> ordered = TopologicalSort(operations, byCode);
        Dictionary<CraneTag, DateTimeOffset> craneAvailableAt = [];

        List<TurnoverOperation> scheduled = [];

        foreach (TurnoverOperation operation in ordered)
        {
            DateTimeOffset earliest = horizonStart;

            foreach (string predecessorCode in operation.PredecessorCodes)
            {
                if (!byCode.TryGetValue(predecessorCode, out TurnoverOperation? predecessor) || predecessor is null)
                {
                    throw new InvalidOperationException($"Unknown predecessor '{predecessorCode}'.");
                }

                TurnoverOperation scheduledPredecessor = scheduled.First(item => item.OperationCode == predecessorCode);
                DateTimeOffset predecessorEnd = scheduledPredecessor.ScheduledEnd
                    ?? throw new InvalidOperationException($"Predecessor '{predecessorCode}' is not scheduled.");

                earliest = Max(earliest, predecessorEnd);
            }

            if (operation.Crane is CraneTag crane)
            {
                if (craneAvailableAt.TryGetValue(crane, out DateTimeOffset craneFreeAt))
                {
                    earliest = Max(earliest, craneFreeAt);
                }

                earliest = ApplyCraneOutages(crane, earliest, operation.Duration, outages);
            }

            TurnoverOperation scheduledOperation = operation.WithScheduledStart(earliest);
            scheduled.Add(scheduledOperation);

            if (operation.Crane is CraneTag reservedCrane)
            {
                craneAvailableAt[reservedCrane] = scheduledOperation.ScheduledEnd
                    ?? throw new InvalidOperationException("Scheduled operation must have an end time.");
            }
        }

        return scheduled.ToImmutableList();
    }

    public static WorkMinutes CriticalPathLength(ImmutableList<TurnoverOperation> operations)
    {
        if (operations.IsEmpty)
        {
            return new WorkMinutes(0);
        }

        Dictionary<string, WorkMinutes> longestPathTo = [];

        foreach (TurnoverOperation operation in TopologicalSort(operations, operations.ToDictionary(item => item.OperationCode)))
        {
            WorkMinutes incoming = operation.PredecessorCodes
                .Select(code => longestPathTo[code])
                .DefaultIfEmpty(new WorkMinutes(0))
                .Aggregate((left, right) => left + right);

            longestPathTo[operation.OperationCode] = incoming + operation.Duration;
        }

        return longestPathTo.Values.MaxBy(minutes => minutes.Value);
    }

    public static IReadOnlyList<string> FindPrecedenceViolations(ImmutableList<TurnoverOperation> operations)
    {
        Dictionary<string, TurnoverOperation> byCode = operations.ToDictionary(operation => operation.OperationCode);
        List<string> violations = [];

        foreach (TurnoverOperation operation in operations)
        {
            if (operation.ScheduledStart is null)
            {
                continue;
            }

            foreach (string predecessorCode in operation.PredecessorCodes)
            {
                TurnoverOperation predecessor = byCode[predecessorCode];
                if (predecessor.ScheduledEnd is null)
                {
                    continue;
                }

                if (operation.ScheduledStart.Value < predecessor.ScheduledEnd.Value)
                {
                    violations.Add(
                        $"Operation '{operation.OperationCode}' starts before predecessor '{predecessorCode}' finishes.");
                }
            }
        }

        return violations;
    }

    public static IReadOnlyList<string> FindCraneConflicts(ImmutableList<TurnoverOperation> operations)
    {
        List<string> violations = [];

        foreach (IGrouping<CraneTag, TurnoverOperation> craneGroup in operations
                     .Where(operation => operation.Crane is not null && operation.ScheduledStart is not null)
                     .GroupBy(operation => operation.Crane!.Value))
        {
            List<TurnoverOperation> craneOps = craneGroup
                .OrderBy(operation => operation.ScheduledStart)
                .ToList();

            for (int index = 1; index < craneOps.Count; index++)
            {
                TurnoverOperation previous = craneOps[index - 1];
                TurnoverOperation current = craneOps[index];

                if (current.ScheduledStart!.Value < previous.ScheduledEnd!.Value)
                {
                    violations.Add(
                        $"Crane '{craneGroup.Key}' is double-booked between '{previous.OperationCode}' and '{current.OperationCode}'.");
                }
            }
        }

        return violations;
    }

    private static DateTimeOffset ApplyCraneOutages(
        CraneTag crane,
        DateTimeOffset earliest,
        WorkMinutes duration,
        ImmutableList<CraneOutage> outages)
    {
        DateTimeOffset candidate = earliest;

        foreach (CraneOutage outage in outages.Where(item => item.Crane == crane))
        {
            DateTimeOffset candidateEnd = candidate.Add(duration.ToTimeSpan());
            if (candidate < outage.EndsAt && candidateEnd > outage.StartsAt)
            {
                candidate = Max(candidate, outage.EndsAt);
            }
        }

        return candidate;
    }

    private static List<TurnoverOperation> TopologicalSort(
        ImmutableList<TurnoverOperation> operations,
        Dictionary<string, TurnoverOperation> byCode)
    {
        HashSet<string> visited = [];
        HashSet<string> visiting = [];
        List<TurnoverOperation> ordered = [];

        void Visit(string code)
        {
            if (visited.Contains(code))
            {
                return;
            }

            if (visiting.Contains(code))
            {
                throw new InvalidOperationException($"Cycle detected at operation '{code}'.");
            }

            visiting.Add(code);

            foreach (string predecessor in byCode[code].PredecessorCodes)
            {
                Visit(predecessor);
            }

            visiting.Remove(code);
            visited.Add(code);
            ordered.Add(byCode[code]);
        }

        foreach (TurnoverOperation operation in operations)
        {
            Visit(operation.OperationCode);
        }

        return ordered;
    }

    private static DateTimeOffset Max(DateTimeOffset left, DateTimeOffset right) =>
        left >= right ? left : right;
}
