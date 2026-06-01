using CommunicationHub.Backend.Api.Services;
using CommunicationHub.Backend.Core.Models;
using FluentAssertions;

namespace CommunicationHub.Backend.Tests;

public sealed class InteractionIdempotencyStoreTests
{
    [Fact]
    public void TryGet_ReturnsFalse_ForUnknownKey()
    {
        var store = new InMemoryInteractionIdempotencyStore();

        var found = store.TryGet("unknown", out _);

        found.Should().BeFalse();
    }

    [Fact]
    public void Store_ThenTryGet_ReturnsSavedResult()
    {
        var store = new InMemoryInteractionIdempotencyStore();
        var expected = new InteractionSaveResult
        {
            InteractionId = "int-1",
            BcEntryNo = "100",
        };

        store.Store("k-1", expected);
        var found = store.TryGet("k-1", out var actual);

        found.Should().BeTrue();
        actual.InteractionId.Should().Be("int-1");
        actual.BcEntryNo.Should().Be("100");
    }
}
