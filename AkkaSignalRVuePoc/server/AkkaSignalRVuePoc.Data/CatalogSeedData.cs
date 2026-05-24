namespace AkkaSignalRVuePoc.Data;

public static class CatalogSeedData
{
    public static readonly Guid AcmeOrganisationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DrivenItOrganisationId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CustomerPortalProjectId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid AkkaPocProjectId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly DateTimeOffset SeedCreatedAt =
        new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
