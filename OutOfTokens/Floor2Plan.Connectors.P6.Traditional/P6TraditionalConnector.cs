using Contracts.Infrastructure.Connectors;
using Contracts.Model.Enums;
using Domain.Model;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Configuration;
using Floor2Plan.Connectors.P6.Messages;
using Floor2Plan.Connectors.P6.Sync;
using Floor2Plan.Connectors.P6.Traditional.Services;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Traditional
{
    /// <summary>
    /// Traditional OOP implementation of the P6 connector — same Refit client and sync plans as the actor version.
    /// </summary>
    public sealed class P6TraditionalConnector : IGenericConnector
    {
        private readonly P6SyncOptions _syncOptions;
        private readonly IProcessLogger<P6TraditionalConnector> _processLogger;
        private readonly IP6ProjectSelectionStore _projectSelectionStore;
        private readonly P6SyncService _syncService;
        private readonly P6RawDataExporter _rawDataExporter;
        private readonly P6PagedCatalogReader _catalogReader;
        private readonly P6SessionService _sessionService;

        public P6TraditionalConnector(
            P6SyncService syncService,
            P6RawDataExporter rawDataExporter,
            P6PagedCatalogReader catalogReader,
            P6SessionService sessionService,
            IProcessLogger<P6TraditionalConnector> processLogger,
            IP6ProjectSelectionStore projectSelectionStore,
            IOptions<P6SyncOptions> syncOptions = null)
        {
            _syncService = syncService;
            _rawDataExporter = rawDataExporter;
            _catalogReader = catalogReader;
            _sessionService = sessionService;
            _processLogger = processLogger;
            _projectSelectionStore = projectSelectionStore;
            _syncOptions = syncOptions?.Value ?? new P6SyncOptions();
        }

        public const string ActiveProjectsGroupKey = "ActiveProjects";

        public string ConfigKey => "P6-Traditional";

        public ImportType ImportType => ImportType.Planning;

        public string ConfigDescription => "Primavera P6 (traditional OOP)";

        public string ConfigIcon => "img/floor2plan_icon_192.png";

        public string ConfigLogo => "img/floorganise.png";

        public string ConfigRemark => "Traditional async/await P6 connector for comparison with the actor pipeline.";

        public async Task<ConnectorConfiguration> GetConnectorConfigurationAsync()
        {
            return new ConnectorConfiguration
            {
                Description = ConfigDescription,
                Logo = ConfigLogo,
                Icon = ConfigIcon,
                Remark = ConfigRemark,
                CanSync = true,
                CanSave = true,
                CanDownloadRawData = true,
                PropertyGroups = [await GetActiveProjectsGroupAsync()]
            };
        }

        public async Task<(MemoryStream memoryStream, string fileName, string mimeType)> GetRawDataAsync(SyncParams syncParams)
        {
            _ = syncParams;
            var rawData = await _rawDataExporter.ExportAsync(CancellationToken.None);
            return (rawData.Content, rawData.FileName, rawData.MimeType);
        }

        public Task SaveConnectorConfigurationAsync(ConnectorConfiguration connectorConfiguration)
        {
            var selectedProjectIds = connectorConfiguration?.PropertyGroups
                .FirstOrDefault(x => x.Key == ActiveProjectsGroupKey)?
                .Properties?
                .OfType<ConfigurationProperty>()
                .Where(IsChecked)
                .Select(x => x.Name)
                ?? [];

            return _projectSelectionStore.SaveSelectedProjectIdsAsync(selectedProjectIds);
        }

        public async Task<SyncResult> SyncAllAsync(IEnumerable<ConfigurationPropertyGroup> propertyGroups)
        {
            _ = propertyGroups;
            var selectedProjectObjectIds = (await _projectSelectionStore.GetSelectedProjectIdsAsync()).ToArray();
            if (selectedProjectObjectIds.Length == 0)
            {
                throw new InvalidOperationException("No projects selected to sync.");
            }

            var syncPlans = P6SyncPlanFactory.FromSyncOptions(selectedProjectObjectIds, _syncOptions);
            if (!_syncService.TryQueueSync(selectedProjectObjectIds, syncPlans, out var rejectionReason))
            {
                throw new InvalidOperationException(rejectionReason);
            }

            _processLogger.Log(new SyncLogMessageDto(
                $"P6 sync queued for {selectedProjectObjectIds.Length} project(s). Progress is written to the sync log as catalogs are fetched.",
                SyncInformation.Information,
                SyncType.Unknown));

            return new SyncResult();
        }

        private static bool IsChecked(ConfigurationProperty property)
        {
            return property.Values?.Any(x => string.Equals(x, "true", StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        private async Task<ConfigurationPropertyGroup> GetActiveProjectsGroupAsync()
        {
            var selectedProjectObjectIds = (await _projectSelectionStore.GetSelectedProjectIdsAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            await using var session = await _sessionService.OpenSessionAsync(CancellationToken.None);
            var projects = await _catalogReader.FetchProjectsAsync(session.Cookie, CancellationToken.None);

            var properties = projects
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .Select(project => new ConfigurationProperty
                {
                    InputType = InputType.Checkbox,
                    Name = project.ObjectId.ToString(),
                    Description = $"{project.Id} - {project.Name}",
                    Values = [selectedProjectObjectIds.Contains(project.ObjectId.ToString()).ToString()],
                    Options =
                    [
                        new ConfigurationListItem { Description = "Status", Value = project.Status },
                        new ConfigurationListItem { Description = "Activities", Value = project.SummaryActivityCount?.ToString() },
                        new ConfigurationListItem { Description = "Data date", Value = project.DataDate }
                    ]
                })
                .ToList();

            return new ConfigurationPropertyGroup
            {
                Key = ActiveProjectsGroupKey,
                Description = "Projects to sync",
                InputGroupType = InputGroupType.GridCheckbox,
                NameHeader = "Project",
                NameWidth = 200,
                DescriptionHeader = "Name",
                Properties = properties
            };
        }
    }
}
