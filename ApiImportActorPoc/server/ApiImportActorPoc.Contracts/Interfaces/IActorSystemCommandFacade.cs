using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;

namespace ApiImportActorPoc.Contracts.Interfaces;

public interface IActorSystemCommandFacade
{
    Task<StartImportResult> StartImportAsync(
        ProjectImportPayload payload,
        CancellationToken cancellationToken = default);

    Task<GetImportModelResult> GetImportModelAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<PersistImportResult> PersistImportAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
