using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using CommunicationHub.Backend.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommunicationHub.Backend.Core.Search;

/// <summary>
/// Azure AI Search client with mandatory tenant + ACL filtering on every query.
/// The underlying <see cref="SearchClient"/> instances are constructed per index and
/// injected via <see cref="SearchClientFactory"/>.
/// </summary>
public sealed partial class AiSearchClient(
    SearchClientFactory factory,
    IOptions<SearchOptions> options,
    ILogger<AiSearchClient> logger) : ISearchClient
{
    public async Task<List<SourceReference>> SearchInteractionsAsync(
        TenantContext ctx,
        string query,
        int topK = 8,
        CancellationToken ct = default)
    {
        LogSearch(logger, "interactions", query, ctx.TenantId);
        var client = factory.GetClient(options.Value.InteractionsIndex);
        return await ExecuteHybridQueryAsync(client, ctx, query, topK, ct);
    }

    public async Task<List<SourceReference>> SearchBcMasterAsync(
        TenantContext ctx,
        string query,
        int topK = 5,
        CancellationToken ct = default)
    {
        LogSearch(logger, "bc-master", query, ctx.TenantId);
        var client = factory.GetClient(options.Value.BcMasterIndex);
        return await ExecuteHybridQueryAsync(client, ctx, query, topK, ct);
    }

    private static async Task<List<SourceReference>> ExecuteHybridQueryAsync(
        SearchClient client,
        TenantContext ctx,
        string query,
        int topK,
        CancellationToken ct)
    {
        // Mandatory security filter — never trust documents without tenant + company filter.
        var filter = $"tenant_id eq '{EscapeODataString(ctx.TenantId)}'"
                   + $" and company_id eq '{EscapeODataString(ctx.BcCompanyId)}'";

        var searchOptions = new Azure.Search.Documents.SearchOptions
        {
            Filter = filter,
            Size = topK,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = "default",
            },
        };

        // TODO Sprint 1: add vectorized query for true hybrid retrieval.
        var response = await client.SearchAsync<SearchDocument>(query, searchOptions, ct);

        var results = new List<SourceReference>();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(new SourceReference
            {
                Id = result.Document.TryGetValue("id", out var id) ? id?.ToString() ?? string.Empty : string.Empty,
                Title = result.Document.TryGetValue("title", out var title) ? title?.ToString() ?? string.Empty : string.Empty,
                Excerpt = result.Document.TryGetValue("excerpt", out var excerpt) ? excerpt?.ToString() : null,
                SourceType = result.Document.TryGetValue("source_type", out var st) ? st?.ToString() ?? "Email" : "Email",
            });
        }

        return results;
    }

    private static string EscapeODataString(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);

    [LoggerMessage(Level = LogLevel.Debug, Message = "AI Search: index={Index} query={Query} tenant={TenantId}")]
    private static partial void LogSearch(ILogger logger, string index, string query, string tenantId);
}

/// <summary>Factory that creates/caches <see cref="SearchClient"/> per index name.</summary>
public sealed class SearchClientFactory(IOptions<SearchOptions> options, Azure.Core.TokenCredential credential)
{
    private readonly Dictionary<string, SearchClient> _cache = [];
    private readonly SearchOptions _opts = options.Value;

    public SearchClient GetClient(string indexName)
    {
        if (!_cache.TryGetValue(indexName, out var client))
        {
            client = new SearchClient(new Uri(_opts.ServiceEndpoint), indexName, credential);
            _cache[indexName] = client;
        }
        return client;
    }
}

/// <summary>Configuration for <see cref="AiSearchClient"/>.</summary>
public sealed class SearchOptions
{
    public const string Section = "AzureSearch";
    public required string ServiceEndpoint { get; init; }
    public string InteractionsIndex { get; init; } = "interactions";
    public string BcMasterIndex { get; init; } = "bc-master";
}
