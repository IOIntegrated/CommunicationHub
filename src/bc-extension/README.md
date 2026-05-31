# BC Extension (AL)

> Business Central Extension – Datenmodell, Pages, Custom APIs für den Customer Communication Copilot.

**Plandokumente:**
- [docs/plan/02-bc-data-model.md](../../docs/plan/02-bc-data-model.md) – Tabellen & Felder
- [docs/plan/03-bc-apis.md](../../docs/plan/03-bc-apis.md) – Custom APIs
- [docs/plan/12-security-compliance.md §5](../../docs/plan/12-security-compliance.md) – Permission Sets

**Owner:** `@TODO-org/communicationhub-bc`

## Zweck

Liefert das BC-seitige Datenmodell (Communication Interaction & verwandte Tabellen), Pages (Timeline, FactBoxes, Setup, Matching Inbox) und Custom APIs (v2.0) für Outlook Add-in, Teams App und Backend.

## Sprint 0 (dieses Repo-Scaffolding)

- [x] Ordnerstruktur + `app.json`-Skelett
- [ ] `id`, `publisher`, `idRanges` von BC-Admin festlegen (TODO)
- [ ] AL-Go for GitHub anbinden (siehe `.github/workflows/bc-extension-ci.yml`)

## Sprint 1 (geplant)

- Tabellen `Communication Interaction`, `Participant`, `Entity Link`, `AI Summary`, `Action Item`, `Source Reference`, `Setup`, `Internal Domain`, `Audit` (Subset gem. MVP1, [13-mvp-roadmap.md §2.1](../../docs/plan/13-mvp-roadmap.md)).
- Permission Sets `IOI_COMM_HUB_*`.
- Customer Communication Timeline Page + FactBoxes.
- Custom APIs `interaction`, `matchSuggest`, `contextCustomer`, `saveSummary`.

## Build / Test

TODO Sprint 1: AL-Go-for-GitHub Pipeline (`.github/workflows/bc-extension-ci.yml`).
