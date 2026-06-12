using ApiImportActorPoc.Contracts.Notifications;

namespace ApiImportActorPoc.Core.Publishing;

public interface ISignalrHubWrapper
{
    Task PublishImportEventAsync(ImportEventNotification notification);
}
