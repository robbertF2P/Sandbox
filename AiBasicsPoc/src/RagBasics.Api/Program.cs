using AiBasics.Core.DependencyInjection;
using AiBasics.Core.Services;
using AiBasics.Core.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAiBasicsCore(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapPost("/ingest", async (IngestRequest request, IndexingService indexing, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest("Content is required.");
    }

    var source = string.IsNullOrWhiteSpace(request.SourcePath) ? "manual" : request.SourcePath;
    await indexing.IndexTextAsync(request.Content, source, ct);
    return Results.Ok(new { message = "Text indexed." });
});

app.MapPost("/ingest/corpus", async (IndexingService indexing, CancellationToken ct) =>
{
    var count = await indexing.IndexCorpusAsync(ct);
    return Results.Ok(new { indexed = count });
});

app.MapGet("/ask", async (string query, RagService rag, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.BadRequest("Query is required.");
    }

    var answer = await rag.AskAsync(query, cancellationToken: ct);
    return Results.Ok(answer);
});

app.MapGet("/stats", async (IVectorStore store, CancellationToken ct) =>
    Results.Ok(new { chunks = await store.CountAsync(ct) }));

app.Run();

internal sealed record IngestRequest(string Content, string? SourcePath);
