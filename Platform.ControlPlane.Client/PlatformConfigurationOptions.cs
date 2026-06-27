namespace Platform.ControlPlane.Client;

public sealed class PlatformConfigurationOptions
{
    public const string SectionName = "Platform";

    public string BaseUrl { get; set; } = string.Empty;

    public string ConfigurationApiKey { get; set; } = string.Empty;
}
