using CommunicationHub.Backend.Core.Models;
using FluentAssertions;

namespace CommunicationHub.Backend.Tests;

/// <summary>
/// Unit tests for core request/response models.
/// Covers invariants the rest of the pipeline depends on.
/// </summary>
public sealed class ModelTests
{
    [Fact]
    public void TenantContext_CorrelationId_DefaultsToNewGuid()
    {
        var ctx1 = new TenantContext { TenantId = "t", BcCompanyId = "c", UserId = "u", MailboxUpn = "m" };
        var ctx2 = new TenantContext { TenantId = "t", BcCompanyId = "c", UserId = "u", MailboxUpn = "m" };

        ctx1.CorrelationId.Should().NotBeNullOrEmpty();
        ctx2.CorrelationId.Should().NotBeNullOrEmpty();
        ctx1.CorrelationId.Should().NotBe(ctx2.CorrelationId, "each context should have a unique correlation id by default");
    }

    [Fact]
    public void InteractionSaveRequest_UserConfirmed_DefaultsFalse()
    {
        var req = new InteractionSaveRequest { MessageId = "msg-1" };
        req.UserConfirmed.Should().BeFalse("save must never happen without explicit confirmation");
    }

    [Fact]
    public void InteractionSaveRequest_SourceChannel_DefaultsToEmail()
    {
        var req = new InteractionSaveRequest { MessageId = "msg-1" };
        req.SourceChannel.Should().Be("Email");
    }

    [Fact]
    public void MailAnalysisRequest_StreamResponse_DefaultsFalse()
    {
        var req = new MailAnalysisRequest { MessageId = "msg-1" };
        req.StreamResponse.Should().BeFalse();
    }

    [Fact]
    public void MailAnalysisRequest_IncludeSuggestions_DefaultsTrue()
    {
        var req = new MailAnalysisRequest { MessageId = "msg-1" };
        req.IncludeSuggestions.Should().BeTrue();
    }

    [Fact]
    public void TeamsMessageAnalysisRequest_IncludeSuggestions_DefaultsTrue()
    {
        var req = new TeamsMessageAnalysisRequest
        {
            ChatId = "chat-1",
            MessageId = "msg-1",
            MessageText = "hello"
        };

        req.IncludeSuggestions.Should().BeTrue();
        req.StreamResponse.Should().BeFalse();
    }

    [Fact]
    public void MailAnalysisResult_Sources_DefaultsToEmptyList()
    {
        var result = new MailAnalysisResult
        {
            AnalysisId = "id",
            Classification = new ClassificationResult(),
            Extraction = new ExtractionResult(),
            Audit = new AuditInfo { DecisionId = "id", ModelDeployment = "gpt-4", TokenCount = 0, LatencyMs = 0 },
        };
        result.Sources.Should().NotBeNull().And.BeEmpty();
        result.PromptInjectionWarning.Should().BeFalse();
    }

    [Fact]
    public void SourceReference_CapturedAtUtc_DefaultsToCurrentTime()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-2);

        var source = new SourceReference
        {
            Id = "src-1",
            Title = "Mail",
        };

        source.CapturedAtUtc.Should().BeOnOrAfter(before);
    }
}
