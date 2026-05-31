# Backend – Communication Copilot Service

> Synchrone Copilot-API: `/context`, `/suggest-reply`, `/summary`, `/match`, `/interactions`. Trägt latenzkritische Pfade für Outlook Add-in, Teams App und BC.

**Plandokumente:**
- [docs/plan/01-architecture.md §3–5](../../docs/plan/01-architecture.md) – Container, Endpunkte, Sequenzen
- [docs/plan/08-ai-orchestration.md](../../docs/plan/08-ai-orchestration.md) – Capabilities C1–C8
- [docs/plan/12-security-compliance.md §6](../../docs/plan/12-security-compliance.md) – Pre-AI-Permission-Resolver

**Owner:** `@TODO-org/communicationhub-backend`

**Stack:** .NET 8 (ASP.NET Core), Linux Container, Azure App Service. Semantic Kernel für AI-Orchestrierung. Managed Identity gegen Key Vault / OpenAI / Search.

## Projektstruktur (geplant)

```
src/
  CommunicationHub.Backend.Api/        ASP.NET Core Host, Controller, Auth-Middleware
  CommunicationHub.Backend.Core/       Domain, Orchestrierung, AI-Capabilities, Permission Resolver
tests/
  CommunicationHub.Backend.Tests/      xUnit – Unit + Integration
```

## Sprint 0 (dieses Repo)

- [x] Ordnerstruktur + `Directory.Build.props` + `global.json`
- [ ] Echte `.sln` und Projekte (`.csproj`) anlegen → siehe `CommunicationHub.Backend.sln.md`
- [ ] OIDC-Deployment-Workflow → `.github/workflows/backend-ci.yml`

## Sprint 1 (geplant)

- ASP.NET-Core-Host mit Health-Checks, OBO-Middleware (Entra ID).
- `/context`, `/suggest-reply`, `/summary`, `/match` Endpunkt-Skelette.
- Integrationen: Graph (delegated/OBO), BC Custom APIs (S2S), Azure OpenAI, Azure AI Search.
- Pre-AI-Permission-Resolver (Pflicht vor jedem OpenAI-Call, [12-security §6](../../docs/plan/12-security-compliance.md)).
- App Insights mit `ai.suggestion.*`-Custom-Events.

## Build / Test (Sprint 1)

```powershell
dotnet restore
dotnet build
dotnet test
```
