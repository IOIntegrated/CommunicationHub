using CommunicationHub.Backend.Core.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CommunicationHub.Backend.Api.Middleware;

/// <summary>
/// Resolves and validates the <see cref="TenantContext"/> from request headers and the
/// Bearer token claims, then stores it in <c>HttpContext.Items</c>.
/// Must run after authentication middleware.
///
/// Headers consumed:
///   x-ccc-tenant    – M365 tenant ID (must match the 'tid' claim)
///   x-ccc-bc-company – BC company System-ID
///   x-correlation-id – caller-supplied or generated
/// </summary>
public sealed partial class TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
{
    public const string ItemKey = "TenantContext";

    private const string HeaderTenant = "x-ccc-tenant";
    private const string HeaderCompany = "x-ccc-bc-company";
    private const string HeaderCorrelation = "x-correlation-id";

    public async Task InvokeAsync(HttpContext context)
    {
        // Health-check endpoints bypass tenant validation.
        if (context.Request.Path.StartsWithSegments("/v1/health", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var user = context.User;

        // Only enforce on authenticated requests.
        if (user.Identity?.IsAuthenticated == true)
        {
            var tenantFromHeader = context.Request.Headers[HeaderTenant].FirstOrDefault() ?? string.Empty;
            var tenantFromToken = user.FindFirstValue("tid") ?? string.Empty;

            // Tenant isolation: header must match token claim.
            if (!string.IsNullOrEmpty(tenantFromHeader)
                && !string.Equals(tenantFromHeader, tenantFromToken, StringComparison.OrdinalIgnoreCase))
            {
                LogTenantMismatch(logger, tenantFromHeader, tenantFromToken);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "tenant_mismatch" });
                return;
            }

            var correlationId = context.Request.Headers[HeaderCorrelation].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            var tenantCtx = new TenantContext
            {
                TenantId = tenantFromToken,
                BcCompanyId = context.Request.Headers[HeaderCompany].FirstOrDefault() ?? string.Empty,
                UserId = user.FindFirstValue("oid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                MailboxUpn = user.FindFirstValue("upn") ?? user.FindFirstValue(ClaimTypes.Upn) ?? string.Empty,
                CorrelationId = correlationId,
            };

            context.Items[ItemKey] = tenantCtx;

            // Echo correlation-id in the response.
            context.Response.Headers["x-correlation-id"] = correlationId;
        }

        await next(context);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "TenantContext: header tenant={HeaderTenant} ≠ token tenant={TokenTenant}")]
    private static partial void LogTenantMismatch(ILogger l, string headerTenant, string tokenTenant);
}

/// <summary>Extension to retrieve the resolved <see cref="TenantContext"/> from the current request.</summary>
public static class TenantContextExtensions
{
    public static TenantContext? GetTenantContext(this HttpContext context) =>
        context.Items.TryGetValue(TenantContextMiddleware.ItemKey, out var val)
            ? val as TenantContext
            : null;

    public static TenantContext RequireTenantContext(this HttpContext context) =>
        context.GetTenantContext()
        ?? throw new InvalidOperationException("TenantContext was not resolved for this request.");
}
