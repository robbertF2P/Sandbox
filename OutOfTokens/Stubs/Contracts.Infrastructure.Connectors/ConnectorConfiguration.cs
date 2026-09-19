namespace Contracts.Infrastructure.Connectors;

public sealed class ConnectorConfiguration
{
    public string Description { get; set; } = string.Empty;

    public string Logo { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public bool CanSync { get; set; }

    public bool CanSave { get; set; }

    public bool CanDownloadRawData { get; set; }

    public IList<ConfigurationPropertyGroup> PropertyGroups { get; set; } = [];
}
