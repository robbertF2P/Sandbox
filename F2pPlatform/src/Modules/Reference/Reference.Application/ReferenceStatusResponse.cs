using Reference.Application.Ports;
using Reference.Domain;

namespace Reference.Application;

public sealed record ReferenceStatusResponse(
    string ModuleName,
    string Health,
    bool ModuleRegistered,
    bool StranglerAdapterPresent,
    DateTimeOffset CheckedAtUtc)
{
    public static ReferenceStatusResponse FromSnapshot(ReferenceStatusSnapshot snapshot)
    {
        ReferenceHealth health = ReferenceStatusRules.ResolveHealth(
            snapshot.ModuleRegistered,
            snapshot.StranglerAdapterPresent);

        return new ReferenceStatusResponse(
            snapshot.ModuleName,
            health.ToString(),
            snapshot.ModuleRegistered,
            snapshot.StranglerAdapterPresent,
            snapshot.CheckedAtUtc);
    }
}
