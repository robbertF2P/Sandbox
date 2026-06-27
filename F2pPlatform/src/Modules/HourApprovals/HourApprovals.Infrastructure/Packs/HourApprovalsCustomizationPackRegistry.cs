using HourApprovals.Application.Ports;

namespace HourApprovals.Infrastructure.Packs;

public interface IHourApprovalsCustomizationPackRegistry
{
    IHourApprovalsCustomizationPack Resolve(string packId);
}

public interface ICustomizationPackRegistryBuilder
{
    ICustomizationPackRegistryBuilder AddPack(IHourApprovalsCustomizationPack pack);

    ICustomizationPackRegistryBuilder AddPack<TPack>() where TPack : class, IHourApprovalsCustomizationPack, new();
}

internal sealed class CustomizationPackRegistryBuilder : ICustomizationPackRegistryBuilder
{
    private readonly List<IHourApprovalsCustomizationPack> _packs = [];

    public ICustomizationPackRegistryBuilder AddPack(IHourApprovalsCustomizationPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        _packs.Add(pack);
        return this;
    }

    public ICustomizationPackRegistryBuilder AddPack<TPack>()
        where TPack : class, IHourApprovalsCustomizationPack, new()
    {
        _packs.Add(new TPack());
        return this;
    }

    internal HourApprovalsCustomizationPackRegistry Build() =>
        new(_packs);
}

internal sealed class HourApprovalsCustomizationPackRegistry(IEnumerable<IHourApprovalsCustomizationPack> packs)
    : IHourApprovalsCustomizationPackRegistry
{
    private readonly Dictionary<string, IHourApprovalsCustomizationPack> _packsById =
        packs.ToDictionary(pack => pack.PackId, StringComparer.OrdinalIgnoreCase);

    private readonly IHourApprovalsCustomizationPack _defaultPack =
        packs.FirstOrDefault(pack => pack.PackId == DefaultHourApprovalsPack.DefaultPackId)
        ?? throw new InvalidOperationException("Default hour-approvals customization pack must be registered.");

    public IHourApprovalsCustomizationPack Resolve(string packId) =>
        string.IsNullOrWhiteSpace(packId) || !_packsById.TryGetValue(packId, out IHourApprovalsCustomizationPack? pack)
            ? _defaultPack
            : pack;
}
