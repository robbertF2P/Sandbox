using AiBasics.Core.Corpus;
using AiBasics.Core.Models;
using AiBasics.Core.Storage;
using Microsoft.Extensions.AI;

namespace AiBasics.Core.Services;

public sealed class IndexingService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStore vectorStore,
    DocumentCorpus corpus)
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
    private readonly IVectorStore _vectorStore = vectorStore;
    private readonly DocumentCorpus _corpus = corpus;

    public async Task<int> IndexTextAsync(string content, string sourcePath, CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddingGenerator.GenerateAsync(content, cancellationToken: cancellationToken);
        var id = $"{sourcePath}#{Guid.NewGuid():N}";

        await _vectorStore.UpsertAsync(
            new DocumentChunk(id, sourcePath, content, embedding.Vector),
            cancellationToken);

        return 1;
    }

    public async Task<int> IndexCorpusAsync(CancellationToken cancellationToken = default)
    {
        await _vectorStore.ClearAsync(cancellationToken);
        var documents = _corpus.LoadDocuments();
        var indexed = 0;

        foreach (var document in documents)
        {
            var embedding = await _embeddingGenerator.GenerateAsync(
                document.Content,
                cancellationToken: cancellationToken);

            await _vectorStore.UpsertAsync(
                new DocumentChunk(document.Id, document.SourcePath, document.Content, embedding.Vector),
                cancellationToken);

            indexed++;
        }

        return indexed;
    }
}
