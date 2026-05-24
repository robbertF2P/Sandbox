using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Data;

public static class CatalogSeedData
{
    public static readonly Guid AcmeOrganisationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DrivenItOrganisationId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CustomerPortalProjectId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid AkkaPocProjectId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly DateTimeOffset SeedCreatedAt =
        new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organisation>().HasData(
            new Organisation
            {
                Id = AcmeOrganisationId,
                Name = "Acme Corp",
                CreatedAt = SeedCreatedAt
            },
            new Organisation
            {
                Id = DrivenItOrganisationId,
                Name = "Driven IT",
                CreatedAt = SeedCreatedAt
            });

        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = CustomerPortalProjectId,
                OrganisationId = AcmeOrganisationId,
                Name = "Customer Portal",
                Description = "Public-facing web application",
                CreatedAt = SeedCreatedAt
            },
            new Project
            {
                Id = AkkaPocProjectId,
                OrganisationId = DrivenItOrganisationId,
                Name = "Akka SignalR POC",
                Description = "Demonstration of Akka.NET, SignalR, and Vue",
                CreatedAt = SeedCreatedAt
            });
    }
}
