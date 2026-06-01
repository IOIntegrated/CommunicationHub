using CommunicationHub.Backend.Core.Models;

namespace CommunicationHub.Backend.Core.Search;

/// <summary>Contract for Azure AI Search queries used by the Copilot API.</summary>
public interface ISearchClient
{
    /// <summary>
    /// Hybrid search (keyword + vector) over the interactions index.
    /// Results are always filtered by tenantId + companyId + visibility ACLs from
    /// <paramref name="ctx"/> — caller should never trust the raw index without these filters.
    /// </summary>
    Task<List<SourceReference>> SearchInteractionsAsync(
        TenantContext ctx,
        string query,
        int topK = 8,
        CancellationToken ct = default);

    /// <summary>Hybrid search over the bc-master index (customers, contacts, items).</summary>
    Task<List<SourceReference>> SearchBcMasterAsync(
        TenantContext ctx,
        string query,
        int topK = 5,
        CancellationToken ct = default);
}
