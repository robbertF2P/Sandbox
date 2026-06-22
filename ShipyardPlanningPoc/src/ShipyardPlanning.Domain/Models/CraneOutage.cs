using ShipyardPlanning.Domain.ValueObjects;

namespace ShipyardPlanning.Domain.Models;

public readonly record struct CraneOutage(CraneTag Crane, DateTimeOffset StartsAt, TimeSpan Duration)
{
    public DateTimeOffset EndsAt => StartsAt.Add(Duration);
}
