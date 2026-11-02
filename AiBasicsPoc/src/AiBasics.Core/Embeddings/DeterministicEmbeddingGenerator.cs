using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;

namespace AiBasics.Core.Embeddings;

/// <summary>
/// Hash-based embeddings for unit tests — no Ollama required.
/// </summary>
public sealed class DeterministicEmbeddingGenerator(int dimensions = 64) : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly int _dimensions = dimensions;

    public EmbeddingGeneratorMetadata Metadata { get; } = new("deterministic");

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var embeddings = values
            .Select(v => new Embedding<float>(Embed(v)))
            .ToList();

        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }

    private float[] Embed(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text.ToLowerInvariant()));
        var vector = new float[_dimensions];

        for (var i = 0; i < _dimensions; i++)
        {
            vector[i] = (bytes[i % bytes.Length] / 255f) - 0.5f;
        }

        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));
        if (magnitude > 0)
        {
            for (var i = 0; i < vector.Length; i++)
            {
                vector[i] /= magnitude;
            }
        }

        return vector;
    }
}
