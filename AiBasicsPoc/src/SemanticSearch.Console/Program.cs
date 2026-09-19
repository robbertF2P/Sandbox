using AiBasics.Core.DependencyInjection;
using AiBasics.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddAiBasicsCore(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var indexing = scope.ServiceProvider.GetRequiredService<IndexingService>();
var search = scope.ServiceProvider.GetRequiredService<SemanticSearchService>();

Console.WriteLine("Indexing SandBox documentation corpus...");
var count = await indexing.IndexCorpusAsync();
Console.WriteLine($"Indexed {count} chunks.");
Console.WriteLine("Enter a semantic search query (empty line to exit).");

while (true)
{
    Console.Write("\nQuery: ");
    var query = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(query))
    {
        break;
    }

    var output = await search.FormatResultsAsync(query, topK: 3);
    Console.WriteLine();
    Console.WriteLine(output);
}
