using Akka.Actor;
using ApiImportActorPoc.Contracts.Interfaces;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Messages.Progress;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Contracts.Values;
using Platform.Serilog.Logging.Akka;

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
        _rootActor.AskCorrelated<StartImportResult>(
            new StartImportCommand(payload),
            "Import.Start",
            _askTimeout,
            cancellationToken);

    public Task<GetImportModelResult> GetImportModelAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default) =>
        _rootActor.AskCorrelated<GetImportModelResult>(
            new GetImportModelQuery(sessionId),
            "Import.GetModel",
            _askTimeout,
            cancellationToken);

    public Task<PersistImportResult> PersistImportAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default) =>
        _rootActor.AskCorrelated<PersistImportResult>(
            new PersistImportCommand(sessionId),
            "Import.Persist",
            _askTimeout,
            cancellationToken);

    public Task<BookHoursResult> BookHoursAsync(
        int assignmentId,
        Hours hours,
        string? notes,
        CancellationToken cancellationToken = default) =>
        _rootActor.AskCorrelated<BookHoursResult>(
            new BookHoursCommand(assignmentId, hours, notes),
            "Hours.Book",
            _askTimeout,
            cancellationToken);
}
