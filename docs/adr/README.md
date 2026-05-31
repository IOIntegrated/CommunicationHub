# Architecture Decision Records (ADRs)

> Hier werden ADRs für den Customer Communication Copilot abgelegt.
>
> **Format:** [MADR](https://adr.github.io/madr/) (Markdown). Eine Datei pro Entscheidung,
> Dateiname: `NNNN-kurztitel.md` (z. B. `0001-trunk-based-development.md`).
>
> **Bestehende Entscheidungen** sind in den Plandokumenten dokumentiert und werden
> sukzessive in eigene ADRs ausgelagert. Zentrale Risiko-/Entscheidungsliste:
> [docs/plan/14-risks-decisions.md](../plan/14-risks-decisions.md).

## Konvention

Jede ADR enthält mindestens:

- **Status:** proposed / accepted / superseded / deprecated
- **Kontext:** Was hat uns zu dieser Entscheidung gebracht?
- **Entscheidung:** Was wurde entschieden?
- **Konsequenzen:** Was bedeutet das, positiv wie negativ?
- **Alternativen:** Welche Optionen wurden erwogen?

## Geplante ADRs (Sprint 1)

- `0001` Trunk-based Development + Conventional Commits
- `0002` Federated Identity (OIDC) statt Service-Principal-Secrets
- `0003` Region Sweden Central (EU Data Boundary)
- `0004` Application Permissions + Application Access Policy für Mail-Ingestion
- `0005` Service Bus Premium + Sessions je conversationId
