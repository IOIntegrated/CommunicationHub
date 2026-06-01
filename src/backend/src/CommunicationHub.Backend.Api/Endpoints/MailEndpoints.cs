using CommunicationHub.Backend.Api.Middleware;
using CommunicationHub.Backend.Core.AI;
using CommunicationHub.Backend.Core.Auth;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationHub.Backend.Api.Endpoints;

public static class MailEndpoints
{
    public static void MapMailEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/mail")
            .RequireAuthorization()
            .WithTags("Mail");

        group.MapPost("/analyze", AnalyzeAsync)
            .WithName("AnalyzeMail")
            .WithSummary("Analyse a mail message (classify, extract, reply suggestion)");
    }

    private static async Task<IResult> AnalyzeAsync(
        [FromBody] MailAnalysisRequest request,
        HttpContext ctx,
        ICopilotOrchestrator orchestrator,
        IPermissionResolver permissions,
        IBcApiClient bcClient,
        CancellationToken ct)
    {
        // Validate input at the boundary.
        if (string.IsNullOrWhiteSpace(request.MessageId))
            return Results.BadRequest(new { error = "message_id_required" });

        var tenantCtx = ctx.RequireTenantContext();

        // Pre-AI permission check (L5).
        var allowed = await permissions.CanTriggerAiAsync(tenantCtx, "Mail", request.MessageId, ct);
        if (!allowed)
        {
            await SafeAuditAsync(bcClient, tenantCtx, "permission.denied", "mail analyze denied", ct);
            return Results.Json(new { error = "permission_denied" }, statusCode: StatusCodes.Status403Forbidden);
        }

        try
        {
            var result = await orchestrator.AnalyzeMailAsync(tenantCtx, request, ct);
            await SafeAuditAsync(bcClient, tenantCtx, "mail.analysis.completed", request.MessageId, ct);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            await SafeAuditAsync(bcClient, tenantCtx, "permission.denied", ex.Message, ct);
            return Results.Json(new { error = "permission_denied", detail = ex.Message },
                statusCode: StatusCodes.Status403Forbidden);
        }
    }

    private static async Task SafeAuditAsync(
        IBcApiClient bcClient,
        CommunicationHub.Backend.Core.Models.TenantContext ctx,
        string eventType,
        string message,
        CancellationToken ct)
    {
        try
        {
            await bcClient.WriteAuditEventAsync(ctx, eventType, message, ct);
        }
        catch
        {
            // Audit failures must not block user-facing operations.
        }
    }
}
