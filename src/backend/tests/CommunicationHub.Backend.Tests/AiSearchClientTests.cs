using CommunicationHub.Backend.Core.Models;
using CommunicationHub.Backend.Core.Search;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace CommunicationHub.Backend.Tests;

/// <summary>Unit tests for <see cref="AiSearchClient"/>.</summary>
public sealed class AiSearchClientTests
{
    private static TenantContext MakeCtx() => new()
    {
        TenantId = "tenant-1",
        BcCompanyId = "company-1",
        UserId = "u",
        MailboxUpn = "user@contoso.com",
    };

    [Fact]
    public async Task SearchInteractions_ReturnsEmptyList_InCurrentStub()
    {
        var factoryMock = new Mock<SearchClientFactory>();
        var opts = Options.Create(new SearchOptions
        {
            ServiceEndpoint = "https://stub.search.windows.net",
        });

        var sut = new AiSearchClient(factoryMock.Object, opts, NullLogger<AiSearchClient>.Instance);

        var results = await sut.SearchInteractionsAsync(MakeCtx(), "test query", topK: 5);

        // Sprint 0 stub returns empty results until real index is available.
        results.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task SearchBcMaster_ReturnsEmptyList_InCurrentStub()
    {
        var factoryMock = new Mock<SearchClientFactory>();
        var opts = Options.Create(new SearchOptions
        {
            ServiceEndpoint = "https://stub.search.windows.net",
        });

        var sut = new AiSearchClient(factoryMock.Object, opts, NullLogger<AiSearchClient>.Instance);

        var results = await sut.SearchBcMasterAsync(MakeCtx(), "customer 10000", topK: 3);

        results.Should().NotBeNull().And.BeEmpty();
    }
}
