# Customer Communication Copilot – Planungsübersicht

Dieses Verzeichnis enthält das konsolidierte Planungsergebnis zum Auftrag in `../../instructions.md`.
Die Lösung verbindet Microsoft Dynamics 365 Business Central, Outlook und Microsoft Teams zu einem aktiven, Grounded-AI-gestützten Kommunikationsassistenten – ohne automatisches externes Senden.

## Pflicht-Deliverables (aus instructions.md, Abschnitt 10)

| Nr. | Kapitel | Datei |
|----|---------|------|
| 1 | Zielarchitektur | [01-architecture.md](01-architecture.md) |
| 2 | Komponentenübersicht | [01-architecture.md](01-architecture.md) |
| 3 | BC-Datenmodell | [02-bc-data-model.md](02-bc-data-model.md) |
| 4 | API-Konzept | [03-bc-apis.md](03-bc-apis.md) |
| 5 | Outlook Add-in Konzept | [04-outlook-addin.md](04-outlook-addin.md) |
| 6 | Teams App Konzept | [05-teams-app.md](05-teams-app.md) |
| 7 | Backend-Service Konzept | [06-backend-service.md](06-backend-service.md) |
| – | Ingestion-Service & Pipeline | [07-ingestion-pipeline.md](07-ingestion-pipeline.md) |
| 8 | AI-Orchestrierung | [08-ai-orchestration.md](08-ai-orchestration.md) |
| – | Daten- & Suchkonzept | [09-data-search.md](09-data-search.md) |
| – | Zuordnungs-/Matching-Logik | [10-matching.md](10-matching.md) |
| – | Microsoft Graph Feasibility | [11-graph-feasibility.md](11-graph-feasibility.md) |
| 9 | Sicherheits- & Compliance-Konzept | [12-security-compliance.md](12-security-compliance.md) |
| 10 | MVP-Schnitt | [13-mvp-roadmap.md](13-mvp-roadmap.md) |
| 11 | Aufwandsschätzung | [13-mvp-roadmap.md](13-mvp-roadmap.md) |
| 12 | Risiken | [14-risks-decisions.md](14-risks-decisions.md) |
| 13 | Technische Entscheidungen | [14-risks-decisions.md](14-risks-decisions.md) |
| 14 | Offene Fragen | [15-open-questions-next-steps.md](15-open-questions-next-steps.md) |
| 15 | Empfohlene nächste Schritte | [15-open-questions-next-steps.md](15-open-questions-next-steps.md) |
| – | **Sprint 0 Backlog (MVP1 Setup-Sprint)** | [18-sprint-0-backlog.md](18-sprint-0-backlog.md) |

Zusätzlich: Test- & Akzeptanzkonzept ([16-testing-acceptance.md](16-testing-acceptance.md)) und Traceability-Matrix gegen die Akzeptanzkriterien ([17-traceability.md](17-traceability.md)).

## Multi-Agenten-Vorgehen

Welle 1 (parallel, Fundament): Architektur · Security/DSGVO · Graph-Feasibility/Ingestion · AI-Orchestrierung.
Welle 2 (baut auf Welle 1): BC-Datenmodell + APIs · Outlook + Teams UX · Such-/Matching-Konzept · Test-/Akzeptanzstrategie.
Welle 3 (Synthese): MVP-Roadmap & Aufwand · Risiken/Entscheidungen · Offene Fragen/Next Steps · Traceability.

## Globale Leitplanken

- Stack laut `instructions.md` (BC AL Extension, Custom APIs, Microsoft Graph, Azure Functions/App Service, Azure OpenAI, Azure AI Search, Blob, Key Vault, App Insights, Entra ID).
- Grounded AI mit Quellen, Unsicherheits-Markierung, ohne automatisches externes Senden.
- Berechtigungsprüfung **vor** jeder AI-Zusammenfassung; Mandanten- und Unternehmensgrenzen strikt.
- Nur externe geschäftliche Kommunikation als Default; rein interne Kommunikation ausgeschlossen.
- Sprache der Plandokumente: Deutsch.
- Annahme: Business Central SaaS (Online), Multi-Tenant-Service mit Tenant-Isolation – siehe offene Fragen.
