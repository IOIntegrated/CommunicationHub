using CommunicationHub.Backend.Api.Middleware;
using CommunicationHub.Backend.Core.Auth;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Search;

namespace CommunicationHub.Backend.Api.Endpoints;

public static class ContextEndpoints
{
    public static void MapContextEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/context")
            .RequireAuthorization()
            .WithTags("Context");

        group.MapGet("/customer/{customerNo}", GetCustomerContextAsync)
            .WithName("GetCustomerContext")
            .WithSummary("Aggregated customer context: stamm, recent interactions, AI summary");

        // TODO Sprint 1: add /contact/{no} and /project/{no}
    }

    private static async Task<IResult> GetCustomerContextAsync(
        string customerNo,
        HttpContext ctx,
        IPermissionResolver permissions,
        IBcApiClient bcClient,
        ISearchClient searchClient,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(customerNo))
            return Results.BadRequest(new { error = "customer_no_required" });

        var tenantCtx = ctx.RequireTenantContext();

        var canView = await permissions.CanViewCustomerContextAsync(tenantCtx, customerNo, ct);
        if (!canView)
            return Results.Json(new { error = "permission_denied" }, statusCode: StatusCodes.Status403Forbidden);

        // Fetch BC context and recent interactions from Search in parallel.
        var bcContextTask = bcClient.GetCustomerContextAsync(tenantCtx, customerNo, ct);
        var searchTask = searchClient.SearchInteractionsAsync(tenantCtx, $"customer {customerNo}", topK: 5, ct);

        await Task.WhenAll(bcContextTask, searchTask);

        var bcCtx = bcContextTask.Result;
        var searchResults = searchTask.Result;
        var canViewSummary = await permissions.CanViewAiSummaryAsync(tenantCtx, customerNo, ct);

        // Merge search results into the BC context (class has no `with` — build a new instance).
        var merged = new CommunicationHub.Backend.Core.Models.CustomerContextResult
        {
            CustomerNo = bcCtx.CustomerNo,
            Name = bcCtx.Name,
            RecentInteractions = [.. bcCtx.RecentInteractions, .. searchResults],
            RecentDocuments = bcCtx.RecentDocuments,
            OpenActionItems = bcCtx.OpenActionItems,
            AiSummary = canViewSummary ? bcCtx.AiSummary : null,
        };

        return Results.Ok(merged);
    }
}
