using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CommunicationHub.Ingestion.Functions;

/// <summary>
/// Hourly trigger that verifies active Graph subscriptions and schedules backfill for missing entries.
/// </summary>
public sealed partial class SubscriptionRenewalTrigger
{
    private readonly ILogger<SubscriptionRenewalTrigger> _logger;

    public SubscriptionRenewalTrigger(ILogger<SubscriptionRenewalTrigger> logger)
    {
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "SubscriptionRenewalTrigger fired. ScheduleStatus.Last={Last} IsPastDue={IsPastDue} EventType={EventType}")]
    private static partial void LogTriggered(ILogger logger, DateTimeOffset? last, bool isPastDue, string eventType);

    [Function(nameof(SubscriptionRenewalTrigger))]
    public async Task RunAsync(
        [TimerTrigger("0 0 * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        LogTriggered(_logger, timerInfo.ScheduleStatus?.Last, timerInfo.IsPastDue, "subscription.healthcheck.fired");

        // TODO Sprint 1:
        // 1. Read active Graph subscription ids via GET /v1.0/subscriptions.
        // 2. Compare with expected ids derived from IngestionCheckpoint entries.
        // 3. Emit subscription.removed warning events for missing subscriptions.
        // 4. Enqueue BackfillRequest messages with TriggeredBy=SubscriptionLost.

        await Task.CompletedTask;
    }
}
