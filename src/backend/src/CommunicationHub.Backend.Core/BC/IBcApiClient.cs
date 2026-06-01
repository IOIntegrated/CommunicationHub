using CommunicationHub.Backend.Core.Models;

namespace CommunicationHub.Backend.Core.BC;

/// <summary>
/// Contract for calling Business Central Custom APIs.
/// All methods accept a <see cref="TenantContext"/> so the client can route to
/// the correct BC company and propagate the correlation-id.
/// </summary>
public interface IBcApiClient
{
    /// <summary>Check consent status for a mailbox; returns false if consent is absent or withdrawn.</summary>
    Task<bool> CheckConsentAsync(TenantContext ctx, string mailboxUpn, CancellationToken ct = default);

    /// <summary>Suggest matching BC customers/contacts/jobs for a given e-mail/domain.</summary>
    Task<List<CustomerCandidate>> SuggestCustomerMatchAsync(
        TenantContext ctx,
        string senderEmail,
        IEnumerable<string> recipientEmails,
        CancellationToken ct = default);

    /// <summary>Read aggregated customer context (stamm, open documents, recent interactions).</summary>
    Task<CustomerContextResult> GetCustomerContextAsync(
        TenantContext ctx,
        string customerNo,
        CancellationToken ct = default);

    /// <summary>Check if the caller is allowed to read customer context.</summary>
    Task<bool> CanViewCustomerAsync(
        TenantContext ctx,
        string customerNo,
        CancellationToken ct = default);

    /// <summary>Persist a new CommunicationInteraction entry in BC.</summary>
    Task<InteractionSaveResult> SaveInteractionAsync(
        TenantContext ctx,
        InteractionSaveRequest request,
        CancellationToken ct = default);

    /// <summary>Write an audit event to BC (append-only). Fire-and-forget friendly.</summary>
    Task WriteAuditEventAsync(
        TenantContext ctx,
        string eventType,
        string message,
        CancellationToken ct = default);
}
