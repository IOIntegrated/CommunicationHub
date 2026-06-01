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

/// <summary>Request body for POST /v1/teams/message/analyze.</summary>
public sealed class TeamsMessageAnalysisRequest
{
    [Required]
    [MaxLength(256)]
    public required string ChatId { get; init; }

    [Required]
    [MaxLength(1024)]
    public required string MessageId { get; init; }

    /// <summary>
    /// Teams message text sent by the client (ME/Bot).
    /// Graph fetch can be added later without changing the contract.
    /// </summary>
    [Required]
    [MaxLength(16000)]
    public required string MessageText { get; init; }

    [MaxLength(320)]
    public string? SenderUpn { get; init; }

    public List<string> ParticipantUpns { get; init; } = [];

    public bool IncludeSuggestions { get; init; } = true;

    /// <summary>When true the endpoint returns text/event-stream (SSE).</summary>
    public bool StreamResponse { get; init; }
}

/// <summary>Request body for POST /v1/teams/message/preview-interaction.</summary>
public sealed class TeamsPreviewInteractionRequest
{
    [Required]
    [MaxLength(256)]
    public required string ChatId { get; init; }

    [Required]
    [MaxLength(1024)]
    public required string MessageId { get; init; }

    [Required]
    [MaxLength(16000)]
    public required string MessageText { get; init; }

    [MaxLength(320)]
    public string? SenderUpn { get; init; }

    public List<string> ParticipantUpns { get; init; } = [];

    [MaxLength(2048)]
    public string? PermalinkUrl { get; init; }

    /// <summary>
    /// Optional user-selected customer to skip candidate defaulting.
    /// </summary>
    [MaxLength(20)]
    public string? PreferredCustomerNo { get; init; }
}

/// <summary>Request body for POST /v1/interactions (save-to-BC after user confirmation).</summary>
public sealed class InteractionSaveRequest
{
    [Required]
    [MaxLength(1024)]
    public required string MessageId { get; init; }

    /// <summary>Correlation back to a prior MailAnalysisResult.</summary>
    public string? AnalysisId { get; init; }

    [MaxLength(50)]
    public string SourceChannel { get; init; } = "Email";

    [MaxLength(256)]
    public string? ChatId { get; init; }

    [MaxLength(1024)]
    public string? SourceMessageId { get; init; }

    [MaxLength(2048)]
    public string? PermalinkUrl { get; init; }

    [MaxLength(4000)]
    public string? SummaryText { get; init; }

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
