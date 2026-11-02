using System.ComponentModel;
using AiBasics.Core.Services;
using ModelContextProtocol.Server;

namespace DocsSearch.Mcp.Tools;

[McpServerToolType]
public sealed class DocsSearchMcpTools(
    SemanticSearchService searchService,
    RagService ragService,
    IndexingService indexingService)
{
    private readonly SemanticSearchService _searchService = searchService;
    private readonly RagService _ragService = ragService;
    private readonly IndexingService _indexingService = indexingService;

    [McpServerTool]
    [Description("Re-index SandBox docs and agent skills into the in-memory vector store.")]
    public async Task<string> IndexDocs(CancellationToken cancellationToken)
    {
        var count = await _indexingService.IndexCorpusAsync(cancellationToken);
        return $"Indexed {count} documentation chunks from docs/ and .cursor/skills/.";
    }

    [McpServerTool]
    [Description("Semantic search over SandBox documentation and agent skills.")]
    public Task<string> SearchDocs(
        [Description("Natural language search query.")]
        string query,
        CancellationToken cancellationToken) =>
        _searchService.FormatResultsAsync(query, topK: 5, cancellationToken);

    [McpServerTool]
    [Description("Ask a question grounded in SandBox documentation using RAG.")]
    public Task<string> AskDocs(
        [Description("Question about SandBox conventions, skills, or architecture.")]
        string question,
        CancellationToken cancellationToken) =>
        _ragService.FormatAnswerAsync(question, cancellationToken);
}
