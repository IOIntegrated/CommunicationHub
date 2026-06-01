using CommunicationHub.Backend.Core.Models;

namespace CommunicationHub.Backend.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        // Liveness probe — no auth required, called by App Service every 30 s.
        app.MapGet("/v1/health", () => Results.Ok(new HealthResult { Status = "ok" }))
            .WithName("Liveness")
            .WithTags("Health")
            .AllowAnonymous();

        // Deep health — checks downstream dependencies; auth required.
        app.MapGet("/v1/health/deep", DeepHealthAsync)
            .WithName("ReadinessDeep")
            .WithTags("Health")
            .RequireAuthorization();

        // Version info.
        app.MapGet("/v1/version", (IConfiguration cfg) => Results.Ok(new
        {
            version = cfg["Version"] ?? "0.0.0-dev",
            environment = cfg["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
        }))
        .WithName("Version")
        .WithTags("Health")
        .AllowAnonymous();
    }

    private static Task<IResult> DeepHealthAsync(CancellationToken ct)
    {
        // TODO Sprint 1: perform actual downstream pings (BC, AOAI, Search).
        var result = new HealthResult
        {
            Status = "ok",
            Dependencies = new Dictionary<string, string>
            {
                ["bc"] = "stub",
                ["aoai"] = "stub",
                ["search"] = "stub",
            },
        };
        return Task.FromResult(Results.Ok(result));
    }
}
