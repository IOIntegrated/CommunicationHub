// CommunicationHub.Ingestion.Core – Sprint 0 Skeleton (I4)
// High-Water-Mark model for per-mailbox ingestion progress tracking.
// See: docs/plan/22-high-water-mark.md

namespace CommunicationHub.Ingestion.Core.Models;

/// <summary>
/// Represents the ingestion progress checkpoint for a single mailbox.
/// Persisted in Azure Table Storage (PartitionKey = TenantId, RowKey = MailboxAddress).
/// </summary>
public sealed record HighWaterMark
{
    /// <summary>Microsoft 365 tenant identifier (GUID string).</summary>
    public required string TenantId { get; init; }

    /// <summary>UPN of the mailbox being ingested (e.g. "user@contoso.com").</summary>
    public required string MailboxAddress { get; init; }

    /// <summary>
    /// Internet Message-ID of the last successfully persisted message.
    /// Null if no message has been processed yet.
    /// </summary>
    public string? LastProcessedMessageId { get; init; }

    /// <summary>
    /// ReceivedDateTime of the last successfully persisted message (UTC).
    /// Null if no message has been processed yet.
    /// </summary>
    public DateTimeOffset? LastProcessedAt { get; init; }

    /// <summary>Start of the current or last backfill window (UTC, inclusive).</summary>
    public DateTimeOffset BackfillFrom { get; init; }

    /// <summary>End of the current or last backfill window (UTC, exclusive).</summary>
    public DateTimeOffset BackfillTo { get; init; }

    /// <summary>Current processing status.</summary>
    public HighWaterMarkStatus Status { get; init; }

    /// <summary>Number of messages successfully persisted in the current backfill run.</summary>
    public int ProcessedCount { get; init; }

    /// <summary>Number of messages that failed processing in the current backfill run.</summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Sanitised (no PII) description of the last error. Null if no error occurred.
    /// </summary>
    public string? LastErrorMessage { get; init; }

    /// <summary>
    /// Number of ingestion gaps detected for this mailbox (incremented by Gap Monitor).
    /// See: docs/plan/21-gap-monitor-spec.md
    /// </summary>
    public int GapCount { get; init; }
}

/// <summary>
/// Lifecycle states for a High-Water-Mark entry.
/// See: docs/plan/22-high-water-mark.md §3.
/// </summary>
public enum HighWaterMarkStatus
{
    /// <summary>Registered for backfill but not yet started.</summary>
    Pending,

    /// <summary>Backfill is actively running.</summary>
    InProgress,

    /// <summary>Backfill window fully processed.</summary>
    Completed,

    /// <summary>Fatal error – requires manual intervention.</summary>
    Failed,

    /// <summary>
    /// Paused (throttle back-off, consent withdrawn, or max consecutive failures reached).
    /// Resumed automatically by BackfillResumeTrigger.
    /// </summary>
    Paused,
}
