using Platform.Shared.View;

namespace HourApprovals.Application.Ports;

public static class HourApprovalsScreens
{
    public const string Queue = "hour-approvals-queue";
}

public interface IHourApprovalsCustomizationPack
{
    string PackId { get; }

    ViewDefinition GetView(string screenId);

    /// <summary>
    /// Batch projection for extension columns — one call per list response, not per row.
    /// </summary>
    IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> taskIds);
}
