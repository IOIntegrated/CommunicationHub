namespace CommunicationHub.Backend.Api.Endpoints;

public static class ComplianceEndpoints
{
    private sealed record RiskItem(string Id, string Severity, string Area, string Message, string Mitigation);

    public static void MapComplianceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/compliance")
            .RequireAuthorization()
            .WithTags("Compliance");

        group.MapGet("/risk-assessment", GetRiskAssessment)
            .WithName("GetComplianceRiskAssessment")
            .WithSummary("Returns explicit compliance risk evaluation based on runtime configuration");
    }

    private static IResult GetRiskAssessment(IConfiguration cfg)
    {
        var risks = new List<RiskItem>();

        var dsfaApproved = cfg.GetValue<bool>("Compliance:DsfaApproved");
        if (!dsfaApproved)
        {
            risks.Add(new RiskItem(
                "R-DSFA-001",
                "high",
                "privacy",
                "DSFA is not approved.",
                "Finalize DSFA sign-off before extending rollout scope."));
        }

        var worksCouncilAgreementActive = cfg.GetValue<bool>("Compliance:WorksCouncilAgreementActive");
        if (!worksCouncilAgreementActive)
        {
            risks.Add(new RiskItem(
                "R-BV-001",
                "high",
                "labor-law",
                "Works council agreement is not active.",
                "Complete works council agreement before wide rollout."));
        }

        var tenantIsolationValidated = cfg.GetValue<bool>("Compliance:TenantIsolationValidated");
        if (!tenantIsolationValidated)
        {
            risks.Add(new RiskItem(
                "R-ISO-001",
                "medium",
                "security",
                "Tenant isolation validation not confirmed.",
                "Run cross-tenant negative tests and store evidence."));
        }

        var retentionDays = cfg.GetValue<int?>("Compliance:AuditRetentionDays") ?? 0;
        if (retentionDays < 30)
        {
            risks.Add(new RiskItem(
                "R-AUD-001",
                "medium",
                "audit",
                "Audit retention is below 30 days.",
                "Set Compliance:AuditRetentionDays to at least 30 days."));
        }

        var status = risks.Count == 0
            ? "green"
            : risks.Any(r => r.Severity == "high")
                ? "red"
                : "amber";

        return Results.Ok(new
        {
            generatedAtUtc = DateTimeOffset.UtcNow,
            status,
            riskCount = risks.Count,
            risks,
        });
    }
}
