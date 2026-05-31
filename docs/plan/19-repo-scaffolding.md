# 19 – Repo-Scaffolding & CI/CD-Konventionen

> Bezug: [00-overview.md](00-overview.md), [01-architecture.md](01-architecture.md), [12-security-compliance.md](12-security-compliance.md), [13-mvp-roadmap.md](13-mvp-roadmap.md), [16-testing-acceptance.md](16-testing-acceptance.md).
>
> Status: **MVP1 Sprint 0** – diese Datei beschreibt, **was im Repo angelegt wurde**, und legt die Konventionen für die folgenden Sprints fest. Sämtliche angelegten Strukturen sind **Skelette**; Funktionalität folgt ab MVP1 Sprint 1.

## 1. Repo-Layout

```
CommunicationHub/
├── .editorconfig
├── .gitattributes
├── .gitignore
├── CODEOWNERS
├── CONTRIBUTING.md
├── LICENSE                          (TODO – Lizenzwahl offen)
├── README.md
├── SECURITY.md
├── instructions.md                  (Source of Truth)
│
├── .github/
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug.md
│   │   ├── feature.md
│   │   └── security.md
│   ├── pull_request_template.md
│   └── workflows/
│       ├── ai-eval.yml              (manuell + Prompt-PRs)
│       ├── backend-ci.yml           (.NET 8 build/test)
│       ├── bc-extension-ci.yml      (AL – TODO AL-Go Migration)
│       ├── codeql.yml               (JS/TS + C#)
│       ├── dependency-review.yml    (PR-Gate)
│       ├── infra-validate.yml       (bicep build + what-if)
│       ├── ingestion-ci.yml         (Functions build/test)
│       ├── outlook-addin-ci.yml     (Node 20)
│       └── teams-app-ci.yml         (Node 20)
│
├── docs/
│   ├── adr/README.md                (ADR-Ablage, MADR-Format)
│   ├── runbooks/README.md           (Pflicht ab MVP1)
│   └── plan/                        (00–17, 19 – diese Datei)
│
├── infra/
│   ├── README.md
│   └── bicep/
│       ├── main.bicep
│       ├── modules/
│       │   ├── aisearch.bicep
│       │   ├── aoai.bicep
│       │   ├── appservice.bicep
│       │   ├── functions.bicep
│       │   ├── keyvault.bicep
│       │   ├── loganalytics.bicep   (inkl. dediziertem Audit-Workspace)
│       │   └── servicebus.bicep
│       └── parameters/
│           ├── dev.bicepparam
│           ├── prod.bicepparam
│           └── test.bicepparam
│
├── scripts/
│   ├── README.md
│   └── bootstrap-dev.ps1
│
└── src/
    ├── ai-orchestrator/
    │   ├── README.md
    │   ├── eval/{adversarial,datasets,runners}/
    │   └── prompts/{capabilities,system}/
    ├── backend/
    │   ├── README.md
    │   ├── Directory.Build.props
    │   ├── global.json
    │   ├── CommunicationHub.Backend.sln.md  (TODO: echte .sln in Sprint 1)
    │   ├── src/CommunicationHub.Backend.Api/
    │   ├── src/CommunicationHub.Backend.Core/
    │   └── tests/CommunicationHub.Backend.Tests/
    ├── bc-extension/
    │   ├── README.md
    │   ├── app.json
    │   ├── .vscode/launch.json
    │   └── src/
    ├── ingestion/
    │   ├── README.md
    │   ├── host.json
    │   ├── src/CommunicationHub.Ingestion.Functions/
    │   ├── src/CommunicationHub.Ingestion.Core/
    │   └── tests/CommunicationHub.Ingestion.Tests/
    ├── outlook-addin/
    │   ├── README.md
    │   ├── manifest.xml
    │   ├── package.json
    │   └── src/
    └── teams-app/
        ├── README.md
        ├── package.json
        ├── manifest/manifest.json
        ├── bot/, tab/, extension/
```

## 2. Komponenten-Owner & zuständige Plandokumente

