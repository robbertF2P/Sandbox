using Reference.Application.Ports;

namespace Reference.Infrastructure.Queries;

public sealed class ReferenceStatusQuery : IReferenceStatusQuery
{
    public Task<ReferenceStatusSnapshot> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReferenceStatusSnapshot(
            ModuleName: "Reference",
            ModuleRegistered: true,
            StranglerAdapterPresent: false,
            CheckedAtUtc: DateTimeOffset.UtcNow));
    }
}
