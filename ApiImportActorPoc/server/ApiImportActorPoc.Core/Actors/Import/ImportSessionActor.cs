using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;
using Platform.Serilog.Logging.Correlation;

namespace ApiImportActorPoc.Core.Actors.Import;

public sealed class ImportSessionActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Guid _sessionId;
    private readonly ProjectImportPayload _payload;
    private readonly IActorRef _importManager;
    private readonly string _correlationId;
    private readonly string? _useCase;

    public ImportSessionActor(
        Guid sessionId,
        ProjectImportPayload payload,
        IActorRef importManager,
        string correlationId,
        string? useCase)
    {
        _sessionId = sessionId;
        _payload = payload;
        _importManager = importManager;
        _correlationId = correlationId;
        _useCase = useCase;

        Receive<BuildImportModelCommand>(HandleBuild);
    }

    public static Props Props(
        Guid sessionId,
        ProjectImportPayload payload,
        IActorRef importManager,
        string correlationId,
        string? useCase) =>
        Akka.Actor.Props.Create(() => new ImportSessionActor(sessionId, payload, importManager, correlationId, useCase));

    protected override void PreStart()
    {
        Self.Tell(new BuildImportModelCommand(_sessionId, _payload));
    }

    private void HandleBuild(BuildImportModelCommand command)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        Context.System.EventStream.Publish(new ImportStarted(
            _sessionId,
            command.Payload.Name,
            occurredAt,
            _correlationId,
            _useCase));

        try
        {
            var result = ProjectModelBuilder.Build(command.Payload, progress =>
            {
                Context.System.EventStream.Publish(new ImportProgressUpdated(
                    _sessionId,
                    progress.Step,
                    progress.TotalSteps,
                    progress.Message,
                    DateTimeOffset.UtcNow,
                    _correlationId,
                    _useCase));
            });

            Context.System.EventStream.Publish(new ImportCompleted(
                _sessionId,
                result.Model,
                DateTimeOffset.UtcNow,
                _correlationId,
                _useCase));
            _importManager.Tell(new CorrelatedMessageEnvelope(
                new RegisterImportSession(_sessionId, result.Model),
                _correlationId,
                _useCase));
            _log.Info("Import session {0} completed for project '{1}'", _sessionId, result.Model.Name);
        }
        catch (Exception exception)
        {
            Context.System.EventStream.Publish(new ImportFailed(
                _sessionId,
                exception.Message,
                DateTimeOffset.UtcNow,
                _correlationId,
                _useCase));
            _importManager.Tell(new CorrelatedMessageEnvelope(
                new BuildImportModelCompleted(_sessionId, false, null, exception.Message),
                _correlationId,
                _useCase));
            _log.Warning(exception, "Import session {0} failed", _sessionId);
        }
        finally
        {
            Context.Stop(Self);
        }
    }
}
