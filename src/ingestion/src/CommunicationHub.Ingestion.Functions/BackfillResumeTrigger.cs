// CommunicationHub.Ingestion.Functions – Sprint 0 Skeleton (I4)
// Timer trigger that resumes paused backfill jobs.
// See: docs/plan/22-high-water-mark.md §5, docs/plan/21-gap-monitor-spec.md §3

using CommunicationHub.Ingestion.Core.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CommunicationHub.Ingestion.Functions;

/// <summary>
/// Fires every 15 minutes and resumes any <see cref="HighWaterMarkStatus.Paused"/>
/// or <see cref="HighWaterMarkStatus.Pending"/> backfill jobs for active mailboxes.
/// </summary>
/// <remarks>
/// Sprint 0: logs timer invocation only. Full implementation in Sprint 1.
/// CRON schedule: every 15 minutes – "0 */15 * * * *".
/// See: docs/plan/22-high-water-mark.md §5
/// </remarks>
public sealed partial class BackfillResumeTrigger
{
    private readonly ILogger<BackfillResumeTrigger> _logger;

    public BackfillResumeTrigger(ILogger<BackfillResumeTrigger> logger)
    {
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "BackfillResumeTrigger fired. ScheduleStatus.Last={Last} IsPastDue={IsPastDue} EventType={EventType}")]
    private static partial void LogTriggered(ILogger logger, DateTimeOffset? last, bool isPastDue, string eventType);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "BackfillResumeTrigger is running late. EventType={EventType}")]
    private static partial void LogPastDue(ILogger logger, string eventType);

    [Function(nameof(BackfillResumeTrigger))]
    public async Task RunAsync(
        [TimerTrigger("0 */15 * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        LogTriggered(_logger, timerInfo.ScheduleStatus?.Last, timerInfo.IsPastDue, "backfill.trigger.fired");

        if (timerInfo.IsPastDue)
        {
            LogPastDue(_logger, "backfill.trigger.pastdue");
        }

        // TODO Sprint 1: Resolve IIngestionCheckpointRepository from DI.
        // TODO Sprint 1: Query all HighWaterMark entries with Status == Paused
        //                where LastProcessedAt < (DateTimeOffset.UtcNow - ResumeDelayMinutes).
        // TODO Sprint 1: For each paused entry:
        //   1. Transition Status → InProgress (ETag optimistic update).
        //   2. Log eventType = "backfill.resumed" (correlationId, tenantId, mailbox hash only).
        //   3. Enqueue BackfillRequest to Service Bus queue "ingestion-backfill".
        //      Message: { TenantId, MailboxAddress (hashed for log), BackfillFrom = HWM.LastProcessedAt,
        //                 BackfillTo = DateTimeOffset.UtcNow, TriggeredBy = "Resume" }
        // TODO Sprint 1: Also pick up Pending entries that have not been started yet.

        await Task.CompletedTask; // placeholder – remove when DI services are wired
    }
}
