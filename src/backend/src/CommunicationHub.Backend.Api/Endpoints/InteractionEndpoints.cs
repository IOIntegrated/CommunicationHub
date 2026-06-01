using CommunicationHub.Backend.Api.Middleware;
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
        CancellationToken ct)
    {
        // UserConfirmed must be explicitly true — prevents accidental saves.
        if (!request.UserConfirmed)
            return Results.BadRequest(new { error = "user_confirmation_required" });

        if (string.IsNullOrWhiteSpace(request.MessageId))
            return Results.BadRequest(new { error = "message_id_required" });

        var tenantCtx = ctx.RequireTenantContext();

        var result = await bcClient.SaveInteractionAsync(tenantCtx, request, ct);
        return Results.Created($"/v1/interactions/{result.BcEntryNo}", result);
    }
}
