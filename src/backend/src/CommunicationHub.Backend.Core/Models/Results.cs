namespace CommunicationHub.Backend.Core.Models;

/// <summary>Full response from POST /v1/mail/analyze.</summary>
public sealed class MailAnalysisResult
{
    public required string AnalysisId { get; init; }
    public required ClassificationResult Classification { get; init; }
    public required ExtractionResult Extraction { get; init; }
    public CustomerMatchResult? CustomerMatch { get; init; }
    public ReplySuggestion? ReplySuggestion { get; init; }
    public List<SourceReference> Sources { get; init; } = [];
    public required AuditInfo Audit { get; init; }
    public bool PromptInjectionWarning { get; init; }
}

public sealed class ClassificationResult
{
    public bool IsExternal { get; init; }
    public string Channel { get; init; } = "Email";
    public string Sensitivity { get; init; } = "Internal";
    public double Confidence { get; init; }
}

public sealed class ExtractionResult
{
    public List<ParticipantInfo> Participants { get; init; } = [];
    public List<ActionItemInfo> ActionItems { get; init; } = [];
    public List<TopicInfo> Topics { get; init; } = [];
}

public sealed class ParticipantInfo
{
    public required string Email { get; init; }
    public required string Role { get; init; }  // From | To | CC
    public bool IsExternal { get; init; }
    public string? BcMatchHint { get; init; }
}

public sealed class ActionItemInfo
{
    public required string Description { get; init; }
    public string? DueDateHint { get; init; }
    public double Confidence { get; init; }
}

public sealed class TopicInfo
{
    public required string Label { get; init; }
    public double Confidence { get; init; }
}

public sealed class CustomerMatchResult
{
    public List<CustomerCandidate> Candidates { get; init; } = [];
}

public sealed class CustomerCandidate
{
    public required string No { get; init; }
    public required string Name { get; init; }
    public double Confidence { get; init; }
    public string? Evidence { get; init; }
}

public sealed class ReplySuggestion
{
    /// <summary>Brief reply text (1-3 sentences).</summary>
    public required string Brief { get; init; }
    /// <summary>Extended reply text with full rationale (optional).</summary>
    public string? Extended { get; init; }
    public List<string> Citations { get; init; } = [];
    public bool ContainsCommitments { get; init; }
    public string Language { get; init; } = "de";
    public double Confidence { get; init; }
}

public sealed class SourceReference
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Excerpt { get; init; }
    public string SourceType { get; init; } = "Email";
    public DateTimeOffset CapturedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class AuditInfo
{
    public required string DecisionId { get; init; }
    public required string ModelDeployment { get; init; }
    public int TokenCount { get; init; }
    public int LatencyMs { get; init; }
}

/// <summary>Response for GET /v1/context/customer/{no}.</summary>
public sealed class CustomerContextResult
{
    public required string CustomerNo { get; init; }
    public required string Name { get; init; }
    public List<SourceReference> RecentInteractions { get; init; } = [];
    public List<SourceReference> RecentDocuments { get; init; } = [];
    public List<string> OpenActionItems { get; init; } = [];
    public string? AiSummary { get; init; }
}

/// <summary>Response for GET /v1/health.</summary>
public sealed class HealthResult
{
    public string Status { get; init; } = "ok";
    public Dictionary<string, string> Dependencies { get; init; } = [];
}

/// <summary>Response for POST /v1/interactions.</summary>
public sealed class InteractionSaveResult
{
    public required string InteractionId { get; init; }
    public required string BcEntryNo { get; init; }
}

/// <summary>Response for POST /v1/teams/message/preview-interaction.</summary>
public sealed class TeamsInteractionPreviewResult
{
    public required MailAnalysisResult Analysis { get; init; }
    public CustomerContextResult? CustomerContext { get; init; }
    public required InteractionSaveRequest SuggestedSaveRequest { get; init; }
}
