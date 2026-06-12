namespace ApiImportActorPoc.Data.Entities;

public sealed class AssignmentEntity
{
    public Guid Id { get; set; }

    public Guid ActivityId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ActivityEntity Activity { get; set; } = null!;
}
