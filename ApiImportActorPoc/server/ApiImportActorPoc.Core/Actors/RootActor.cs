using Akka.Actor;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Core.Actors.Data;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Actors;

public sealed class RootActor : ReceiveActor
{
    private readonly IDbContextFactory<ImportDbContext> _dbContextFactory;
    private IActorRef _importManager = ActorRefs.Nobody;
    private IActorRef _dataManager = ActorRefs.Nobody;

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
    }

    private void Ready()
    {
        Receive<StartImportCommand>(_importManager.Forward);
        Receive<GetImportModelQuery>(_importManager.Forward);
        Receive<PersistImportCommand>(cmd =>
        {
            _importManager.Tell(new GetImportModelQuery(cmd.SessionId));
            Become(() => WaitForModelThenPersist(cmd.SessionId, Sender));
        });
    }

    private void WaitForModelThenPersist(Guid sessionId, IActorRef originalSender)
    {
        Receive<GetImportModelResult>(result =>
        {
            if (!result.Found || result.Model is null)
            {
                originalSender.Tell(new PersistImportResult(false, null, "Import session not found or model unavailable."));
                Become(Ready);
                return;
            }

            _dataManager.Tell(new PersistImportWithModelCommand(sessionId, result.Model));
            Become(() => WaitForPersistResult(originalSender));
        });
    }

    private void WaitForPersistResult(IActorRef originalSender)
    {
        Receive<PersistImportResult>(result =>
        {
            originalSender.Tell(result);
            Become(Ready);
        });
    }
}
