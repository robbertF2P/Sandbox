namespace ShipyardPlanning.Domain.Models;

public enum TurnoverOperationKind
{
    FitUp,
    Weld,
    NonDestructiveTest,
    CraneTurnover,
}

public enum TurnoverPlanStatus
{
    Draft,
    Committed,
}
