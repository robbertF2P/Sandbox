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

    IReadOnlyDictionary<string, object?> GetRowExtensions(Guid taskId);
}
