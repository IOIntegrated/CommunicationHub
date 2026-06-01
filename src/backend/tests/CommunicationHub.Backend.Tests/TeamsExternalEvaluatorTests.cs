using CommunicationHub.Backend.Core.AI;
using FluentAssertions;

namespace CommunicationHub.Backend.Tests;

public sealed class TeamsExternalEvaluatorTests
{
    [Fact]
    public void HasExternalParticipation_ReturnsTrue_WhenSenderIsExternal()
    {
        var result = TeamsExternalEvaluator.HasExternalParticipation(
            mailboxUpn: "agent@contoso.com",
            senderUpn: "guest@fabrikam.com",
            participantUpns: ["agent@contoso.com"]);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasExternalParticipation_ReturnsTrue_WhenParticipantIsExternal()
    {
        var result = TeamsExternalEvaluator.HasExternalParticipation(
            mailboxUpn: "agent@contoso.com",
            senderUpn: "agent@contoso.com",
            participantUpns: ["worker@contoso.com", "guest@northwind.com"]);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasExternalParticipation_ReturnsFalse_WhenAllParticipantsInternal()
    {
        var result = TeamsExternalEvaluator.HasExternalParticipation(
            mailboxUpn: "agent@contoso.com",
            senderUpn: "owner@contoso.com",
            participantUpns: ["worker@contoso.com", "lead@contoso.com"]);

        result.Should().BeFalse();
    }
}
