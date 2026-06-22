using System.Collections.Immutable;
using ShipyardPlanning.Domain.Models;
using ShipyardPlanning.Domain.ValueObjects;

namespace ShipyardPlanning.Tests.Support;

internal static class FerryBerth7Scenario
{
    internal static readonly CraneTag Goliath = new("GOLIATH");
    internal static readonly DateTimeOffset Horizon = new(2026, 6, 2, 6, 0, 0, TimeSpan.Zero);

    internal static BlockTurnoverPlan DraftPlan() =>
        new BlockTurnoverPlan(new HullNumber("HULL-4721"), "Berth 7", Horizon)
            .WithOperations(Operations());

    internal static ImmutableList<TurnoverOperation> Operations() =>
    [
        new("A12-FIT", new BlockCode("A12"), TurnoverOperationKind.FitUp, new WorkMinutes(240), []),
        new("A12-WELD", new BlockCode("A12"), TurnoverOperationKind.Weld, new WorkMinutes(480), ["A12-FIT"]),
        new("A12-NDT", new BlockCode("A12"), TurnoverOperationKind.NonDestructiveTest, new WorkMinutes(120), ["A12-WELD"]),
        new("A12-LIFT", new BlockCode("A12"), TurnoverOperationKind.CraneTurnover, new WorkMinutes(60), ["A12-NDT"], Goliath),
        new("B03-FIT", new BlockCode("B03"), TurnoverOperationKind.FitUp, new WorkMinutes(180), []),
        new("B03-WELD", new BlockCode("B03"), TurnoverOperationKind.Weld, new WorkMinutes(360), ["B03-FIT"]),
        new("B03-LIFT", new BlockCode("B03"), TurnoverOperationKind.CraneTurnover, new WorkMinutes(60), ["B03-WELD"], Goliath),
    ];

    internal static TurnoverOperation Find(BlockTurnoverPlan plan, string operationCode) =>
        plan.Operations.First(operation => operation.OperationCode == operationCode);
}
