using Akka.Actor;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Messages.Progress;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Core.Actors.Data;
using ApiImportActorPoc.Core.Actors.Progress;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Akka;
using Platform.Serilog.Logging.Correlation;

namespace ApiImportActorPoc.Core.Actors;

public sealed class RootActor : PlatformReceiveActor
{
    private readonly IDbContextFactory<ImportDbContext> _dbContextFactory;
    private IActorRef _importManager = ActorRefs.Nobody;
    private IActorRef _dataManager = ActorRefs.Nobody;
    private IActorRef _progressManager = ActorRefs.Nobody;

    public RootActor(IDbContextFactory<ImportDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

        Context.System.EventStream.Publish(new ActorSystemStarted(
            Context.System.Name,
            DateTimeOffset.UtcNow));

        Ready();
    }

    public static Props Props(IDbContextFactory<ImportDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new RootActor(dbContextFactory));

    protected override void PreStart()
    {
        _importManager = Context.ActorOf(Import.ImportManagerActor.Props(), "import-manager");
        _dataManager = Context.ActorOf(DataManagerActor.Props(_dbContextFactory), "data-manager");
        _progressManager = Context.ActorOf(ProgressManagerActor.Props(_dbContextFactory), "progress-manager");
    }

    private void Ready()
    {
        RegisterEnvelopeHandler();

        ReceiveCorrelated<StartImportCommand>((command, flow) => _importManager.Forward(flow.Wrap(command)));
        ReceiveCorrelated<GetImportModelQuery>((query, flow) => _importManager.Forward(flow.Wrap(query)));
        ReceiveCorrelated<BookHoursCommand>((command, flow) => _progressManager.Forward(flow.Wrap(command)));
        ReceiveCorrelated<PersistImportCommand>((cmd, flow, sender) =>
        {
            _importManager.Tell(flow.Wrap(new GetImportModelQuery(cmd.SessionId)), Self);
            Become(() => WaitForModelThenPersist(cmd.SessionId, flow, sender));
        });
    }

    private void WaitForModelThenPersist(Guid sessionId, CorrelationFlow flow, IActorRef originalSender)
    {
        Receive<GetImportModelResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (!result.Found || result.Model is null)
            {
                originalSender.Tell(new PersistImportResult(false, null, "Import session not found or model unavailable."));
                Become(Ready);
                return;
            }

            _dataManager.Tell(flow.WrapChild(new PersistImportWithModelCommand(sessionId, result.Model), "Import.Persist"));
            Become(() => WaitForPersistResult(flow, originalSender));
        });
    }

    private void WaitForPersistResult(CorrelationFlow flow, IActorRef originalSender)
    {
        Receive<PersistImportResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            originalSender.Tell(result);
            Become(Ready);
        });
    }
}
