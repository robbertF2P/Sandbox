using AiBasics.Core.Corpus;
using AiBasics.Core.Embeddings;
using AiBasics.Core.Options;
using AiBasics.Core.Services;
using AiBasics.Core.Storage;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace AiBasics.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiBasicsCore(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useDeterministicEmbeddings = false)
    {
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<CorpusOptions>(configuration.GetSection(CorpusOptions.SectionName));

        services.AddSingleton<IVectorStore, InMemoryVectorStore>();
        services.AddSingleton<DocumentCorpus>();

        if (useDeterministicEmbeddings)
        {
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, DeterministicEmbeddingGenerator>();
        }
        else
        {
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
                return new OllamaApiClient(new Uri(options.Url), options.EmbeddingModel);
            });
        }

        services.AddHttpClient<OllamaChatClient>();
        services.AddSingleton<IndexingService>();
        services.AddSingleton<SemanticSearchService>();
        services.AddSingleton<RagService>();

        return services;
    }
}
