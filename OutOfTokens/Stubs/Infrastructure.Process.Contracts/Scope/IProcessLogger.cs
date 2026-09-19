using Domain.Model.Sync;

namespace Infrastructure.Process.Contracts.Scope;

public interface IProcessLogger
{
    void Log(SyncLogMessageDto message);

    void LogRange(IEnumerable<SyncLogMessageDto> messages);
}

public interface IProcessLogger<T> : IProcessLogger
{
}
