using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Core.Actors.Import;

public sealed class ImportManagerActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<Guid, ProjectModel> _sessions = [];

    public ImportManagerActor()
    {
        Receive<StartImportCommand>(HandleStartImport);
        Receive<RegisterImportSession>(HandleRegisterSession);
        Receive<BuildImportModelCompleted>(HandleBuildCompleted);
        Receive<GetImportModelQuery>(HandleGetModel);
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new ImportManagerActor());

    private void HandleStartImport(StartImportCommand command)
    {
        var sessionId = Guid.NewGuid();
        Context.ActorOf(
            ImportSessionActor.Props(sessionId, command.Payload, Self),
            $"import-session-{sessionId:N}");

        _log.Info("Started import session {0}", sessionId);
        Sender.Tell(new StartImportResult(sessionId, true, null));
    }

    private void HandleRegisterSession(RegisterImportSession message)
    {
        _sessions[message.SessionId] = message.Model;
        Sender.Tell(new BuildImportModelCompleted(message.SessionId, true, message.Model, null));
    }

    private void HandleBuildCompleted(BuildImportModelCompleted message)
    {
        if (!message.Success)
        {
            _log.Warning("Import session {0} failed: {1}", message.SessionId, message.ErrorMessage);
        }
    }

    private void HandleGetModel(GetImportModelQuery query)
    {
        if (_sessions.TryGetValue(query.SessionId, out var model))
        {
            Sender.Tell(new GetImportModelResult(true, model));
            return;
        }

        Sender.Tell(new GetImportModelResult(false, null));
    }
}
