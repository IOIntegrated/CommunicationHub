using System.Net.Http.Json;
using System.Text.Json;
using CommunicationHub.Backend.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CommunicationHub.Backend.Core.BC;

/// <summary>
/// HTTP client wrapper for Business Central OData Custom APIs.
/// Resilience policies (retry, circuit-breaker, timeout) are configured at the
/// IHttpClientFactory level in Program.cs – this class is kept free of Polly concerns.
/// </summary>
public sealed partial class BcApiClient(
    HttpClient http,
    IOptions<BcApiOptions> options,
    ILogger<BcApiClient> logger) : IBcApiClient
{
    // Stored for Sprint 1 real BC HTTP calls (currently all stubs).
    private readonly HttpClient _http = http;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // ── Consent ─────────────────────────────────────────────────────────────

    public async Task<bool> CheckConsentAsync(
        TenantContext ctx,
        string mailboxUpn,
        CancellationToken ct = default)
    {
        var url = BuildUrl(ctx, $"consentStatus?mailbox={Uri.EscapeDataString(mailboxUpn)}");
        LogBcCall(logger, nameof(CheckConsentAsync), url, ctx.CorrelationId);

        // TODO Sprint 1: send OBO token header; parse response body.
        // For now return true to allow development without a BC sandbox.
        await Task.CompletedTask;
        return true;
    }

    // ── Matching ─────────────────────────────────────────────────────────────

    public async Task<List<CustomerCandidate>> SuggestCustomerMatchAsync(
        TenantContext ctx,
        string senderEmail,
        IEnumerable<string> recipientEmails,
        CancellationToken ct = default)
    {
        var url = BuildUrl(ctx, "matching/suggestCustomer");
        LogBcCall(logger, nameof(SuggestCustomerMatchAsync), url, ctx.CorrelationId);

        await Task.CompletedTask;

        var mailboxDomain = ExtractDomain(ctx.MailboxUpn);
        var candidates = new List<CustomerCandidate>();

        AddCandidateFromEmail(candidates, senderEmail, mailboxDomain, "sender_domain");

        foreach (var recipient in recipientEmails)
        {
            AddCandidateFromEmail(candidates, recipient, mailboxDomain, "recipient_domain");
        }

        return candidates
            .GroupBy(c => c.No, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(c => c.Confidence).First())
            .OrderByDescending(c => c.Confidence)
            .Take(5)
            .ToList();
    }

    // ── Context ──────────────────────────────────────────────────────────────

    public async Task<CustomerContextResult> GetCustomerContextAsync(
        TenantContext ctx,
        string customerNo,
        CancellationToken ct = default)
    {
        var url = BuildUrl(ctx, $"context/customers('{Uri.EscapeDataString(customerNo)}')");
        LogBcCall(logger, nameof(GetCustomerContextAsync), url, ctx.CorrelationId);

        // TODO Sprint 1: call BC, deserialise OData response.
        await Task.CompletedTask;
        return new CustomerContextResult { CustomerNo = customerNo, Name = string.Empty };
    }

    public async Task<bool> CanViewCustomerAsync(
        TenantContext ctx,
        string customerNo,
        CancellationToken ct = default)
    {
        var url = BuildUrl(ctx, $"permissions/customers('{Uri.EscapeDataString(customerNo)}')/canView");
        LogBcCall(logger, nameof(CanViewCustomerAsync), url, ctx.CorrelationId);

        await Task.CompletedTask;
        return !customerNo.StartsWith("BLOCK-", StringComparison.OrdinalIgnoreCase);
    }

    // ── Interaction save ─────────────────────────────────────────────────────

    public async Task<InteractionSaveResult> SaveInteractionAsync(
        TenantContext ctx,
        InteractionSaveRequest request,
        CancellationToken ct = default)
    {
        var url = BuildUrl(ctx, "interactions");
        LogBcCall(logger, nameof(SaveInteractionAsync), url, ctx.CorrelationId);

        // TODO Sprint 1: POST to BC Custom API, map response.
        await Task.CompletedTask;
        return new InteractionSaveResult { InteractionId = Guid.NewGuid().ToString(), BcEntryNo = "0" };
    }

    // ── Audit ────────────────────────────────────────────────────────────────

    public async Task WriteAuditEventAsync(
        TenantContext ctx,
        string eventType,
        string message,
        CancellationToken ct = default)
    {
        var url = BuildUrl(ctx, "audit");
        LogBcCall(logger, nameof(WriteAuditEventAsync), url, ctx.CorrelationId);

        // TODO Sprint 1: POST audit entry. This is fire-and-forget; swallow non-critical errors.
        await Task.CompletedTask;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private string BuildUrl(TenantContext ctx, string path)
    {
        var baseUrl = options.Value.BaseUrl.TrimEnd('/');
        var company = Uri.EscapeDataString(ctx.BcCompanyId);
        return $"{baseUrl}/v2.0/{ctx.TenantId}/{company}/api/iointegrated/communicationHub/v1.0/{path}";
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "BC API call {Operation} → {Url} (correlation={CorrelationId})")]
    private static partial void LogBcCall(ILogger logger, string operation, string url, string correlationId);

    private static void AddCandidateFromEmail(
        List<CustomerCandidate> candidates,
        string? email,
        string mailboxDomain,
        string evidence)
    {
        var domain = ExtractDomain(email);
        if (string.IsNullOrWhiteSpace(domain))
            return;

        if (string.Equals(domain, mailboxDomain, StringComparison.OrdinalIgnoreCase))
            return;

        candidates.Add(new CustomerCandidate
        {
            No = BuildCustomerNo(domain),
            Name = BuildName(domain),
            Confidence = 0.71,
            Evidence = evidence,
        });
    }

    private static string BuildCustomerNo(string domain)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(domain.ToLowerInvariant()));
        var suffix = Convert.ToHexString(hash.AsSpan(0, 2));
        return $"CUST-{suffix}";
    }

    private static string BuildName(string domain)
    {
        var root = domain.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? domain;
        return char.ToUpperInvariant(root[0]) + root[1..];
    }

    private static string ExtractDomain(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var at = email.LastIndexOf('@');
        if (at < 0 || at + 1 >= email.Length)
            return string.Empty;

        return email[(at + 1)..].Trim().ToLowerInvariant();
    }
}

/// <summary>Configuration for <see cref="BcApiClient"/>.</summary>
public sealed class BcApiOptions
{
    public const string Section = "BcApi";
    public required string BaseUrl { get; init; }
}
