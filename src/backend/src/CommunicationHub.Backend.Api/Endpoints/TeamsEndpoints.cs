using CommunicationHub.Backend.Api.Middleware;
using CommunicationHub.Backend.Core.AI;
using CommunicationHub.Backend.Core.Auth;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationHub.Backend.Api.Endpoints;

public static class TeamsEndpoints
{
    public static void MapTeamsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/teams")
            .RequireAuthorization()
            .WithTags("Teams");

        group.MapPost("/message/analyze", AnalyzeMessageAsync)
            .WithName("AnalyzeTeamsMessage")
            .WithSummary("Analyze a Teams message (classify, extract, reply suggestion)");

        group.MapPost("/message/preview-interaction", PreviewInteractionAsync)
            .WithName("PreviewTeamsInteraction")
            .WithSummary("Build an interaction preview for Teams Message Extension confirmation flow");
    }

    private static async Task<IResult> AnalyzeMessageAsync(
        [FromBody] TeamsMessageAnalysisRequest request,
        HttpContext ctx,
        ICopilotOrchestrator orchestrator,
        IPermissionResolver permissions,
        IBcApiClient bcClient,
        CancellationToken ct)
    {
        var validationError = ValidateMessageRequest(request.ChatId, request.MessageId, request.MessageText);
        if (validationError is not null)
            return validationError;

        var tenantCtx = ctx.RequireTenantContext();

        var allowed = await permissions.CanTriggerAiAsync(tenantCtx, "Teams", request.MessageId, ct);
        if (!allowed)
        {
            await SafeAuditAsync(bcClient, tenantCtx, "permission.denied", "teams analyze denied", ct);
            return Results.Json(new { error = "permission_denied" }, statusCode: StatusCodes.Status403Forbidden);
        }

        try
        {
            var result = await orchestrator.AnalyzeTeamsMessageAsync(tenantCtx, request, ct);
            await SafeAuditAsync(bcClient, tenantCtx, "teams.analysis.completed", request.MessageId, ct);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            await SafeAuditAsync(bcClient, tenantCtx, "permission.denied", ex.Message, ct);
            return Results.Json(new { error = "permission_denied", detail = ex.Message },
                statusCode: StatusCodes.Status403Forbidden);
        }
    }

    private static async Task<IResult> PreviewInteractionAsync(
        [FromBody] TeamsPreviewInteractionRequest request,
        HttpContext ctx,
        ICopilotOrchestrator orchestrator,
        IPermissionResolver permissions,
        IBcApiClient bcClient,
        CancellationToken ct)
    {
        var validationError = ValidateMessageRequest(request.ChatId, request.MessageId, request.MessageText);
        if (validationError is not null)
            return validationError;

        var tenantCtx = ctx.RequireTenantContext();

        var allowed = await permissions.CanTriggerAiAsync(tenantCtx, "Teams", request.MessageId, ct);
        if (!allowed)
        {
            await SafeAuditAsync(bcClient, tenantCtx, "permission.denied", "teams preview denied", ct);
            return Results.Json(new { error = "permission_denied" }, statusCode: StatusCodes.Status403Forbidden);
        }

        var analysis = await orchestrator.AnalyzeTeamsMessageAsync(
            tenantCtx,
            new TeamsMessageAnalysisRequest
            {
                ChatId = request.ChatId,
                MessageId = request.MessageId,
                MessageText = request.MessageText,
                SenderUpn = request.SenderUpn,
                ParticipantUpns = request.ParticipantUpns,
                IncludeSuggestions = true,
            },
            ct);

        var customerNo = request.PreferredCustomerNo;
        if (string.IsNullOrWhiteSpace(customerNo))
        {
            customerNo = analysis.CustomerMatch?.Candidates
                .OrderByDescending(c => c.Confidence)
                .FirstOrDefault()?.No;
        }

        CustomerContextResult? customerContext = null;
        if (!string.IsNullOrWhiteSpace(customerNo))
        {
            customerContext = await bcClient.GetCustomerContextAsync(tenantCtx, customerNo, ct);
        }

        var preview = new TeamsInteractionPreviewResult
        {
            Analysis = analysis,
            CustomerContext = customerContext,
            SuggestedSaveRequest = new InteractionSaveRequest
            {
                MessageId = request.MessageId,
                SourceMessageId = request.MessageId,
                ChatId = request.ChatId,
                SourceChannel = "Teams Chat",
                PermalinkUrl = request.PermalinkUrl,
                AnalysisId = analysis.AnalysisId,
                SummaryText = analysis.ReplySuggestion?.Brief,
                EntityLinks = analysis.CustomerMatch?.Candidates
                    .OrderByDescending(c => c.Confidence)
                    .Take(3)
                    .Select(c => new EntityLink
                    {
                        EntityType = "Customer",
                        EntityNo = c.No,
                        Confidence = c.Confidence,
                    })
                    .ToList() ?? [],
                UserConfirmed = false,
            },
        };

        await SafeAuditAsync(bcClient, tenantCtx, "teams.preview.generated", request.MessageId, ct);

        return Results.Ok(preview);
    }

    private static IResult? ValidateMessageRequest(string? chatId, string? messageId, string? messageText)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            return Results.BadRequest(new { error = "chat_id_required" });

        if (string.IsNullOrWhiteSpace(messageId))
            return Results.BadRequest(new { error = "message_id_required" });

        if (string.IsNullOrWhiteSpace(messageText))
            return Results.BadRequest(new { error = "message_text_required" });

        return null;
    }

    private static async Task SafeAuditAsync(
        IBcApiClient bcClient,
        TenantContext ctx,
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
