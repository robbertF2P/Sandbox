using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;

namespace ApiImportActorPoc.Core.Actors.Import;

public sealed class ImportSessionActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Guid _sessionId;
    private readonly ProjectImportPayload _payload;
    private readonly IActorRef _importManager;

    public ImportSessionActor(Guid sessionId, ProjectImportPayload payload, IActorRef importManager)
    {
        _sessionId = sessionId;
        _payload = payload;
        _importManager = importManager;

        Receive<BuildImportModelCommand>(HandleBuild);
    }

    public static Props Props(Guid sessionId, ProjectImportPayload payload, IActorRef importManager) =>
        Akka.Actor.Props.Create(() => new ImportSessionActor(sessionId, payload, importManager));

    protected override void PreStart()
    {
        Self.Tell(new BuildImportModelCommand(_sessionId, _payload));
    }

    private void HandleBuild(BuildImportModelCommand command)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        Context.System.EventStream.Publish(new ImportStarted(_sessionId, command.Payload.Name, occurredAt));

        try
        {
            var result = ProjectModelBuilder.Build(command.Payload, progress =>
            {
                Context.System.EventStream.Publish(new ImportProgressUpdated(
                    _sessionId,
                    progress.Step,
                    progress.TotalSteps,
                    progress.Message,
                    DateTimeOffset.UtcNow));
            });

            Context.System.EventStream.Publish(new ImportCompleted(_sessionId, result.Model, DateTimeOffset.UtcNow));
            _importManager.Tell(new RegisterImportSession(_sessionId, result.Model));
            _log.Info("Import session {0} completed for project '{1}'", _sessionId, result.Model.Name);
        }
        catch (Exception exception)
        {
            Context.System.EventStream.Publish(new ImportFailed(_sessionId, exception.Message, DateTimeOffset.UtcNow));
            _importManager.Tell(new BuildImportModelCompleted(_sessionId, false, null, exception.Message));
            _log.Warning(exception, "Import session {0} failed", _sessionId);
        }
        finally
        {
            Context.Stop(Self);
        }
    }
}
