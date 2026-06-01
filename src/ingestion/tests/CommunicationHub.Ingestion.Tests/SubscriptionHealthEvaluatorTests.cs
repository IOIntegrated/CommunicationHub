using CommunicationHub.Ingestion.Core.Models;
using CommunicationHub.Ingestion.Core.SubscriptionHealth;
using FluentAssertions;
using Xunit;

namespace CommunicationHub.Ingestion.Tests;

public sealed class SubscriptionHealthEvaluatorTests
{
    [Fact]
    public void BuildBackfillRequestsForMissingSubscriptionsCreatesRequestWhenSubscriptionIsMissing()
    {
        var now = DateTimeOffset.UtcNow;
        var checkpoints = new[]
        {
            new HighWaterMark
            {
                TenantId = "tenant-1",
                MailboxAddress = "sales@contoso.com",
                LastProcessedMessageId = "<m1>",
                LastProcessedAt = now.AddMinutes(-30),
                BackfillFrom = now.AddHours(-24),
                BackfillTo = now,
                Status = HighWaterMarkStatus.Completed,
                ProcessedCount = 10,
                FailureCount = 0,
                LastErrorMessage = null,
                GapCount = 0,
            }
        };

        var activeSubscriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sub-other"
        };

        var requests = SubscriptionHealthEvaluator.BuildBackfillRequestsForMissingSubscriptions(
            checkpoints,
            activeSubscriptions,
            expectedSubscriptionId: c => $"sub-{c.MailboxAddress}",
            nowUtc: now,
            backfillLookbackHours: 24);

        requests.Should().HaveCount(1);
        var request = requests[0];
        request.TenantId.Should().Be("tenant-1");
        request.MailboxAddress.Should().Be("sales@contoso.com");
        request.TriggeredBy.Should().Be(BackfillTriggerSource.SubscriptionLost);
        request.BackfillFrom.Should().Be(now.AddMinutes(-30));
        request.BackfillTo.Should().Be(now);
    }

    [Fact]
    public void BuildBackfillRequestsForMissingSubscriptionsSkipsWhenSubscriptionExists()
    {
        var now = DateTimeOffset.UtcNow;
        var checkpoint = new HighWaterMark
        {
            TenantId = "tenant-1",
            MailboxAddress = "sales@contoso.com",
            LastProcessedMessageId = "<m1>",
            LastProcessedAt = now.AddMinutes(-30),
            BackfillFrom = now.AddHours(-24),
            BackfillTo = now,
            Status = HighWaterMarkStatus.Completed,
            ProcessedCount = 10,
            FailureCount = 0,
            LastErrorMessage = null,
            GapCount = 0,
        };

        var activeSubscriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sub-sales@contoso.com"
        };

        var requests = SubscriptionHealthEvaluator.BuildBackfillRequestsForMissingSubscriptions(
            new[] { checkpoint },
            activeSubscriptions,
            expectedSubscriptionId: c => $"sub-{c.MailboxAddress}",
            nowUtc: now,
            backfillLookbackHours: 24);

        requests.Should().BeEmpty();
    }
}
