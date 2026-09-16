using Contracts.Model.Enums;

namespace Contracts.Infrastructure.Connectors;

public sealed class ConfigurationProperty
{
    public InputType InputType { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IList<string> Values { get; set; } = [];

    public IList<ConfigurationListItem> Options { get; set; } = [];
}
