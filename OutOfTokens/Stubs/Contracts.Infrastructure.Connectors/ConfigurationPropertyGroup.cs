using Contracts.Model.Enums;

namespace Contracts.Infrastructure.Connectors;

public sealed class ConfigurationPropertyGroup
{
    public string Key { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public InputGroupType InputGroupType { get; set; }

    public string NameHeader { get; set; } = string.Empty;

    public int NameWidth { get; set; }

    public string DescriptionHeader { get; set; } = string.Empty;

    public IList<ConfigurationProperty> Properties { get; set; } = [];
}
