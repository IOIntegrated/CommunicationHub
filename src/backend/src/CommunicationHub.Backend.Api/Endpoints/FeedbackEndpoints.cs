using CommunicationHub.Backend.Api.Middleware;
using CommunicationHub.Backend.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationHub.Backend.Api.Endpoints;

public static partial class FeedbackEndpoints
{
    public static void MapFeedbackEndpoints(this WebApplication app)
    {
        app.MapPost("/v1/feedback", SubmitAsync)
            .RequireAuthorization()
            .WithName("SubmitFeedback")
            .WithTags("Feedback")
            .WithSummary("Submit user feedback on an AI suggestion");
    }

    private static async Task<IResult> SubmitAsync(
        [FromBody] FeedbackRequest request,
        HttpContext ctx,
        ILogger<FeedbackRequest> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DecisionId))
            return Results.BadRequest(new { error = "decision_id_required" });

        var tenantCtx = ctx.RequireTenantContext();

        // TODO Sprint 1: persist feedback to App Insights Custom Event + BC audit log.
        LogFeedbackReceived(logger, request.DecisionId, request.Accepted, tenantCtx.TenantId);

        await Task.CompletedTask;
        return Results.NoContent();
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Feedback received decisionId={DecisionId} accepted={Accepted} tenant={TenantId}")]
    private static partial void LogFeedbackReceived(
        ILogger logger, string decisionId, bool accepted, string tenantId);
}
