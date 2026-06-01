namespace CommunicationHub.Ingestion.Core.Models;

/// <summary>
/// Command message that schedules a bounded backfill run for a single mailbox.
/// </summary>
public sealed record BackfillRequest
{
    public required string TenantId { get; init; }
    public required string MailboxAddress { get; init; }
    public required DateTimeOffset BackfillFrom { get; init; }
    public required DateTimeOffset BackfillTo { get; init; }
    public required BackfillTriggerSource TriggeredBy { get; init; }
}

public enum BackfillTriggerSource
{
    GapDetected,
    SubscriptionLost,
    ManualRequest,
}
