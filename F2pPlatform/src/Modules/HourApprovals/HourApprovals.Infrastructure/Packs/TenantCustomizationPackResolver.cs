using HourApprovals.Application.Ports;
using Microsoft.Extensions.Configuration;
using PlatformConfig.Application.Ports;
using Platform.Shared.View;

namespace HourApprovals.Infrastructure.Packs;

public sealed class TenantCustomizationPackResolver(
    IHourApprovalsCustomizationPackRegistry registry,
    ITenantRuntimeContext tenantRuntimeContext,
    IConfiguration configuration) : IHourApprovalsCustomizationPack
{
    public string PackId => ResolveActivePack().PackId;

    public ViewDefinition GetView(string screenId) => ResolveActivePack().GetView(screenId);

    public IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> taskIds) =>
        ResolveActivePack().GetRowExtensions(taskIds);

    private IHourApprovalsCustomizationPack ResolveActivePack() =>
        registry.Resolve(ResolveActivePackId());

    private string ResolveActivePackId()
    {
        string? fromTenant = tenantRuntimeContext.Current?
            .PackEntitlements
            .CustomizationPacks
            .FirstOrDefault(static packId => !string.IsNullOrWhiteSpace(packId));

        if (!string.IsNullOrWhiteSpace(fromTenant))
        {
            return fromTenant;
        }

        string? fromConfig = configuration
            .GetSection("Tenant:PackEntitlements:customizationPacks")
            .Get<string[]>()?
            .FirstOrDefault(static packId => !string.IsNullOrWhiteSpace(packId));

        return string.IsNullOrWhiteSpace(fromConfig)
            ? DefaultHourApprovalsPack.DefaultPackId
            : fromConfig;
    }
}
