using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Data;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Actors.Data;

public sealed class DataManagerActor : ReceiveActor
{
    private readonly IDbContextFactory<ImportDbContext> _dbContextFactory;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private IActorRef _projectImportData = ActorRefs.Nobody;

    public DataManagerActor(IDbContextFactory<ImportDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public static Props Props(IDbContextFactory<ImportDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new DataManagerActor(dbContextFactory));

    protected override void PreStart()
    {
        _projectImportData = Context.ActorOf(
            ProjectImportDataActor.Props(_dbContextFactory),
            "project-import-data");
        Become(Ready);
    }

    private void Ready()
    {
        Receive<PersistImportWithModelCommand>(HandlePersist);
    }

    private void HandlePersist(PersistImportWithModelCommand command)
    {
        _log.Info("Data manager starting persist for import session {0}", command.SessionId);
        _projectImportData.Tell(new PersistProjectImportDataCommand(command.SessionId, command.Model));
        Become(() => WaitForPersistResult(command.SessionId, Sender));
    }

    private void WaitForPersistResult(Guid sessionId, IActorRef originalSender)
    {
        Receive<PersistProjectImportDataResult>(result =>
        {
            if (result.Success && result.ProjectId is int projectId)
            {
                Context.System.EventStream.Publish(new ImportPersisted(sessionId, projectId, DateTimeOffset.UtcNow));
                originalSender.Tell(new PersistImportResult(true, projectId, null));
                _log.Info("Data manager completed persist for session {0} as project {1}", sessionId, projectId);
            }
            else
            {
                originalSender.Tell(new PersistImportResult(false, null, result.ErrorMessage));
                _log.Warning(
                    "Data manager persist failed for session {0}: {1}",
                    sessionId,
                    result.ErrorMessage ?? "unknown error");
            }

            Become(Ready);
        });
    }
}
