namespace AkkaSignalRVuePoc.Data.Entities;

public sealed class Organisation
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Project> Projects { get; set; } = [];
}
