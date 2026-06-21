namespace Reference.Domain;

/// <summary>
/// Pure domain rules — FP in detail; no side effects.
/// </summary>
public static class ReferenceStatusRules
{
    public static ReferenceHealth ResolveHealth(bool moduleRegistered, bool adapterPresent) =>
        moduleRegistered switch
        {
            false => ReferenceHealth.Unknown,
            true when adapterPresent => ReferenceHealth.Degraded,
            true => ReferenceHealth.Healthy,
        };
}
