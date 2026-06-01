namespace CommunicationHub.Backend.Api.Middleware;

/// <summary>
/// Adds security-related HTTP response headers (OWASP A05).
/// Should run early in the pipeline, before any endpoint handler.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Content-Security-Policy"] =
            "default-src 'none'; frame-ancestors 'none'; form-action 'none'";

        // HSTS is set by the reverse proxy / App Service TLS termination in production.
        // Only set here in non-development to avoid conflicts with local dev certs.
        if (!context.Request.IsHttps)
        {
            // HTTP in non-prod → redirect handled by App Service; skip HSTS.
        }
        else
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        return next(context);
    }
}
