using CommunicationHub.Backend.Api.Middleware;
using CommunicationHub.Backend.Api.Services;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationHub.Backend.Api.Endpoints;

public static class InteractionEndpoints
{
    public static void MapInteractionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/interactions")
            .RequireAuthorization()
            .WithTags("Interactions");

        group.MapPost("/", SaveAsync)
            .WithName("SaveInteraction")
            .WithSummary("Persist a communication interaction in BC after explicit user confirmation");
    }

    private static async Task<IResult> SaveAsync(
        [FromBody] InteractionSaveRequest request,
        HttpContext ctx,
        IBcApiClient bcClient,
        IInteractionIdempotencyStore idempotency,
        CancellationToken ct)
    {
        // UserConfirmed must be explicitly true — prevents accidental saves.
        if (!request.UserConfirmed)
            return Results.BadRequest(new { error = "user_confirmation_required" });

        if (string.IsNullOrWhiteSpace(request.MessageId))
            return Results.BadRequest(new { error = "message_id_required" });

        if (request.SourceChannel.StartsWith("Teams", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(request.ChatId))
        {
            return Results.BadRequest(new { error = "chat_id_required_for_teams" });
        }

        var tenantCtx = ctx.RequireTenantContext();
        var idempotencyKey = BuildIdempotencyKey(tenantCtx, request);

        if (idempotency.TryGet(idempotencyKey, out var existing))
        {
            await SafeAuditAsync(bcClient, tenantCtx, "interaction.duplicate", request.MessageId, ct);
            return Results.Ok(new { duplicate = true, existing.InteractionId, existing.BcEntryNo });
        }

        var result = await bcClient.SaveInteractionAsync(tenantCtx, request, ct);
        idempotency.Store(idempotencyKey, result);
        await SafeAuditAsync(bcClient, tenantCtx, "interaction.persisted", request.MessageId, ct);
        return Results.Created($"/v1/interactions/{result.BcEntryNo}", result);
    }

    private static string BuildIdempotencyKey(TenantContext tenantCtx, InteractionSaveRequest request)
    {
        var chatPart = request.ChatId ?? string.Empty;
        return string.Join('|',
            tenantCtx.TenantId,
            tenantCtx.BcCompanyId,
            request.SourceChannel,
            request.MessageId,
            chatPart);
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
