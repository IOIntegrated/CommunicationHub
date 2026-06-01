using CommunicationHub.Ingestion.Core.Models;

namespace CommunicationHub.Ingestion.Core.GapMonitoring;

/// <summary>
/// Evaluates if a mailbox ingestion stream is lagging enough to require a backfill request.
/// </summary>
public static class GapMonitorEvaluator
{
    public static bool ShouldQueueBackfill(
        HighWaterMark checkpoint,
        DateTimeOffset nowUtc,
        int maxLagMinutes)
    {
        if (maxLagMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLagMinutes), "maxLagMinutes must be > 0.");

        if (checkpoint.Status is HighWaterMarkStatus.InProgress)
            return false;

        if (!checkpoint.LastProcessedAt.HasValue)
            return true;

        var lag = nowUtc - checkpoint.LastProcessedAt.Value;
        return lag > TimeSpan.FromMinutes(maxLagMinutes);
    }
}
