namespace Reference.Application.Ports;

public interface IReferenceStatusQuery
{
    Task<ReferenceStatusSnapshot> GetStatusAsync(CancellationToken cancellationToken = default);
}

public sealed record ReferenceStatusSnapshot(
    string ModuleName,
    bool ModuleRegistered,
    bool StranglerAdapterPresent,
    DateTimeOffset CheckedAtUtc);
