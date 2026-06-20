using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models;
using Platform.Serilog.Logging.Akka;
using Platform.Serilog.Logging.Correlation;

namespace ApiImportActorPoc.Core.Actors.Import;

public sealed class ImportManagerActor : PlatformReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<Guid, ProjectModel> _sessions = [];

    public ImportManagerActor()
    {
        ReceiveCorrelated<StartImportCommand>(HandleStartImport);
        ReceiveCorrelated<RegisterImportSession>(HandleRegisterSession);
        ReceiveCorrelated<BuildImportModelCompleted>(HandleBuildCompleted);
        ReceiveCorrelated<GetImportModelQuery>(HandleGetModel);
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new ImportManagerActor());

    private void HandleStartImport(StartImportCommand command, CorrelationFlow flow)
    {
        var sessionId = Guid.NewGuid();
        Context.ActorOf(
            ImportSessionActor.Props(sessionId, command.Payload, Self, flow.CorrelationId, flow.UseCase),
            $"import-session-{sessionId:N}");

        _log.Info("Started import session {0} for use case {1}", sessionId, flow.UseCase);
        Sender.Tell(new StartImportResult(sessionId, true, null));
    }

    private void HandleRegisterSession(RegisterImportSession message, CorrelationFlow flow)
    {
        _sessions[message.SessionId] = message.Model;
        Sender.Tell(new BuildImportModelCompleted(message.SessionId, true, message.Model, null));
    }

    private void HandleBuildCompleted(BuildImportModelCompleted message, CorrelationFlow flow)
    {
        if (!message.Success)
        {
            _log.Warning("Import session {0} failed: {1}", message.SessionId, message.ErrorMessage);
        }
    }

    private void HandleGetModel(GetImportModelQuery query, CorrelationFlow flow)
    {
        if (_sessions.TryGetValue(query.SessionId, out var model))
        {
            Sender.Tell(new GetImportModelResult(true, model));
            return;
        }

        Sender.Tell(new GetImportModelResult(false, null));
    }
}
