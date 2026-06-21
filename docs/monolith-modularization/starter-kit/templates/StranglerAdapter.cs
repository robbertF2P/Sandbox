namespace <Context>.Infrastructure.Adapters;

/// <summary>
/// Marks temporary bridges to legacy code during strangler migration.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class StranglerAdapterAttribute : Attribute
{
    public StranglerAdapterAttribute(string removalTicket) => RemovalTicket = removalTicket;

    public string RemovalTicket { get; }
}
