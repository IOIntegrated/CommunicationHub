using CommunicationHub.Ingestion.Core.GapMonitoring;
using CommunicationHub.Ingestion.Core.Models;
using FluentAssertions;
using Xunit;

namespace CommunicationHub.Ingestion.Tests;

public sealed class GapMonitorEvaluatorTests
{
    [Fact]
    public void ShouldQueueBackfillReturnsTrueWhenLastProcessedExceedsMaxLag()
    {
        var now = DateTimeOffset.UtcNow;
        var checkpoint = BuildCheckpoint(lastProcessedAt: now.AddMinutes(-45), status: HighWaterMarkStatus.Completed);

        var shouldQueue = GapMonitorEvaluator.ShouldQueueBackfill(checkpoint, now, maxLagMinutes: 15);

        shouldQueue.Should().BeTrue();
    }

    [Fact]
    public void ShouldQueueBackfillReturnsFalseWhenCheckpointIsInProgress()
    {
        var now = DateTimeOffset.UtcNow;
        var checkpoint = BuildCheckpoint(lastProcessedAt: now.AddMinutes(-90), status: HighWaterMarkStatus.InProgress);

        var shouldQueue = GapMonitorEvaluator.ShouldQueueBackfill(checkpoint, now, maxLagMinutes: 15);

        shouldQueue.Should().BeFalse();
    }

    [Fact]
    public void ShouldQueueBackfillReturnsTrueWhenNoHistoryExists()
    {
        var now = DateTimeOffset.UtcNow;
        var checkpoint = BuildCheckpoint(lastProcessedAt: null, status: HighWaterMarkStatus.Pending);

        var shouldQueue = GapMonitorEvaluator.ShouldQueueBackfill(checkpoint, now, maxLagMinutes: 15);

        shouldQueue.Should().BeTrue();
    }

    private static HighWaterMark BuildCheckpoint(DateTimeOffset? lastProcessedAt, HighWaterMarkStatus status) =>
        new()
        {
            TenantId = "tenant-1",
            MailboxAddress = "sales@contoso.com",
            LastProcessedAt = lastProcessedAt,
            LastProcessedMessageId = "<m1>",
            BackfillFrom = DateTimeOffset.UtcNow.AddHours(-24),
            BackfillTo = DateTimeOffset.UtcNow,
            Status = status,
            ProcessedCount = 2,
            FailureCount = 0,
            LastErrorMessage = null,
            GapCount = 0,
        };
}
