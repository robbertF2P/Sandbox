namespace PlatformConfig.Infrastructure.Persistence.Entities;

public sealed class TenantConfigurationEntity
{
    public Guid TenantId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
