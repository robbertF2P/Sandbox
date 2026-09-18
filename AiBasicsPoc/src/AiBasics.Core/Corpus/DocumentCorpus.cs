using System.Text;
using AiBasics.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiBasics.Core.Corpus;

public sealed class DocumentCorpus
{
    private readonly CorpusOptions _options;
    private readonly ILogger<DocumentCorpus> _logger;

    public DocumentCorpus(IOptions<CorpusOptions> options, ILogger<DocumentCorpus> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public IReadOnlyList<CorpusDocument> LoadDocuments()
    {
        var root = ResolveRootPath();
        var documents = new List<CorpusDocument>();

        foreach (var pattern in _options.IncludeGlobs)
        {
            var fullPattern = Path.Combine(root, pattern);
            var directory = Path.GetDirectoryName(fullPattern) ?? root;
            var searchPattern = Path.GetFileName(fullPattern);

            if (!Directory.Exists(directory))
            {
                _logger.LogDebug("Skipping missing corpus directory {Directory}", directory);
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(root, file);
                var content = File.ReadAllText(file, Encoding.UTF8);

                foreach (var chunk in ChunkMarkdown(relativePath, content))
                {
                    documents.Add(chunk);
                }
            }
        }

        _logger.LogInformation("Loaded {Count} corpus chunks from {Root}", documents.Count, root);
        return documents;
    }

    private string ResolveRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_options.RootPath))
        {
            return Path.GetFullPath(_options.RootPath);
        }

        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repository root (AGENTS.md). Set Corpus:RootPath in configuration.");
    }

    private static IEnumerable<CorpusDocument> ChunkMarkdown(string sourcePath, string content)
    {
        const int maxChunkLength = 1200;
        var paragraphs = content
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var buffer = new StringBuilder();
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            if (buffer.Length + paragraph.Length > maxChunkLength && buffer.Length > 0)
            {
                yield return CreateChunk(sourcePath, chunkIndex++, buffer.ToString());
                buffer.Clear();
            }

            if (buffer.Length > 0)
            {
                buffer.AppendLine();
                buffer.AppendLine();
            }

            buffer.Append(paragraph);
        }

        if (buffer.Length > 0)
        {
            yield return CreateChunk(sourcePath, chunkIndex, buffer.ToString());
        }
    }

    private static CorpusDocument CreateChunk(string sourcePath, int index, string content) =>
        new($"{sourcePath}#{index}", sourcePath, content.Trim());
}

public sealed record CorpusDocument(string Id, string SourcePath, string Content);
