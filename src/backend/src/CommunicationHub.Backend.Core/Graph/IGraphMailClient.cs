using CommunicationHub.Backend.Core.Models;
using Microsoft.Graph.Models;

namespace CommunicationHub.Backend.Core.Graph;

/// <summary>Contract for Microsoft Graph operations used by the Copilot API.</summary>
public interface IGraphMailClient
{
    /// <summary>
    /// Fetch a single mail message by its Exchange message-id.
    /// The caller is responsible for providing a valid OBO access token for Graph.
    /// </summary>
    Task<GraphMailMessage?> GetMessageAsync(
        string mailboxUpn,
        string messageId,
        string oboAccessToken,
        CancellationToken ct = default);
}

/// <summary>Lightweight DTO; avoids leaking Graph SDK types across layers.</summary>
public sealed class GraphMailMessage
{
    public required string Id { get; init; }
    public string? Subject { get; init; }
    public string? BodyPreview { get; init; }
    public string? InternetMessageId { get; init; }
    public string? ConversationId { get; init; }
    public DateTimeOffset? ReceivedDateTime { get; init; }
    public required string SenderEmail { get; init; }
    public List<string> RecipientEmails { get; init; } = [];
    public bool HasAttachments { get; init; }
}
