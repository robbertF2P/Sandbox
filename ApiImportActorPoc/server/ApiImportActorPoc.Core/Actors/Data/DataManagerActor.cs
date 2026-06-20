using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Data;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Akka;
using Platform.Serilog.Logging.Correlation;

namespace ApiImportActorPoc.Core.Actors.Data;

public sealed class DataManagerActor : PlatformReceiveActor
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
        RegisterEnvelopeHandler();

        ReceiveCorrelated<PersistImportWithModelCommand>(HandlePersist);
    }

    private void HandlePersist(PersistImportWithModelCommand command, CorrelationFlow flow, IActorRef sender)
    {
        _log.Info("Data manager starting persist for import session {0}", command.SessionId);
        _projectImportData.Tell(flow.WrapChild(
            new PersistProjectImportDataCommand(command.SessionId, command.Model),
            "Import.PersistData"));
        Become(() => WaitForPersistResult(command.SessionId, flow, sender));
    }

    private void WaitForPersistResult(Guid sessionId, CorrelationFlow flow, IActorRef originalSender)
    {
        Receive<PersistProjectImportDataResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (result.Success && result.ProjectId is int projectId)
            {
                Context.System.EventStream.Publish(new ImportPersisted(
                    sessionId,
                    projectId,
                    DateTimeOffset.UtcNow,
                    flow.CorrelationId,
                    flow.UseCase));
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
