// CommunicationHub.Ingestion.Functions – Sprint 0 Skeleton (I4)
// Azure Functions v4 isolated-worker entry point.
// See: docs/plan/07-ingestion-pipeline.md, docs/plan/18-sprint-0-backlog.md §I4

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // TODO Sprint 1: Register IIngestionCheckpointRepository (Azure Table Storage).
        // TODO Sprint 1: Register IGraphSubscriptionService.
        // TODO Sprint 1: Register IConsentService (calls BC Backend API).
        // TODO Sprint 1: Register IInteractionPersistenceService.
        // TODO Sprint 1: Configure IngestionOptions from IConfiguration.
    })
    .Build();

await host.RunAsync();
