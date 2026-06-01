using CommunicationHub.Backend.Core.Models;

namespace CommunicationHub.Backend.Core.AI;

/// <summary>Contract for the Semantic Kernel-based AI orchestration layer.</summary>
public interface ICopilotOrchestrator
{
    /// <summary>
    /// Run the full mail-analysis pipeline (C1 classify → C2 extract → C3 reply suggestion).
    /// The pipeline is deterministic (no auto-planner); each capability is called explicitly.
    /// </summary>
    Task<MailAnalysisResult> AnalyzeMailAsync(
        TenantContext ctx,
        MailAnalysisRequest request,
        CancellationToken ct = default);
}
