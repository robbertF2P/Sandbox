using Contracts.Model.Enums;
using Domain.Model.Sync;

namespace Contracts.Infrastructure.Connectors;

public interface IGenericConnector
{
    string ConfigKey { get; }

    ImportType ImportType { get; }

    string ConfigDescription { get; }

    string ConfigIcon { get; }

    string ConfigLogo { get; }

    string ConfigRemark { get; }

    Task<ConnectorConfiguration> GetConnectorConfigurationAsync();

    Task<(MemoryStream memoryStream, string fileName, string mimeType)> GetRawDataAsync(SyncParams syncParams);

    Task SaveConnectorConfigurationAsync(ConnectorConfiguration connectorConfiguration);

    Task<SyncResult> SyncAllAsync(IEnumerable<ConfigurationPropertyGroup> propertyGroups);
}
