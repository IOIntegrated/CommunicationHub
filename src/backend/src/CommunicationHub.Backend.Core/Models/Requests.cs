using System.ComponentModel.DataAnnotations;

namespace CommunicationHub.Backend.Core.Models;

/// <summary>Request body for POST /v1/mail/analyze.</summary>
public sealed class MailAnalysisRequest
{
    [Required]
    [MaxLength(1024)]
    public required string MessageId { get; init; }

    /// <summary>Optional UPN hint to disambiguate shared mailbox access.</summary>
    [MaxLength(320)]
    public string? MailboxHint { get; init; }

    public bool IncludeSuggestions { get; init; } = true;

    /// <summary>When true the endpoint returns text/event-stream (SSE).</summary>
    public bool StreamResponse { get; init; }
}

/// <summary>Request body for POST /v1/interactions (save-to-BC after user confirmation).</summary>
public sealed class InteractionSaveRequest
{
    [Required]
    [MaxLength(1024)]
    public required string MessageId { get; init; }

    /// <summary>Correlation back to a prior MailAnalysisResult.</summary>
    public string? AnalysisId { get; init; }

    public List<EntityLink> EntityLinks { get; init; } = [];

    /// <summary>Must be explicitly true; prevents accidental saves.</summary>
    public bool UserConfirmed { get; init; }
}

public sealed class EntityLink
{
    public required string EntityType { get; init; }  // "Customer" | "Contact" | "Job"
    public required string EntityNo { get; init; }
    public double Confidence { get; init; }
}

/// <summary>Request body for POST /v1/feedback.</summary>
public sealed class FeedbackRequest
{
    [Required]
    public required string DecisionId { get; init; }

    /// <summary>true = accepted, false = rejected.</summary>
    public required bool Accepted { get; init; }

    [MaxLength(2000)]
    public string? FreeText { get; init; }
}
