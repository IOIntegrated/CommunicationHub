using System.ComponentModel;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;
using Microsoft.SemanticKernel;

namespace CommunicationHub.Backend.Core.AI.Plugins;

/// <summary>
/// SK Plugin that wraps Business Central Custom API calls.
/// The kernel invokes these functions as tools during AI planning.
/// </summary>
public sealed class BcPlugin(IBcApiClient bcClient, TenantContext tenantContext)
{
    [KernelFunction, Description("Check whether the user has a valid consent for a given mailbox address.")]
    public Task<bool> CheckConsent(
        [Description("UPN of the mailbox to check")] string mailboxUpn,
        CancellationToken cancellationToken = default) =>
        bcClient.CheckConsentAsync(tenantContext, mailboxUpn, cancellationToken);

    [KernelFunction, Description("Suggest matching BC customers for an e-mail address. Returns JSON array of candidates.")]
    public async Task<string> SuggestCustomers(
        [Description("Sender e-mail address")] string senderEmail,
        [Description("Comma-separated list of recipient e-mail addresses")] string recipientsCsv,
        CancellationToken cancellationToken = default)
    {
        var recipients = recipientsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var candidates = await bcClient.SuggestCustomerMatchAsync(
            tenantContext, senderEmail, recipients, cancellationToken);

        return System.Text.Json.JsonSerializer.Serialize(candidates);
    }

    [KernelFunction, Description("Get aggregated customer context including recent interactions, open documents, and open action items.")]
    public async Task<string> GetCustomerContext(
        [Description("BC Customer No.")] string customerNo,
        CancellationToken cancellationToken = default)
    {
        var ctx = await bcClient.GetCustomerContextAsync(tenantContext, customerNo, cancellationToken);
        return System.Text.Json.JsonSerializer.Serialize(ctx);
    }
}
