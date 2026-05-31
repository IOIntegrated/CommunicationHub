# Infrastructure as Code (Bicep)

> Azure-Infrastruktur für den Customer Communication Copilot. **Region: Sweden Central** (EU Data Boundary, Azure OpenAI verfügbar).

**Plandokumente:**
- [docs/plan/01-architecture.md §3](../docs/plan/01-architecture.md) – Container, Topologie
- [docs/plan/12-security-compliance.md](../docs/plan/12-security-compliance.md) – Secrets, Identitäten, Audit

**Owner:** `@TODO-org/communicationhub-devops`

## Layout

```
bicep/
  main.bicep                  Subscription/RG-Einstieg, Module-Aufrufe
  modules/
    keyvault.bicep            Premium KV, RBAC, Private Endpoint (TODO)
    aoai.bicep                Azure OpenAI Account + Deployments (EU)
    aisearch.bicep            Azure AI Search (S2, Vector)
    servicebus.bicep          Service Bus Premium, Topics, Sessions, DLQ
    functions.bicep           Ingestion Functions (Premium EP1)
    appservice.bicep          Copilot API (Linux Container)
    loganalytics.bicep        LA Workspace + dedizierter Audit-Workspace
  parameters/
    dev.bicepparam
    test.bicepparam
    prod.bicepparam
```

## Sprint 0

- [x] Modul-Skelette mit `targetScope`, Parametern und Kommentar-Marker
- [ ] Bicep `what-if` in CI (`.github/workflows/infra-validate.yml`)
- [ ] OIDC Federated Identity zwischen GitHub und Azure einrichten (TODO – siehe [19-repo-scaffolding §5](../docs/plan/19-repo-scaffolding.md))

## Konventionen

- **Region default:** `swedencentral`.
- **Naming:** `<prefix>-<component>-<env>-<region>` (z. B. `cch-api-dev-sc`).
- **Tags:** `env`, `owner`, `costCenter`, `dataClassification`.
- **Identitäten:** ausschließlich **Managed Identity** für Azure-zu-Azure; **OIDC Federated Identity** für CI.
- **CMK:** Customer-Managed Keys für Storage/Search/SB wo verfügbar (Sprint 1).
- **Private Endpoints:** Pflicht für KV, Storage, SB, Search, Cosmos (Sprint 1).

## Deploy (Sprint 1)

```powershell
az deployment sub create `
  --location swedencentral `
  --template-file bicep/main.bicep `
  --parameters bicep/parameters/dev.bicepparam
```
