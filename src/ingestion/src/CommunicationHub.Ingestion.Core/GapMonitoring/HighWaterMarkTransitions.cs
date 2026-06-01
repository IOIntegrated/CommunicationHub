using CommunicationHub.Ingestion.Core.Models;

namespace CommunicationHub.Ingestion.Core.GapMonitoring;

/// <summary>
/// Stateless transition helpers for HighWaterMark lifecycle updates.
/// </summary>
public static class HighWaterMarkTransitions
{
    public static HighWaterMark MarkMessageProcessed(
        HighWaterMark current,
        string messageId,
        DateTimeOffset processedAtUtc)
    {
        return current with
        {
            LastProcessedMessageId = messageId,
            LastProcessedAt = processedAtUtc,
            ProcessedCount = current.ProcessedCount + 1,
            Status = HighWaterMarkStatus.InProgress,
            LastErrorMessage = null,
        };
    }

    public static HighWaterMark MarkMessageFailed(
        HighWaterMark current,
        string sanitizedError,
        int maxConsecutiveFailures)
    {
        if (maxConsecutiveFailures <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxConsecutiveFailures), "maxConsecutiveFailures must be > 0.");

        var nextFailures = current.FailureCount + 1;
        var nextStatus = nextFailures >= maxConsecutiveFailures
            ? HighWaterMarkStatus.Paused
            : HighWaterMarkStatus.InProgress;

        return current with
        {
            FailureCount = nextFailures,
            LastErrorMessage = sanitizedError,
            Status = nextStatus,
        };
    }

    public static HighWaterMark IncrementGapCount(HighWaterMark current) =>
        current with { GapCount = current.GapCount + 1 };
}
