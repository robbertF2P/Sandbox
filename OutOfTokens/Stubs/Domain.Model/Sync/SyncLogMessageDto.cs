using Contracts.Model.Enums;

namespace Domain.Model.Sync;

public sealed class SyncLogMessageDto
{
    public SyncLogMessageDto(string message, SyncInformation syncInformation, SyncType syncType)
    {
        Message = message;
        SyncInformation = syncInformation;
        SyncType = syncType;
    }

    public SyncLogMessageDto(string message, SyncInformation syncInformation, SyncType syncType, int quantity)
        : this(message, syncInformation, syncType)
    {
        Quantity = quantity;
    }

    public string Message { get; }

    public SyncInformation SyncInformation { get; }

    public SyncType SyncType { get; }

    public int? Quantity { get; }
}
