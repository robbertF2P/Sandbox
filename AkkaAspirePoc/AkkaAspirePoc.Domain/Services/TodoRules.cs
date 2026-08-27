namespace AkkaAspirePoc.Domain.Services;

public static class TodoRules
{
    public const int MaxTitleLength = 200;

    public static bool IsValidTitle(string? title) =>
        !string.IsNullOrWhiteSpace(title) && title.Trim().Length <= MaxTitleLength;
}
