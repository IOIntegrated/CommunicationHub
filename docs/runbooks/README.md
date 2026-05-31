# Runbooks

> Operative Runbooks für den Customer Communication Copilot. **Pflicht ab MVP1.**
>
> Owner: `@TODO-org/communicationhub-devops` + `@TODO-org/communicationhub-backend`

## Geplante Runbooks (Sprint 1 / MVP1)

| Runbook | Auslöser | Eskalation |
|---------|----------|------------|
| `graph-subscription-renewal-failed.md` | Lifecycle-Notification ohne erfolgreiches Renew | On-Call Backend |
| `mail-ingestion-stuck.md` | Service Bus DLQ-Tiefe > Schwellwert | On-Call Backend |
| `aoai-rate-limited.md` | 429-Rate gegen Azure OpenAI > Schwellwert | On-Call AI |
| `bc-api-degraded.md` | Custom-API-Fehlerquote / Throttling | On-Call BC |
| `prompt-injection-detected.md` | Audit-Event `prompt.injection.detected` | Security |
| `tenant-onboarding.md` | Neue Pilot-/Kunden-Mailgruppe | Delivery |
| `dsar-data-subject-request.md` | DSGVO-Auskunft / Löschung | Datenschutz |

## Konvention

Pro Runbook:

- **Trigger / Symptome**
- **Sofort-Maßnahmen** (was tun in den ersten 15 min)
- **Diagnose** (Logs, KQL-Queries, Dashboards)
- **Behebung** (Schritt-für-Schritt)
- **Validierung** (woran erkenne ich, dass es behoben ist?)
- **Postmortem-Pflicht** ja/nein

Siehe auch [docs/plan/12-security-compliance.md](../plan/12-security-compliance.md) §7 (Audit) und §11 (DSGVO).
