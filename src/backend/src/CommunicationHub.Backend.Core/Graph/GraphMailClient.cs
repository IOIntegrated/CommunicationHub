using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CommunicationHub.Backend.Core.Graph;

/// <summary>
/// Microsoft Graph mail client backed by the Graph SDK v5.
/// The <see cref="GraphServiceClient"/> is expected to be constructed with an
/// <c>OnBehalfOfCredential</c> so all calls run under the delegated user identity.
/// </summary>
public sealed partial class GraphMailClient(
    GraphServiceClient graphClient,
    ILogger<GraphMailClient> logger) : IGraphMailClient
{
    public async Task<GraphMailMessage?> GetMessageAsync(
        string mailboxUpn,
        string messageId,
        string oboAccessToken,
        CancellationToken ct = default)
    {
        LogFetchMessage(logger, mailboxUpn, messageId);

        // TODO Sprint 1: supply the OBO token to the Graph client per-request.
        // Currently the GraphServiceClient uses the credential set at DI registration time.
        var message = await graphClient.Users[mailboxUpn]
            .Messages[messageId]
            .GetAsync(req =>
            {
                req.QueryParameters.Select =
                [
                    "id", "subject", "bodyPreview", "internetMessageId",
                    "conversationId", "receivedDateTime", "sender",
                    "toRecipients", "ccRecipients", "hasAttachments"
                ];
            }, ct);

        if (message is null) return null;

        var senderEmail = message.Sender?.EmailAddress?.Address ?? string.Empty;
        var recipients = (message.ToRecipients ?? [])
            .Concat(message.CcRecipients ?? [])
            .Select(r => r.EmailAddress?.Address ?? string.Empty)
            .Where(e => !string.IsNullOrEmpty(e))
            .ToList();

        return new GraphMailMessage
        {
            Id = message.Id ?? messageId,
            Subject = message.Subject,
            BodyPreview = message.BodyPreview,
            InternetMessageId = message.InternetMessageId,
            ConversationId = message.ConversationId,
            ReceivedDateTime = message.ReceivedDateTime,
            SenderEmail = senderEmail,
            RecipientEmails = recipients,
            HasAttachments = message.HasAttachments ?? false,
        };
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Graph: fetching message {MessageId} for mailbox {Mailbox}")]
    private static partial void LogFetchMessage(ILogger logger, string mailbox, string messageId);
}
