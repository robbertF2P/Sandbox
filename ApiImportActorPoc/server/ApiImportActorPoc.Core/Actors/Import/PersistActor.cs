using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Actors.Import;

public sealed class PersistActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IDbContextFactory<ImportDbContext> _dbContextFactory;

    public PersistActor(IDbContextFactory<ImportDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        ReceiveAsync<PersistImportWithModelCommand>(HandlePersistAsync);
    }

    public static Props Props(IDbContextFactory<ImportDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new PersistActor(dbContextFactory));

    private async Task HandlePersistAsync(PersistImportWithModelCommand command)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var entity = ProjectEntityMapper.ToEntity(command.Model);
            db.Projects.Add(entity);
            await db.SaveChangesAsync();

            Context.System.EventStream.Publish(new ImportPersisted(command.SessionId, entity.Id, DateTimeOffset.UtcNow));
            Sender.Tell(new PersistImportResult(true, entity.Id, null));
            _log.Info("Persisted import session {0} as project {1}", command.SessionId, entity.Id);
        }
        catch (Exception exception)
        {
            _log.Error(exception, "Failed to persist import session {0}", command.SessionId);
            Sender.Tell(new PersistImportResult(false, null, exception.Message));
        }
    }
}
