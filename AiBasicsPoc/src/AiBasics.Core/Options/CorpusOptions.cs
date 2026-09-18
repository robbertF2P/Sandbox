namespace AiBasics.Core.Options;

public sealed class CorpusOptions
{
    public const string SectionName = "Corpus";

    /// <summary>
    /// Repository root containing docs/ and .cursor/skills/. When empty, walks up from cwd.
    /// </summary>
    public string? RootPath { get; set; }

    public IReadOnlyList<string> IncludeGlobs { get; set; } =
        ["docs/**/*.md", ".cursor/skills/**/SKILL.md"];
}