| Komponente | Pfad | Owner (TODO Sprint 0) | Plandokument(e) |
|------------|------|------------------------|------------------|
| BC Extension (AL) | `src/bc-extension/` | `@TODO-org/communicationhub-bc` | [02-bc-data-model.md](02-bc-data-model.md), [03-bc-apis.md](03-bc-apis.md) |
| Backend (Copilot API) | `src/backend/` | `@TODO-org/communicationhub-backend` | [01-architecture.md](01-architecture.md), [08-ai-orchestration.md](08-ai-orchestration.md) |
| Ingestion (Functions) | `src/ingestion/` | `@TODO-org/communicationhub-backend` | [07-ingestion-pipeline.md](07-ingestion-pipeline.md), [11-graph-feasibility.md](11-graph-feasibility.md) |
| AI-Orchestrator (Prompts & Eval) | `src/ai-orchestrator/` | `@TODO-org/communicationhub-ai` | [08-ai-orchestration.md](08-ai-orchestration.md), [16-testing-acceptance.md](16-testing-acceptance.md) |
| Outlook Add-in | `src/outlook-addin/` | `@TODO-org/communicationhub-frontend` | [04-outlook-addin.md](04-outlook-addin.md) |
| Teams App | `src/teams-app/` | `@TODO-org/communicationhub-frontend` | [05-teams-app.md](05-teams-app.md) |
| Infrastructure (Bicep) | `infra/` | `@TODO-org/communicationhub-devops` | [01-architecture.md](01-architecture.md), [12-security-compliance.md](12-security-compliance.md) |
| CI/CD & Repo-Konventionen | `.github/`, root | `@TODO-org/communicationhub-devops` | dieses Dokument |
| Security & Compliance (Querschnitt) | – | `@TODO-org/communicationhub-security` | [12-security-compliance.md](12-security-compliance.md) |
| Architektur (Querschnitt) | – | `@TODO-org/communicationhub-architects` | [00-overview.md](00-overview.md), [01-architecture.md](01-architecture.md), [14-risks-decisions.md](14-risks-decisions.md) |

## 3. Branching-Strategie

- **Trunk-based Development.** `main` ist jederzeit deploybar.
- **Feature-Branches** sind kurzlebig (Ziel < 3 Tage) und gehören genau einer Person/Feature:
  - `feat/<kurz>`
  - `fix/<kurz>`
  - `chore/<kurz>`
  - `docs/<kurz>`
  - `ci/<kurz>`
- **Pull Requests sind Pflicht** für jede Änderung an `main`.
- **Mindest-Reviewer:**
  - 1 Reviewer für Standard-Änderungen.
  - 2 Reviewer für sicherheits-/datenmodell-/IaC-relevante Pfade
    (`src/bc-extension/**`, `infra/**`, `.github/workflows/**`, `SECURITY.md`, `docs/plan/12-security-compliance.md`).
- **Conventional Commits** (`feat:`, `fix:`, `chore:`, `docs:`, `ci:`, `refactor:`, `perf:`, `test:`, `build:`, `style:`, `revert:`).
- **Squash-Merge** bevorzugt (sauberer Trunk-History).
- **Keine Force-Pushes auf `main`.** Keine direkte Bypass-Option, auch nicht für Admins (TODO Sprint 0: Branch-Protection-Regel setzen).
- **Versionierung:** SemVer pro Komponente; Tags `<component>-vX.Y.Z` (z. B. `backend-v0.1.0`, `bc-extension-v0.1.0`).

## 4. CI/CD-Stages und -Trigger

> Alle Workflows liegen unter `.github/workflows/`. Alle Workflows tragen `permissions: id-token: write`, um in Sprint 1 OIDC gegen Azure nutzen zu können (siehe §5).

