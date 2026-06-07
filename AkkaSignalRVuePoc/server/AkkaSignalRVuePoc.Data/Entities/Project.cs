using AkkaSignalRVuePoc.Data.Values;

namespace AkkaSignalRVuePoc.Data.Entities;

public sealed class Project
{
    public Guid Id
    {
        get; set;
    }

    public Guid OrganisationId
    {
        get; set;
    }

    public string Name { get; set; } = string.Empty;

    public string? Description
    {
        get; set;
    }

    public DateTimeOffset CreatedAt
    {
        get; set;
    }

    public Hours EstimatedHours { get; set; } = Hours.Zero;

    public Organisation Organisation { get; set; } = null!;
}
