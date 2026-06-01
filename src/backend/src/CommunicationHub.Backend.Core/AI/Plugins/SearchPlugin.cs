using System.ComponentModel;
using CommunicationHub.Backend.Core.Models;
using CommunicationHub.Backend.Core.Search;
using Microsoft.SemanticKernel;

namespace CommunicationHub.Backend.Core.AI.Plugins;

/// <summary>SK Plugin that wraps Azure AI Search queries.</summary>
public sealed class SearchPlugin(ISearchClient searchClient, TenantContext tenantContext)
{
    [KernelFunction, Description("Search the interactions history for context relevant to a query string. Returns JSON array of source references.")]
    public async Task<string> SearchInteractions(
        [Description("Natural language query")] string query,
        [Description("Maximum number of results to return (default 8)")] int topK = 8,
        CancellationToken cancellationToken = default)
    {
        var results = await searchClient.SearchInteractionsAsync(tenantContext, query, topK, cancellationToken);
        return System.Text.Json.JsonSerializer.Serialize(results);
    }

    [KernelFunction, Description("Search BC master data (customers, contacts, items) for context relevant to a query string.")]
    public async Task<string> SearchBcMaster(
        [Description("Natural language query")] string query,
        [Description("Maximum number of results to return (default 5)")] int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var results = await searchClient.SearchBcMasterAsync(tenantContext, query, topK, cancellationToken);
        return System.Text.Json.JsonSerializer.Serialize(results);
    }
}
