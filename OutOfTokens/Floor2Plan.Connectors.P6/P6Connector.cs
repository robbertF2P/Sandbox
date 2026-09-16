using Akka.Actor;
using Contracts.Infrastructure.Connectors;
using Contracts.Model.Enums;
using Domain.Model;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6.Actors;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Configuration;
using Floor2Plan.Connectors.P6.Messages;
using Infrastructure.Akka.Contracts;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6
{
    public sealed class P6Connector : IGenericConnector
    {
        private static readonly TimeSpan _askTimeout = TimeSpan.FromSeconds(30);

        private readonly IActorSystemFacade _actorSystemFacade;
        private readonly IP6RestApi _api;
        private readonly P6AuthOptions _authOptions;
        private readonly P6SyncOptions _syncOptions;
        private readonly IProcessLogger<P6Connector> _processLogger;
        private readonly IP6ProjectSelectionStore _projectSelectionStore;
        private readonly Lazy<Task<IActorRef>> _actor;

        public P6Connector(
            IActorSystemFacade actorSystemFacade,
            IP6RestApi api,
            IOptions<P6AuthOptions> authOptions,
            IProcessLogger<P6Connector> processLogger,
            IP6ProjectSelectionStore projectSelectionStore,
            IOptions<P6SyncOptions> syncOptions = null)
        {
            _actorSystemFacade = actorSystemFacade;
            _api = api;
            _authOptions = authOptions.Value;
            _processLogger = processLogger;
            _projectSelectionStore = projectSelectionStore;
            _syncOptions = syncOptions?.Value ?? new P6SyncOptions();
            _actor = new Lazy<Task<IActorRef>>(
                () => _actorSystemFacade.RegisterActor(P6Actor.ActorName, P6Actor.Props(_api, _authOptions, _syncOptions)));
        }

        public const string ActiveProjectsGroupKey = "ActiveProjects";

        public string ConfigKey => "P6";

        public ImportType ImportType => ImportType.Planning;

        public string ConfigDescription => "Primavera P6";

        public string ConfigIcon => "img/floor2plan_icon_192.png";

        public string ConfigLogo => "img/floorganise.png";

        public string ConfigRemark => "Initial actor-based P6 connector demonstration.";

        public async Task<ConnectorConfiguration> GetConnectorConfigurationAsync()
        {
            return new ConnectorConfiguration
            {
                Description = ConfigDescription,
                Logo = ConfigLogo,
                Icon = ConfigIcon,
                Remark = ConfigRemark,
                CanSync = true, //DO NOT TOUCH THIS!!
                CanSave = true,
                CanDownloadRawData = true,
                PropertyGroups = [await GetActiveProjectsGroupAsync()]
            };
        }

        public async Task<(MemoryStream memoryStream, string fileName, string mimeType)> GetRawDataAsync(SyncParams syncParams)
        {
            _ = syncParams;

            var actor = await _actor.Value;
            // normally we don't use Ask in actor model world, but because this is an http request, we need an answer for the response, so we use Ask
            var rawData = await actor.Ask<P6RawData>(new GetP6RawData(), _askTimeout, CancellationToken.None);
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

        private static bool IsChecked(ConfigurationProperty property)
        {
            return property.Values?.Any(x => string.Equals(x, "true", StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Builds the checkbox grid listing every P6 project, pre-ticking the ones that were saved before.
        /// The checkbox key is the project's numeric ObjectId (not its business Id/code), because ObjectId
        /// is what the P6 REST API requires for Filter=ProjectObjectId=... during sync. Resolving Id -> ObjectId
        /// happens once here (we already fetch the full project catalog for this screen) so sync itself never
        /// needs an extra lookup call per project.
        /// </summary>
        private async Task<ConfigurationPropertyGroup> GetActiveProjectsGroupAsync()
        {
            var selectedProjectObjectIds = (await _projectSelectionStore.GetSelectedProjectIdsAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var actor = await _actor.Value;
            // normally we don't use Ask in actor model world, but because this is an http request, we need an answer for the response, so we use Ask
            var projects = await actor.Ask<P6Projects>(new GetP6Projects() , CancellationToken.None);

            var properties = projects.Projects
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

        public async Task<SyncResult> SyncAllAsync(IEnumerable<ConfigurationPropertyGroup> propertyGroups)
        {
            _ = propertyGroups;
            // The selection store persists P6 project ObjectIds (resolved once at config-save time), so no
            // extra lookup call is needed here - these values are passed straight through to the actor.
            var selectedProjectObjectIds = (await _projectSelectionStore.GetSelectedProjectIdsAsync()).ToArray();
            var actor = await _actor.Value;
            var syncResult = await actor.Ask<P6SyncResult>(
                new StartP6Sync(selectedProjectObjectIds),
                _syncOptions.RequestTimeout,
                CancellationToken.None);
            LogSyncMetrics(syncResult);
            if (!syncResult.Succeeded)
            {
                throw new InvalidOperationException($"P6 synchronization failed: {string.Join("; ", syncResult.Errors)}");
            }

            return new SyncResult();
        }

        private void LogSyncMetrics(P6SyncResult syncResult)
        {
            _processLogger.LogRange(new[]
            {
                new SyncLogMessageDto("Projects retrieved from P6", SyncInformation.Total, SyncType.Project, syncResult.ProjectCount),
                new SyncLogMessageDto("WBS elements retrieved from P6", SyncInformation.Total, SyncType.Component, syncResult.WbsCount),
                new SyncLogMessageDto("Activities retrieved from P6", SyncInformation.Total, SyncType.Activity, syncResult.ActivityCount),
                new SyncLogMessageDto("Resources retrieved from P6", SyncInformation.Total, SyncType.Discipline, syncResult.ResourceCount),
                new SyncLogMessageDto("Resource assignments retrieved from P6", SyncInformation.Total, SyncType.Assignment, syncResult.ResourceAssignmentCount),
                new SyncLogMessageDto("Relationships retrieved from P6", SyncInformation.Total, SyncType.ActivityRelation, syncResult.RelationshipCount)
            });

            _processLogger.Log(new SyncLogMessageDto(
                $"P6 sync retrieved {syncResult.TotalRecordCount} records in total.",
                syncResult.Succeeded ? SyncInformation.Success : SyncInformation.Fail,
                SyncType.Unknown));

            foreach (var error in syncResult.Errors)
            {
                _processLogger.Log(new SyncLogMessageDto(error, SyncInformation.Fail, SyncType.Unknown));
            }
        }
    }
}
