namespace CommunicationHub.Backend.Core.Models;

/// <summary>Tenant + company context resolved from the incoming request headers and Bearer token.</summary>
public sealed record TenantContext
{
    public required string TenantId { get; init; }
    public required string BcCompanyId { get; init; }
    public required string UserId { get; init; }       // AAD Object ID (OID claim)
    public required string MailboxUpn { get; init; }   // UPN claim or empty
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
