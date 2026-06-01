using CommunicationHub.Ingestion.Core.Models;

namespace CommunicationHub.Ingestion.Core.SubscriptionHealth;

/// <summary>
/// Detects missing Graph subscriptions and emits backfill requests for affected mailboxes.
/// </summary>
public static class SubscriptionHealthEvaluator
{
    public static List<BackfillRequest> BuildBackfillRequestsForMissingSubscriptions(
        IEnumerable<HighWaterMark> checkpoints,
        ISet<string> activeSubscriptionIds,
        Func<HighWaterMark, string> expectedSubscriptionId,
        DateTimeOffset nowUtc,
        int backfillLookbackHours)
    {
        if (backfillLookbackHours <= 0)
            throw new ArgumentOutOfRangeException(nameof(backfillLookbackHours), "backfillLookbackHours must be > 0.");

        var requests = new List<BackfillRequest>();

        foreach (var checkpoint in checkpoints)
        {
            var subscriptionId = expectedSubscriptionId(checkpoint);
            if (string.IsNullOrWhiteSpace(subscriptionId))
                continue;

            if (activeSubscriptionIds.Contains(subscriptionId))
                continue;

            var from = checkpoint.LastProcessedAt ?? nowUtc.AddHours(-backfillLookbackHours);
            var boundedFrom = from > nowUtc ? nowUtc : from;

            requests.Add(new BackfillRequest
            {
                TenantId = checkpoint.TenantId,
                MailboxAddress = checkpoint.MailboxAddress,
                BackfillFrom = boundedFrom,
                BackfillTo = nowUtc,
                TriggeredBy = BackfillTriggerSource.SubscriptionLost,
            });
        }

        return requests;
    }
}
