# Customer Communication Copilot

Mono-Repo für den **Customer Communication Copilot** – eine Lösung, die externe Kundenkommunikation (E-Mail, Teams, Meetings, Dokumente) erfasst, an Microsoft Dynamics 365 Business Central anbindet und Benutzer mit *Grounded AI* aktiv bei der Bearbeitung unterstützt. **Keine automatischen ausgehenden Nachrichten** – alle Vorschläge sind freigabepflichtig.

> Status: **MVP1 Sprint 0** – Repo-Scaffolding. Funktionalität folgt ab Sprint 1.

## Komponenten (Übersicht)

| Pfad | Komponente | Plandokument |
|------|-----------|--------------|
| [src/bc-extension/](src/bc-extension/) | Business Central Extension (AL) | [docs/plan/02-bc-data-model.md](docs/plan/02-bc-data-model.md), [03-bc-apis.md](docs/plan/03-bc-apis.md) |
| [src/backend/](src/backend/) | Communication Copilot Service (.NET 8, App Service) | [docs/plan/01-architecture.md](docs/plan/01-architecture.md) |
| [src/ingestion/](src/ingestion/) | Ingestion-Pipeline (Azure Functions Premium) | [docs/plan/07-ingestion-pipeline.md](docs/plan/07-ingestion-pipeline.md) |
| [src/ai-orchestrator/](src/ai-orchestrator/) | Prompts + Eval-Runner | [docs/plan/08-ai-orchestration.md](docs/plan/08-ai-orchestration.md) |
| [src/outlook-addin/](src/outlook-addin/) | Outlook Add-in (Office.js / React) | [docs/plan/04-outlook-addin.md](docs/plan/04-outlook-addin.md) |
| [src/teams-app/](src/teams-app/) | Teams App (Bot + Message Extension + Tab) | [docs/plan/05-teams-app.md](docs/plan/05-teams-app.md) |
| [infra/](infra/) | Bicep IaC (Azure EU – Sweden Central) | [docs/plan/01-architecture.md](docs/plan/01-architecture.md), [12-security-compliance.md](docs/plan/12-security-compliance.md) |
| [.github/workflows/](.github/workflows/) | CI/CD (GitHub Actions, OIDC Federated Identity) | [docs/plan/19-repo-scaffolding.md](docs/plan/19-repo-scaffolding.md) |
| [docs/](docs/) | Planung, ADRs, Runbooks | [docs/plan/00-overview.md](docs/plan/00-overview.md) |

## Dokumentation

- **Auftrag (Source of Truth):** [instructions.md](instructions.md)
- **Planungsübersicht:** [docs/plan/00-overview.md](docs/plan/00-overview.md)
- **Architektur:** [docs/plan/01-architecture.md](docs/plan/01-architecture.md)
- **MVP-Roadmap:** [docs/plan/13-mvp-roadmap.md](docs/plan/13-mvp-roadmap.md)
- **Security & Compliance:** [docs/plan/12-security-compliance.md](docs/plan/12-security-compliance.md)
- **Repo-Scaffolding-Konventionen:** [docs/plan/19-repo-scaffolding.md](docs/plan/19-repo-scaffolding.md)

## Lokale Entwicklung (Kurzfassung)

Voraussetzungen (siehe [docs/plan/19-repo-scaffolding.md](docs/plan/19-repo-scaffolding.md) §6):

- .NET 8 SDK
- Node.js 20 LTS + pnpm (oder npm)
- Visual Studio Code mit **AL Language**-Extension
- BC SaaS Sandbox + Container-/Online-Sandbox
- Azure CLI + Bicep CLI
- PowerShell 7+

```powershell
# Erst-Setup (Skelett – Funktionalität folgt)
./scripts/bootstrap-dev.ps1
```

## Sicherheit & Verantwortlichkeiten

- **Federated Identity (OIDC)** statt Secrets in CI/CD – siehe [docs/plan/19-repo-scaffolding.md §5](docs/plan/19-repo-scaffolding.md).
- **Managed Identity** für alle Azure-zu-Azure-Aufrufe.
- Sicherheits-Issues bitte gemäß [SECURITY.md](SECURITY.md) melden.
- Code-Owner siehe [CODEOWNERS](CODEOWNERS).
- Beitragsregeln siehe [CONTRIBUTING.md](CONTRIBUTING.md).

## Lizenz

TODO – Lizenzwahl offen. Siehe [LICENSE](LICENSE).