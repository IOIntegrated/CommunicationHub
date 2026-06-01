using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace CommunicationHub.Backend.Tests;

/// <summary>Unit tests for <see cref="BcApiClient"/>.</summary>
public sealed class BcApiClientTests
{
    private static BcApiClient MakeClient(HttpMessageHandler handler)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.businesscentral.dynamics.com"),
        };
        var opts = Options.Create(new BcApiOptions { BaseUrl = "https://api.businesscentral.dynamics.com" });
        return new BcApiClient(http, opts, NullLogger<BcApiClient>.Instance);
    }

    private static TenantContext MakeCtx() => new()
    {
        TenantId = "tenant-1",
        BcCompanyId = "company-1",
        UserId = "u",
        MailboxUpn = "user@contoso.com",
    };

    [Fact]
    public async Task CheckConsent_ReturnsTrue_InCurrentStub()
    {
        // The current stub always returns true until Sprint 1 wires real BC calls.
        var handlerMock = new Mock<HttpMessageHandler>();
        var sut = MakeClient(handlerMock.Object);

        var result = await sut.CheckConsentAsync(MakeCtx(), "user@contoso.com");

        result.Should().BeTrue("the Sprint 0 stub always grants consent to unblock development");
    }

    [Fact]
    public async Task SuggestCustomerMatch_ReturnsEmptyList_InCurrentStub()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var sut = MakeClient(handlerMock.Object);

        var candidates = await sut.SuggestCustomerMatchAsync(
            MakeCtx(), "sender@external.com", ["rep@contoso.com"]);

        candidates.Should().BeEmpty("stub returns empty until Sprint 1");
    }
}
