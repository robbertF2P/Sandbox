using Reference.Application.Ports;

namespace Reference.Infrastructure.Adapters;

/// <summary>
/// Example strangler adapter — delegates to legacy until parity is proven.
/// Removal ticket: F2P-0000-reference-legacy-shim
/// </summary>
[StranglerAdapter("F2P-0000-reference-legacy-shim")]
public sealed class LegacyReferenceStatusAdapter : IReferenceStatusQuery
{
    public Task<ReferenceStatusSnapshot> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReferenceStatusSnapshot(
            ModuleName: "Reference",
            ModuleRegistered: true,
            StranglerAdapterPresent: true,
            CheckedAtUtc: DateTimeOffset.UtcNow));
    }
}