| Workflow | Trigger | Stages (Skelett heute → MVP1 Sprint 1) | Gates | Test-Doc |
|----------|---------|-----------------------------------------|-------|----------|
| `backend-ci.yml` | push/PR `src/backend/**`, manuell | `build` → `test` → `containerize` → `push-acr` → `deploy-dev` | PR: green build + tests. Deploy: OIDC-Login + Reviewer-Gate für `test`/`prod`. | [16 §3, §5](16-testing-acceptance.md) |
| `ingestion-ci.yml` | push/PR `src/ingestion/**`, manuell | `build` → `test` → `zip` → `func-deploy-dev` | wie backend | [16 §3, §5](16-testing-acceptance.md) |
| `bc-extension-ci.yml` | push/PR `src/bc-extension/**`, manuell | `al-compile` → `al-test` (AL-Go) → `publish-app` | App-File signiert; AppSource-Validator (Sprint 2) | [16 §3](16-testing-acceptance.md) |
| `outlook-addin-ci.yml` | push/PR `src/outlook-addin/**` | `lint` → `build` → `test` → `validate-manifest` → `deploy-static` | grüne CI; Manifest-Validierung | [16 §3](16-testing-acceptance.md) |
| `teams-app-ci.yml` | push/PR `src/teams-app/**` | `lint` → `build` → `test` → `validate-manifest` → `package-zip` | grüne CI; Teams-Manifest validiert | [16 §3](16-testing-acceptance.md) |
| `infra-validate.yml` | push/PR `infra/**`, manuell (env) | `bicep build` → `lint` → `what-if (env)` → `deploy (env)` | what-if Review im PR; `prod` mit Reviewer-Gate (GitHub Environment) | [16 §4](16-testing-acceptance.md) |
| `ai-eval.yml` | manuell, PR `src/ai-orchestrator/**` | `eval-runner` (C1–C8 oder Auswahl) → Report-Artifact | Schwellen MVP1: C1 ≥ 0,80 / C2 F1 ≥ 0,80 / C3 Faithfulness ≥ 0,95 / C4 Coverage ≥ 0,90 | [16 §6](16-testing-acceptance.md) |
| `codeql.yml` | push/PR main, wöchentlich | `analyze` (JS/TS, C#) | Findings ≥ High blockieren PR | [12-security](12-security-compliance.md) |
| `dependency-review.yml` | PR main | `dependency-review-action` | `fail-on-severity: high` | [12-security](12-security-compliance.md) |

**Stage-Modell (über alle Deployment-Workflows hinweg):**

1. **CI** (PR): Build, Lint, Unit-Tests, Static Analysis. Kein Azure-Login.
2. **Dev-Deploy** (auto bei Merge auf `main`): OIDC-Login → Deploy in `dev`-Subscription/RG. Smoke-Tests.
3. **Test-Deploy** (manuell): Reviewer-Gate via GitHub Environment `test`. E2E-Suite gem. [16 §5](16-testing-acceptance.md).
4. **Prod-Deploy** (manuell): zwei Reviewer + Required-Approval; Release-Gates gem. [13-mvp-roadmap.md §2.4](13-mvp-roadmap.md).

## 5. Secrets & Identity – Federated Identity (OIDC)

**Grundsatz:** Keine Secrets im Repo, keine Service-Principal-Client-Secrets in GitHub Secrets, keine PATs.

- **CI/CD ↔ Azure:** OIDC Federated Identity ([Doku](https://learn.microsoft.com/azure/active-directory/workload-identities/workload-identity-federation)).
  - Eine Entra-App pro Umgebung (`cch-gha-dev`, `cch-gha-test`, `cch-gha-prod`).
  - Federated Credentials gebunden an `repo:<org>/CommunicationHub:environment:<env>` und/oder `repo:...:ref:refs/heads/main`.
  - In Workflows: `permissions: id-token: write` (überall gesetzt) + `azure/login@v2` mit `client-id`/`tenant-id`/`subscription-id` aus GitHub-**Variables** (nicht Secrets).
- **Azure ↔ Azure:** ausschließlich **Managed Identity**. App Service / Functions / Container Apps → KV, AOAI, Search, Blob, SB, Cosmos via RBAC.
- **Azure ↔ Microsoft Graph / BC:** Entra-App-Registrierungen mit Zertifikat (in Key Vault, automatisch rotiert); **kein** `clientSecret`.
- **CI/CD ↔ GitHub:** GitHub-native OIDC. Keine PATs zum Triggern anderer Workflows.
- **Lokale Entwicklung:** `az login` (interaktiv), `az account set --subscription dev`, KV-Zugriff via `azure-cli`-Login bzw. `DefaultAzureCredential`. Keine geteilten Service-Account-Credentials.

Konkrete Workflows: alle aktuellen Workflows tragen bereits `permissions: id-token: write` als Skelett. Die Schritte `azure/login@v2` sind in den jeweiligen YAML-Dateien als TODO-Kommentar vorgemerkt.

## 6. Lokale Entwicklungsumgebung

| Werkzeug | Version | Zweck |
|----------|---------|-------|
| .NET SDK | 8.x (siehe `src/backend/global.json`) | Backend + Ingestion Build |
| Node.js | 20 LTS | Outlook Add-in, Teams App |
| pnpm (oder npm) | 9.x | Frontend-Paketverwaltung |
| Visual Studio Code | aktuell | Primäre IDE |
| AL Language Extension | aktuell | BC AL-Entwicklung |
| BC SaaS Sandbox | aktuell | Manuelle Tests gegen Online-Tenant |
| Azure CLI | aktuell | OIDC-/Sub-Setup, Deploys |
| Bicep CLI | aktuell (`az bicep install`) | IaC build / what-if |
| PowerShell | 7.x | `scripts/bootstrap-dev.ps1` |
| Git | aktuell | – |
| GitHub CLI (`gh`) | optional | PR-/Workflow-Komfort |

Erst-Setup: `./scripts/bootstrap-dev.ps1` (Skelett).

## 7. Coding-Konventionen (Kurzfassung)

> Detaillierte Coding-Standards werden in Sprint 1 als ADRs / `docs/standards/`-Sektion ergänzt.

- **AL:** `Codeunit`, `Page`, `Table` mit Präfix `IOI_COMM_HUB_`; `PermissionSet` `IOI_COMM_HUB_*`. Keine `Codeunit ...` ohne `Permissions = ...`-Deklaration. `Caption`/`ToolTip` für jede UI-Property. Kein `WITH`-Statement (Feature `NoImplicitWith`).
- **.NET / C#:** `TreatWarningsAsErrors=true`, Nullable enabled, `Async`-Methoden mit `Async`-Suffix, Dependency Injection via `Microsoft.Extensions.DependencyInjection`, Logging via `ILogger<T>`, Konfiguration via `IOptions<T>` (kein direkter `Configuration["..."]`-Zugriff in Business-Code).
- **TypeScript:** `strict: true`, kein `any`, ESLint mit `@typescript-eslint/recommended-type-checked`, Prettier, React Functional Components + Hooks, `zod` o. ä. für externes Input-Validating, keine `console.log` in Produktion (Logger-Abstraktion).
- **Bicep:** `targetScope` explizit, alle Parameter mit `@description`, keine harten Strings für Regionen/SKUs (Parameter), `disableLocalAuth = true` wo verfügbar, `publicNetworkAccess = 'Disabled'` als Default, Tags konsistent.
- **YAML (Workflows):** `permissions:` explizit (least-privilege), `concurrency:` gesetzt, Action-Pinning auf Major-Version (`@v4`) – ggf. später SHA-Pinning.
- **Markdown:** UTF-8, LF, deutsche Bezeichner in Doku, englische Bezeichner im Code.

## 8. Hinweis

Alle in diesem Sprint angelegten Strukturen sind **Skelette mit klaren TODO-Markierungen**.
**Keine** produktiven IDs (App-IDs, Publisher, Teams-App-ID), **keine** Secrets, **keine**
funktionale Implementierung. Funktionalität folgt ab **MVP1 Sprint 1** gemäß
[13-mvp-roadmap.md](13-mvp-roadmap.md).
