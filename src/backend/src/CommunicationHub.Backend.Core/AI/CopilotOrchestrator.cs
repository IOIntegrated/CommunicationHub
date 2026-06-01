using System.Diagnostics;
using System.Text.Json;
using CommunicationHub.Backend.Core.AI.Plugins;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Graph;
using CommunicationHub.Backend.Core.Models;
using CommunicationHub.Backend.Core.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace CommunicationHub.Backend.Core.AI;

/// <summary>
/// Semantic Kernel orchestrator for the Communication Copilot.
/// The pipeline is deterministic (no auto-planner) to guarantee reproducible behaviour:
///   1. C7 – Prompt-injection pre-check
///   2. C1 – Classify (is_external, channel, sensitivity)
///   3. C2 – Extract (participants, action items, topics)
///   4. C3 – Reply suggestion with citations
/// </summary>
public sealed partial class CopilotOrchestrator(
    IKernelFactory kernelFactory,
    IGraphMailClient graphClient,
    IBcApiClient bcClient,
    ISearchClient searchClient,
    IOptions<CopilotOptions> options,
    ILogger<CopilotOrchestrator> logger) : ICopilotOrchestrator
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<MailAnalysisResult> AnalyzeMailAsync(
        TenantContext ctx,
        MailAnalysisRequest request,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var analysisId = Guid.NewGuid().ToString();

        LogStartAnalysis(logger, analysisId, ctx.TenantId);

        // ── Step 1: fetch mail from Graph ────────────────────────────────────
        // TODO Sprint 1: pass the actual OBO token; for now the GraphServiceClient
        // uses the credential configured at DI registration.
        var mail = await graphClient.GetMessageAsync(
            ctx.MailboxUpn, request.MessageId, oboAccessToken: string.Empty, ct);

        if (mail is null)
        {
            LogMailNotFound(logger, request.MessageId, ctx.CorrelationId);
            // Return a minimal result; the endpoint will translate this to 404.
            return new MailAnalysisResult
            {
                AnalysisId = analysisId,
                Classification = new ClassificationResult(),
                Extraction = new ExtractionResult(),
                Audit = new AuditInfo { DecisionId = analysisId, ModelDeployment = options.Value.ChatDeployment, TokenCount = 0, LatencyMs = (int)sw.ElapsedMilliseconds },
            };
        }

        // ── Step 2: check consent (Pre-AI permission check, L5) ─────────────
        var hasConsent = await bcClient.CheckConsentAsync(ctx, ctx.MailboxUpn, ct);
        if (!hasConsent)
        {
            LogPermissionDenied(logger, "consent_missing", ctx.CorrelationId);
            throw new UnauthorizedAccessException("Consent for this mailbox is absent or withdrawn.");
        }

        // ── Step 3: build per-request kernel with request-scoped plugins ─────
        var kernel = kernelFactory.CreateKernel(ctx, searchClient, bcClient);

        // ── Step 4: C7 – prompt-injection check ──────────────────────────────
        var subjectForCheck = mail.Subject ?? string.Empty;
        var bodyForCheck = mail.BodyPreview ?? string.Empty;
        var injectionDetected = await RunInjectionCheckAsync(kernel, subjectForCheck + "\n" + bodyForCheck, ct);
        if (injectionDetected)
        {
            LogInjectionDetected(logger, ctx.CorrelationId);
        }

        // ── Step 5: C1 – classify ─────────────────────────────────────────────
        var classification = await RunClassificationAsync(kernel, mail, ct);

        // ── Step 6: retrieve context from Search ──────────────────────────────
        var searchQuery = $"{mail.Subject} {mail.SenderEmail}";
        var sources = await searchClient.SearchInteractionsAsync(ctx, searchQuery, topK: 8, ct);

        // ── Step 7: C2 – extract ──────────────────────────────────────────────
        var extraction = await RunExtractionAsync(kernel, mail, ct);

        // ── Step 8: customer match ─────────────────────────────────────────────
        var matchCandidates = request.IncludeSuggestions
            ? await bcClient.SuggestCustomerMatchAsync(ctx, mail.SenderEmail, mail.RecipientEmails, ct)
            : [];

        // ── Step 9: C3 – reply suggestion ─────────────────────────────────────
        ReplySuggestion? reply = null;
        if (request.IncludeSuggestions && !injectionDetected)
        {
            reply = await RunReplySuggestionAsync(kernel, mail, sources, ct);
        }

        sw.Stop();

        var result = new MailAnalysisResult
        {
            AnalysisId = analysisId,
            Classification = classification,
            Extraction = extraction,
            CustomerMatch = matchCandidates.Count > 0
                ? new CustomerMatchResult { Candidates = matchCandidates }
                : null,
            ReplySuggestion = reply,
            Sources = sources,
            PromptInjectionWarning = injectionDetected,
            Audit = new AuditInfo
            {
                DecisionId = analysisId,
                ModelDeployment = options.Value.ChatDeployment,
                TokenCount = 0, // TODO Sprint 1: accumulate tokens from SK usage
                LatencyMs = (int)sw.ElapsedMilliseconds,
            },
        };

        LogAnalysisComplete(logger, analysisId, sw.ElapsedMilliseconds);
        return result;
    }

    // ── C7: Injection check ───────────────────────────────────────────────────

    private static async Task<bool> RunInjectionCheckAsync(Kernel kernel, string content, CancellationToken ct)
    {
        // TODO Sprint 1: implement a lightweight LLM-based C7 injection classifier.
        // For now use a simple regex pre-filter to unblock development.
        await Task.CompletedTask;
        return ContainsInjectionMarkers(content);
    }

    private static readonly string[] InjectionMarkers =
        ["ignore previous instructions", "system:", "user:", "assistant:", "###instruction"];

    private static bool ContainsInjectionMarkers(string text) =>
        InjectionMarkers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase));

    // ── C1: Classification ────────────────────────────────────────────────────

    private static async Task<ClassificationResult> RunClassificationAsync(
        Kernel _kernel,
        GraphMailMessage _mail,
        CancellationToken _ct)
    {
        // TODO Sprint 1: invoke the AOAI classification capability using the
        // system prompt from /prompts/system/classification.md.
        await Task.CompletedTask;
        return new ClassificationResult
        {
            IsExternal = true,
            Channel = "Email",
            Sensitivity = "Internal",
        };
    }

    // ── C2: Extraction ────────────────────────────────────────────────────────

    private static async Task<ExtractionResult> RunExtractionAsync(
        Kernel _kernel,
        GraphMailMessage mail,
        CancellationToken _ct)
    {
        // TODO Sprint 1: invoke the AOAI extraction capability.
        await Task.CompletedTask;
        return new ExtractionResult
        {
            Participants =
            [
                new ParticipantInfo
                {
                    Email = mail.SenderEmail,
                    Role = "From",
                    IsExternal = true,
                }
            ],
        };
    }

    // ── C3: Reply suggestion ──────────────────────────────────────────────────

    private static async Task<ReplySuggestion?> RunReplySuggestionAsync(
        Kernel _kernel,
        GraphMailMessage _mail,
        List<SourceReference> _sources,
        CancellationToken _ct)
    {
        // TODO Sprint 1: invoke the AOAI reply capability with grounding context.
        // IMPORTANT: The reply MUST contain at least one source citation.
        //            Replies without citations MUST be rejected (C3 grounding rule).
        await Task.CompletedTask;
        return null;
    }

    // ── Log messages ──────────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information, Message = "CopilotOrchestrator: start analysis {AnalysisId} tenant={TenantId}")]
    private static partial void LogStartAnalysis(ILogger l, string analysisId, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CopilotOrchestrator: mail {MessageId} not found (correlation={CorrelationId})")]
    private static partial void LogMailNotFound(ILogger l, string messageId, string correlationId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CopilotOrchestrator: permission denied reason={Reason} (correlation={CorrelationId})")]
    private static partial void LogPermissionDenied(ILogger l, string reason, string correlationId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CopilotOrchestrator: prompt injection detected (correlation={CorrelationId})")]
    private static partial void LogInjectionDetected(ILogger l, string correlationId);

    [LoggerMessage(Level = LogLevel.Information, Message = "CopilotOrchestrator: analysis {AnalysisId} complete in {ElapsedMs}ms")]
    private static partial void LogAnalysisComplete(ILogger l, string analysisId, long elapsedMs);
}
