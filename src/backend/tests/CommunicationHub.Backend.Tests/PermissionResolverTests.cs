using CommunicationHub.Backend.Core.Auth;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CommunicationHub.Backend.Tests;

/// <summary>Unit tests for <see cref="PermissionResolver"/>.</summary>
public sealed class PermissionResolverTests
{
    private static TenantContext MakeCtx(string mailboxUpn = "user@contoso.com") => new()
    {
        TenantId = "tenant-1",
        BcCompanyId = "company-1",
        UserId = "user-oid",
        MailboxUpn = mailboxUpn,
    };

    [Fact]
    public async Task CanTriggerAi_ReturnsTrue_WhenConsentGranted()
    {
        var bcMock = new Mock<IBcApiClient>();
        bcMock.Setup(c => c.CheckConsentAsync(It.IsAny<TenantContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        var sut = new PermissionResolver(bcMock.Object, NullLogger<PermissionResolver>.Instance);

        var result = await sut.CanTriggerAiAsync(MakeCtx(), "Mail", "msg-1");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanTriggerAi_ReturnsFalse_WhenConsentAbsent()
    {
        var bcMock = new Mock<IBcApiClient>();
        bcMock.Setup(c => c.CheckConsentAsync(It.IsAny<TenantContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);

        var sut = new PermissionResolver(bcMock.Object, NullLogger<PermissionResolver>.Instance);

        var result = await sut.CanTriggerAiAsync(MakeCtx(), "Mail", "msg-1");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanTriggerAi_SkipsConsentCheck_ForNonMailResource()
    {
        var bcMock = new Mock<IBcApiClient>();

        var sut = new PermissionResolver(bcMock.Object, NullLogger<PermissionResolver>.Instance);

        // For non-Mail resource, consent check is skipped → should return true without a BC call.
        var result = await sut.CanTriggerAiAsync(MakeCtx(), "Teams", "chat-1");

        result.Should().BeTrue();
        bcMock.Verify(c => c.CheckConsentAsync(It.IsAny<TenantContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CanTriggerAi_SkipsConsentCheck_WhenMailboxUpnEmpty()
    {
        var bcMock = new Mock<IBcApiClient>();

        var sut = new PermissionResolver(bcMock.Object, NullLogger<PermissionResolver>.Instance);

        // Empty mailboxUpn → consent check skipped (no UPN to check against).
        var result = await sut.CanTriggerAiAsync(MakeCtx(mailboxUpn: string.Empty), "Mail", "msg-1");

        result.Should().BeTrue();
        bcMock.Verify(c => c.CheckConsentAsync(It.IsAny<TenantContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CanViewCustomerContext_DelegatesToBcPermissionCheck()
    {
        var bcMock = new Mock<IBcApiClient>();
        bcMock.Setup(c => c.CanViewCustomerAsync(It.IsAny<TenantContext>(), "10000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        bcMock.Setup(c => c.CanViewCustomerAsync(It.IsAny<TenantContext>(), "BLOCK-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new PermissionResolver(bcMock.Object, NullLogger<PermissionResolver>.Instance);

        var allowed = await sut.CanViewCustomerContextAsync(MakeCtx(), "10000");
        var blocked = await sut.CanViewCustomerContextAsync(MakeCtx(), "BLOCK-1");

        allowed.Should().BeTrue();
        blocked.Should().BeFalse();
    }

    [Fact]
    public async Task CanViewAiSummary_ReturnsFalse_WhenConsentIsMissing()
    {
        var bcMock = new Mock<IBcApiClient>();
        bcMock.Setup(c => c.CanViewCustomerAsync(It.IsAny<TenantContext>(), "10000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        bcMock.Setup(c => c.CheckConsentAsync(It.IsAny<TenantContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new PermissionResolver(bcMock.Object, NullLogger<PermissionResolver>.Instance);

        var result = await sut.CanViewAiSummaryAsync(MakeCtx(), "10000");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanViewAiSummary_ReturnsTrue_WhenCustomerVisibleAndConsentGranted()
    {
        var bcMock = new Mock<IBcApiClient>();
        bcMock.Setup(c => c.CanViewCustomerAsync(It.IsAny<TenantContext>(), "10000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        bcMock.Setup(c => c.CheckConsentAsync(It.IsAny<TenantContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new PermissionResolver(bcMock.Object, NullLogger<PermissionResolver>.Instance);

        var result = await sut.CanViewAiSummaryAsync(MakeCtx(), "10000");

        result.Should().BeTrue();
    }
}
