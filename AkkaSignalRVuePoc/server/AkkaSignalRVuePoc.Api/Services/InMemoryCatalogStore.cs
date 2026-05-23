using System.Collections.Concurrent;
using AkkaSignalRVuePoc.Api.Models;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class InMemoryCatalogStore
{
    private readonly ConcurrentDictionary<Guid, Organisation> _organisations = new();
    private readonly ConcurrentDictionary<Guid, Project> _projects = new();

    public InMemoryCatalogStore()
    {
        var acmeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var drivenItId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var createdAt = DateTimeOffset.UtcNow;

        _organisations[acmeId] = new Organisation(acmeId, "Acme Corp", createdAt);
        _organisations[drivenItId] = new Organisation(drivenItId, "Driven IT", createdAt);

        var websiteId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var platformId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        _projects[websiteId] = new Project(
            websiteId,
            acmeId,
            "Customer Portal",
            "Public-facing web application",
            createdAt);

        _projects[platformId] = new Project(
            platformId,
            drivenItId,
            "Akka SignalR POC",
            "Demonstration of Akka.NET, SignalR, and Vue",
            createdAt);
    }

    public IReadOnlyCollection<Organisation> GetOrganisations() =>
        _organisations.Values.OrderBy(organisation => organisation.Name).ToArray();

    public Organisation? GetOrganisation(Guid id) =>
        _organisations.GetValueOrDefault(id);

    public Organisation CreateOrganisation(CreateOrganisationRequest request)
    {
        var organisation = new Organisation(
            Guid.NewGuid(),
            request.Name.Trim(),
            DateTimeOffset.UtcNow);

        _organisations[organisation.Id] = organisation;
        return organisation;
    }

    public IReadOnlyCollection<Project> GetProjects() =>
        _projects.Values.OrderBy(project => project.Name).ToArray();

    public IReadOnlyCollection<Project> GetProjectsForOrganisation(Guid organisationId) =>
        _projects.Values
            .Where(project => project.OrganisationId == organisationId)
            .OrderBy(project => project.Name)
            .ToArray();

    public Project? GetProject(Guid id) =>
        _projects.GetValueOrDefault(id);

    public Project? CreateProject(CreateProjectRequest request)
    {
        if (!_organisations.ContainsKey(request.OrganisationId))
        {
            return null;
        }

        var project = new Project(
            Guid.NewGuid(),
            request.OrganisationId,
            request.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DateTimeOffset.UtcNow);

        _projects[project.Id] = project;
        return project;
    }
}
