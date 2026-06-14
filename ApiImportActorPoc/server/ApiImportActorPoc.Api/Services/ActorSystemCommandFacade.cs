using Akka.Actor;
using ApiImportActorPoc.Contracts.Interfaces;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Messages.Progress;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Api.Services;

public sealed class ActorSystemCommandFacade : IActorSystemCommandFacade
{
    private static readonly TimeSpan _askTimeout = TimeSpan.FromSeconds(30);
    private readonly IActorRef _rootActor;

    public ActorSystemCommandFacade(IActorRef rootActor)
    {
        _rootActor = rootActor;
    }

    public Task<StartImportResult> StartImportAsync(
        ProjectImportPayload payload,
        CancellationToken cancellationToken = default) =>
        _rootActor.Ask<StartImportResult>(new StartImportCommand(payload), _askTimeout, cancellationToken);

    public Task<GetImportModelResult> GetImportModelAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default) =>
        _rootActor.Ask<GetImportModelResult>(new GetImportModelQuery(sessionId), _askTimeout, cancellationToken);

    public Task<PersistImportResult> PersistImportAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default) =>
        _rootActor.Ask<PersistImportResult>(new PersistImportCommand(sessionId), _askTimeout, cancellationToken);

    public Task<BookHoursResult> BookHoursAsync(
        int assignmentId,
        Hours hours,
        string? notes,
        CancellationToken cancellationToken = default) =>
        _rootActor.Ask<BookHoursResult>(
            new BookHoursCommand(assignmentId, hours, notes),
            _askTimeout,
            cancellationToken);
}
