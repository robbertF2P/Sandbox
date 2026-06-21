namespace Reference.Infrastructure.Adapters;

/// <summary>
/// Marks temporary bridges to legacy code during strangler migration.
/// Include removal ticket in XML doc or adjacent comment.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class StranglerAdapterAttribute : Attribute
{
    public StranglerAdapterAttribute(string removalTicket) => RemovalTicket = removalTicket;

    public string RemovalTicket { get; }
}
