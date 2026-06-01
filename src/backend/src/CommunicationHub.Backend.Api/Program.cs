using Azure.Identity;
using CommunicationHub.Backend.Api.Endpoints;
using CommunicationHub.Backend.Api.Middleware;
using CommunicationHub.Backend.Core.AI;
using CommunicationHub.Backend.Core.Auth;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Graph;
using CommunicationHub.Backend.Core.Search;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Threading.RateLimiting;
using SearchOptions = CommunicationHub.Backend.Core.Search.SearchOptions;

// ── Bootstrap Serilog early so startup errors are captured ──────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
        formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Communication Copilot API starting up");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithProperty("Application", "CommunicationHub.Backend.Api")
           .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

    // ── Azure Managed Identity credential (AOAI, Search, Key Vault) ──────────
    var credential = new DefaultAzureCredential();

    // ── Authentication: Entra ID Bearer (Microsoft.Identity.Web) ────────────
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration, "AzureAd");
    builder.Services.AddAuthorization();

    // ── Application Insights ─────────────────────────────────────────────────
    builder.Services.AddApplicationInsightsTelemetry();

    // ── Rate limiting (60 req/min per user, OWASP A04) ───────────────────────
    builder.Services.AddRateLimiter(opts =>
    {
        opts.AddFixedWindowLimiter("perUser", policy =>
        {
            policy.Window = TimeSpan.FromMinutes(1);
            policy.PermitLimit = 60;
            policy.QueueLimit = 0;
            policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    });

    // ── OpenAPI ───────────────────────────────────────────────────────────────
    builder.Services.AddOpenApi();

    // ── Options ───────────────────────────────────────────────────────────────
    builder.Services
        .AddOptions<BcApiOptions>()
        .Bind(builder.Configuration.GetSection(BcApiOptions.Section))
        .ValidateOnStart();
    builder.Services
        .AddOptions<SearchOptions>()
        .Bind(builder.Configuration.GetSection(SearchOptions.Section))
        .ValidateOnStart();
    builder.Services
        .AddOptions<CopilotOptions>()
        .Bind(builder.Configuration.GetSection(CopilotOptions.Section))
        .ValidateOnStart();

    // ── HTTP clients with Polly resilience ───────────────────────────────────
    builder.Services
        .AddHttpClient<IBcApiClient, BcApiClient>()
        .AddStandardResilienceHandler();

    // ── Microsoft Graph ───────────────────────────────────────────────────────
    // TODO Sprint 1: swap to per-request OnBehalfOfCredential.
    builder.Services.AddSingleton<GraphServiceClient>(_ =>
        new GraphServiceClient(credential, ["https://graph.microsoft.com/.default"]));
    builder.Services.AddSingleton<IGraphMailClient, GraphMailClient>();

    // ── Azure AI Search ───────────────────────────────────────────────────────
    builder.Services.AddSingleton<Azure.Core.TokenCredential>(_ => credential);
    builder.Services.AddSingleton<SearchClientFactory>();
    builder.Services.AddSingleton<ISearchClient, AiSearchClient>();

    // ── AI Orchestration ─────────────────────────────────────────────────────
    builder.Services.AddSingleton<IKernelFactory, KernelFactory>();
    builder.Services.AddScoped<ICopilotOrchestrator, CopilotOrchestrator>();

    // ── Permission resolver ───────────────────────────────────────────────────
    builder.Services.AddScoped<IPermissionResolver, PermissionResolver>();

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Middleware pipeline (order matters) ───────────────────────────────────
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseSerilogRequestLogging(opts =>
        opts.EnrichDiagnosticContext = (diag, ctx) =>
        {
            if (ctx.Items[TenantContextMiddleware.ItemKey] is CommunicationHub.Backend.Core.Models.TenantContext t)
                diag.Set("TenantId", t.TenantId);
        });

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseMiddleware<TenantContextMiddleware>();

    // ── Route groups ──────────────────────────────────────────────────────────
    app.MapHealthEndpoints();
    app.MapMailEndpoints();
    app.MapTeamsEndpoints();
    app.MapInteractionEndpoints();
    app.MapContextEndpoints();
    app.MapFeedbackEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "Communication Copilot API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
