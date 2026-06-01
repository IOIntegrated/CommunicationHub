using CommunicationHub.Ingestion.Core.GapMonitoring;
using CommunicationHub.Ingestion.Core.Models;
using FluentAssertions;
using Xunit;

namespace CommunicationHub.Ingestion.Tests;

public sealed class HighWaterMarkTransitionsTests
{
    [Fact]
    public void MarkMessageProcessedUpdatesProgressFields()
    {
        var current = BuildCheckpoint();
        var processedAt = DateTimeOffset.UtcNow;

        var updated = HighWaterMarkTransitions.MarkMessageProcessed(current, "<new-message>", processedAt);

        updated.LastProcessedMessageId.Should().Be("<new-message>");
        updated.LastProcessedAt.Should().Be(processedAt);
        updated.ProcessedCount.Should().Be(current.ProcessedCount + 1);
        updated.LastErrorMessage.Should().BeNull();
        updated.Status.Should().Be(HighWaterMarkStatus.InProgress);
    }

    [Fact]
    public void MarkMessageFailedPausesAfterMaxConsecutiveFailures()
    {
        var current = BuildCheckpoint() with { FailureCount = 4, Status = HighWaterMarkStatus.InProgress };

        var updated = HighWaterMarkTransitions.MarkMessageFailed(current, "graph-timeout", maxConsecutiveFailures: 5);

        updated.FailureCount.Should().Be(5);
        updated.Status.Should().Be(HighWaterMarkStatus.Paused);
        updated.LastErrorMessage.Should().Be("graph-timeout");
    }

    [Fact]
    public void IncrementGapCountIncrementsGapCounter()
    {
        var current = BuildCheckpoint() with { GapCount = 2 };

        var updated = HighWaterMarkTransitions.IncrementGapCount(current);

        updated.GapCount.Should().Be(3);
    }

    private static HighWaterMark BuildCheckpoint() =>
        new()
        {
            TenantId = "tenant-1",
            MailboxAddress = "sales@contoso.com",
            LastProcessedAt = DateTimeOffset.UtcNow.AddMinutes(-2),
            LastProcessedMessageId = "<m1>",
            BackfillFrom = DateTimeOffset.UtcNow.AddHours(-24),
            BackfillTo = DateTimeOffset.UtcNow,
            Status = HighWaterMarkStatus.Pending,
            ProcessedCount = 1,
            FailureCount = 0,
            LastErrorMessage = null,
            GapCount = 0,
        };
}
