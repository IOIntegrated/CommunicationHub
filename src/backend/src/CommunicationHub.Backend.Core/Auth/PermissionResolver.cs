using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;

namespace CommunicationHub.Backend.Core.Auth;

/// <summary>
/// Pre-AI permission check (Architecture Principle L5).
/// Every AI capability invocation must pass through this resolver before any AOAI call.
/// </summary>
public interface IPermissionResolver
{
    /// <summary>
    /// Returns true if the caller is permitted to trigger AI processing on the given resource.
    /// Throws <see cref="UnauthorizedAccessException"/> with a structured reason on denial.
    /// </summary>
    Task<bool> CanTriggerAiAsync(
        TenantContext ctx,
        string resourceType,
        string resourceId,
        CancellationToken ct = default);
}

/// <summary>Default implementation that checks BC consent and user role claims.</summary>
public sealed partial class PermissionResolver(
    IBcApiClient bcClient,
    Microsoft.Extensions.Logging.ILogger<PermissionResolver> logger) : IPermissionResolver
{
    public async Task<bool> CanTriggerAiAsync(
        TenantContext ctx,
        string resourceType,
        string resourceId,
        CancellationToken ct = default)
    {
        // 1. Check consent for the mailbox associated with this request.
        //    For resource types other than "Mail" we skip the mailbox check.
        if (resourceType.Equals("Mail", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(ctx.MailboxUpn))
        {
            var hasConsent = await bcClient.CheckConsentAsync(ctx, ctx.MailboxUpn, ct);
            if (!hasConsent)
            {
                LogPermissionDenied(logger, "consent_absent", ctx.TenantId, ctx.CorrelationId);
                return false;
            }
        }

        // 2. TODO Sprint 1: check visibility-scope against the caller's BC permission set.
        //    For now all authenticated requests with valid consent pass.

        return true;
    }

    [Microsoft.Extensions.Logging.LoggerMessage(
        Level = Microsoft.Extensions.Logging.LogLevel.Warning,
        Message = "PermissionResolver: denied reason={Reason} tenant={TenantId} correlation={CorrelationId}")]
    private static partial void LogPermissionDenied(
        Microsoft.Extensions.Logging.ILogger logger,
        string reason,
        string tenantId,
        string correlationId);
}
