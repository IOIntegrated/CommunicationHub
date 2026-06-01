# 06 – Backend-Service Konzept (Communication Copilot Service)

> Bezug: [00-overview.md](00-overview.md) Deliverable Nr. 7 · [01-architecture.md](01-architecture.md) §2–§4 · [03-bc-apis.md](03-bc-apis.md) · [08-ai-orchestration.md](08-ai-orchestration.md) · [12-security-compliance.md](12-security-compliance.md).
>
> Dieses Dokument beschreibt den **Communication Copilot Service** (kurz: Copilot API oder Backend Service) – die synchrone Kernkomponente der Lösung. Separate Spezifikationen für Ingestion ([07](07-ingestion-pipeline.md)), AI ([08](08-ai-orchestration.md)) und BC-APIs ([03](03-bc-apis.md)) ergänzen dieses Dokument.

---

## 1. Zweck & Abgrenzung

| Aspekt | Copilot API (dieses Dokument) | Ingestion Service ([07](07-ingestion-pipeline.md)) |
|--------|-------------------------------|---------------------------------------------------|
| **Aufrufmuster** | Synchron (Request/Response, Streaming) | Asynchron (Event-getrieben, Timer) |
| **Latenzziel** | p95 ≤ 4 s (≤ 25 s mit AOAI-Streaming) | p95 ≤ 60 s E2E Webhook → BC |
| **Auslöser** | Outlook Add-in, Teams App, BC Extension (Benutzeraktion) | Microsoft Graph Change Notification, Timer |
| **Schreibt nach BC** | Nur auf explizite Benutzerfreigabe | Automatisch (nach Consent-Check) |
| **Token-Flow** | OBO (delegierter Benutzer-Token) | Application Permissions (S2S) |
| **Hosting** | Azure App Service Linux (P1v3+) | Azure Functions Premium EP1 |

Der Copilot API-Service hat **keinen eigenständigen Persistenz-Speicher** für fachliche Daten; alle persistenten Daten liegen in BC (über Custom APIs), Azure AI Search (Indizes), und Blob (Roh-Payloads). Der Service ist zustandslos abzüglich Token-Cache und kurzlebigem Tenant-Konfigurations-Cache.

---

## 2. Technologie-Stack

| Schicht | Technologie | Begründung |
|---------|-------------|-----------|
| Framework | **ASP.NET Core 8** (Linux Container) | LTS, Minimal API + Controller, native gRPC, SSE/HTTP-Streaming, hohe Testbarkeit |
| AI-Orchestrierung | **Microsoft Semantic Kernel 1.x** | Unified Plugin-Modell, Function Calling, Streaming, Memory-Abstraktion |
| HTTP-Client-Resilienz | **Polly 8 (Microsoft.Extensions.Resilience)** | Retry + Exponential Backoff + Circuit Breaker + Timeout per Downstream |
| Logging | **Serilog** (sink: App Insights) | Strukturiertes Logging, Korrelations-Enricher, PII-Destructuring |
| Auth | **Microsoft.Identity.Web** | MSAL, OBO-Flow, App-Roles-Middleware, Tenant-Isolation |
| Konfig + Secrets | **Azure App Configuration + Key Vault** | Feature Flags per Tenant, Secrets über Managed Identity |
| Caching | **IMemoryCache / IDistributedCache (Redis optional)** | Token-Cache (Managed Identity), Tenant-Config-Cache (TTL 5 min) |
| Container | **Docker (Linux, .NET 8 base image)** | Reproduzierbarer Build, App Service Web App for Containers |
| CI/CD | GitHub Actions / Azure DevOps (Bicep) | Slot-Deployment (staging → production), Canary via Front Door |

---

## 3. Endpunkt-Übersicht

Alle Endpunkte unter `/v1/`. Authentifizierung: **Bearer-Token (Entra ID)**; OBO-Flow für delegierte Calls. Pflicht-Header: `x-correlation-id`, `x-ccc-tenant`, `x-ccc-bc-company` (siehe [03 §1](03-bc-apis.md)).

> **Wichtig:** Der Copilot API-Service sendet **niemals** E-Mails, Teams-Nachrichten oder externe Aktionen automatisch. Alle schreibenden Operationen erfordern explizite Benutzerfreigabe in der aufrufenden UI (Add-in / Teams App / BC).

### 3.1 Mail-Analyse (Outlook Add-in)

#### `POST /v1/mail/analyze`

Analysiert eine einzelne E-Mail im Kontext des aufrufenden Benutzers. Haupteinstiegspunkt für das Outlook Add-in.

**Request:**
```json
{
  "messageId": "AAMkAGI2THVSAAA=",
  "mailboxHint": "user@contoso.com",
  "includeSuggestions": true,
  "streamResponse": false
}
```

**Ablauf (intern):**
1. Token-Validierung + OBO → Graph-Token.
2. `GET /me/messages/{id}` (Betreff, Snippet, Absender, Empfänger, Anhangsliste).
3. Berechtigungsprüfung: Consent-Status für Postfach gegen BC Custom API (`GET setup/consentStatus`).
4. Matching: BC Custom API `POST matching/suggestCustomer` → Kandidatenliste.
5. Retrieval: Azure AI Search Hybrid-Query (Tenant/Company-Filter, Visibility-Scope-Filter).
6. AI-Orchestrierung (Capabilities C1, C2, C3 – Streaming optional).
7. Audit-Event schreiben (App Insights + optional BC `POST audit`).

**Response:**
```json
{
  "analysisId": "guid",
  "classification": { "isExternal": true, "channel": "Email", "sensitivity": "Internal", "confidence": 0.95 },
  "extraction": {
    "participants": [{ "email": "...", "role": "From", "isExternal": true, "bcMatchHint": "C00100" }],
    "actionItems": [{ "description": "Angebot bis Freitag senden", "dueDateHint": "2026-06-07", "confidence": 0.9 }],
    "topics": [{ "label": "Angebotsanfrage", "confidence": 0.88 }]
  },
  "customerMatch": {
    "candidates": [{ "no": "C00100", "name": "Mustermann GmbH", "confidence": 0.92, "evidence": "Domain match" }]
  },
  "replySuggestion": {
    "short": "Vielen Dank für Ihre Anfrage. Wir werden uns bis Freitag melden.",
    "citations": ["src://bc/customer/C00100", "src://search/mail-abc123"],
    "containsCommitments": false,
    "language": "de",
    "confidence": 0.87
  },
  "sources": [
    { "id": "src://search/mail-abc123", "title": "RE: Angebot Projekt X", "excerpt": "...", "sourceType": "Email" }
  ],
  "audit": { "decisionId": "guid", "modelDeployment": "gpt-4.1-eu", "tokenCount": 1240 }
}
```

**Streaming:** Wenn `streamResponse=true`, antwortet der Endpunkt mit `Content-Type: text/event-stream` (SSE). Felder `replySuggestion.short` und `replySuggestion.long` werden token-weise gestreamt; alle anderen Felder werden im letzten Event mitgeliefert.

---

#### `POST /v1/interactions`

Legt eine Interaction in BC an (nach expliziter Benutzerfreigabe im Add-in).

**Request:**
```json
{
  "messageId": "AAMkAGI2THVSAAA=",
  "analysisId": "guid",
  "entityLinks": [{ "entityType": "Customer", "entityNo": "C00100", "confidence": 0.92 }],
  "summaryRef": "guid",
  "replyDraftRef": "guid",
  "userConfirmed": true
}
```

Der Service validiert `userConfirmed == true`, prüft die Sitzungskorrelation (`analysisId`) und ruft dann BC Custom API `POST interactions` auf.

---

### 3.2 Teams-Nachricht analysieren

#### `POST /v1/teams/message/analyze`

Analysiert eine Teams-Nachricht (Message Extension Action Command). Ablauf analog zu `/v1/mail/analyze`, aber:
- Graph-Call: `GET /chats/{id}/messages/{id}` (delegiert; benötigt `Chat.ReadBasic` oder Teams-Kontext).
- Prüfung auf externe Teilnehmer (Gast-UPNs, externe Federation).
- Bei rein interner Nachricht: Hinweis zurückgeben, kein AI-Aufruf ohne Benutzerbestätigung.

---

### 3.3 Kontext-Aggregation

#### `GET /v1/context/customer/{customerNo}`

Aggregiert Kundenstammdaten, offene Belege, letzte Interactions, offene Aufgaben aus BC und sucht relevante Mails/Docs aus AI Search. Für Outlook-Side-Panel und Teams-Tab.

**Response:** Kombination aus BC Custom API `GET context/customer({no})` und AI Search Top-K.

#### `GET /v1/context/contact/{contactNo}`
#### `GET /v1/context/project/{jobNo}`

Analog. Antwort-Schema entspricht dem BC-API-Schema aus [03 §3](03-bc-apis.md).

---

### 3.4 Briefing

#### `POST /v1/briefing/thread`

Erstellt ein Thread-Briefing (C4b) für eine Menge von Interaction-IDs.

#### `POST /v1/briefing/customer`

Erstellt ein Kunden-Briefing (C4c). Asynchron für große Kunden (> 50 Interactions): liefert `202 Accepted + Location`-Header; polling oder Webhook-Callback.

#### `POST /v1/briefing/meeting`

Erstellt ein Meeting-Briefing (C4d) aus einem Transkript/Interaktions-Set.

---

### 3.5 Hilfsfunktionen

| Endpunkt | Beschreibung |
|----------|-------------|
| `GET /v1/health` | Liveness/Readiness (ohne Auth); antwortet `{"status":"ok","dependencies":{...}}` |
| `GET /v1/health/deep` | Deep-Health mit Downstream-Checks (BC, AOAI, Search, KV); Auth erforderlich |
| `POST /v1/feedback` | Benutzer-Feedback zu einem AI-Vorschlag (decisionId + accepted/rejected + freetext) |
| `GET /v1/version` | Deployed-Version, Modell-Deployments, Feature-Flag-Status |

---

## 4. Authentifizierung & Autorisierung

### 4.1 Token-Flows

```
Outlook Add-in / Teams App
   │  SSO-Token (user_impersonation)
   ▼
Copilot API  ──OBO──▶  Microsoft Graph  (Mail.Read, Chat.ReadBasic, …)
             ──OBO──▶  Business Central  (BC-Custom-APIs)
             ──S2S──▶  Azure AI Search  (Query-Key via Managed Identity)
             ──S2S──▶  Azure OpenAI     (Managed Identity)
             ──S2S──▶  Azure Key Vault  (Managed Identity)
```

- **OBO-Flow** (On-Behalf-Of): Der Benutzer-Token des Add-ins/der Teams App wird via `/.well-known/openid-configuration` validiert und dann per MSAL `AcquireTokenOnBehalfOf` gegen einen Graph-Token und einen BC-Token getauscht.
- **App Roles** (in Entra App-Registrierung): `CommHub.User` (Basiszugriff), `CommHub.Admin` (Setup-Endpunkte). Rollen werden als Claims im Token übertragen und per `[Authorize(Roles="CommHub.User")]`-Middleware geprüft.
- **Tenant-Isolation:** Jeder Request wird gegen `x-ccc-tenant` geprüft; der Bearer-Token-Claim `tid` muss übereinstimmen. Weitere Requests in anderen Tenants werden abgewiesen (keine Tenant-Cross-Anfragen).

### 4.2 Pre-AI Berechtigungsprüfung (L5)

Vor jedem AI-Aufruf prüft der `PermissionResolver`-Service:
1. Consent-Status für das betreffende Postfach/die Interaktion (BC Custom API).
2. Visibility-Scope der Interaction gegen Benutzerrolle (Owner / Owner-Team / Company).
3. Bei fehlender Berechtigung: `403 Forbidden` + Audit-Event `permission.denied`.

Retrieval-Ergebnisse aus AI Search enthalten ACL-Felder (`visibility_scope`, `owner_user_sid`, `team_code`). Der Backend-Service filtert den Search-Query serverseitig – die Filterausdrücke werden auf Basis der verifizierten Token-Claims erzeugt, nicht auf Client-Input.

---

## 5. AI-Orchestrierung (Überblick)

Die vollständige Capability-Beschreibung ist in [08-ai-orchestration.md](08-ai-orchestration.md) dokumentiert. Der Copilot API-Service ist der **exklusive Orchestrator** aller synchronen AI-Aufrufe (C1–C8 interaktiv).

### 5.1 Semantic Kernel Plugin-Architektur

```
CopilotOrchestrator
├── BcPlugin          (BC Custom API Calls: Match, Context, Save)
├── SearchPlugin      (Azure AI Search: Hybrid Query, Filter)
├── GraphPlugin       (Microsoft Graph: Mail/Teams detail fetch)
├── AiPlugin          (Azure OpenAI: Chat, Embeddings, structured outputs)
└── AuditPlugin       (App Insights: Audit-Event schreiben)
```

Jedes Plugin ist ein SK-`KernelPlugin` mit `KernelFunction`-attributierten Methoden. Der Orchestrator orchestriert den Aufrufplan in Code (kein Auto-Planner für produktive Flows, um Determinismus zu gewährleisten).

### 5.2 Prompt-Injection-Schutz (C7)

Alle Mail-/Chat-Inhalte werden **vor** Einspeisung ins Prompt durch einen `InjectionClassifier` geprüft:
1. Regex-Vorfilter (System-Instruktionsmarker, Rolle-Override-Muster).
2. LLM-basierter C7-Klassifikator (GPT-4o-mini, separate Capability).
3. Bei Verdacht: Inhalt wird **nicht** ins Haupt-Prompt gegeben; Audit-Event `prompt.injection.detected`; Response enthält `promptInjectionWarning: true`.

### 5.3 Grounding & Quellenpflicht

Jede AI-Response muss `sources[]` mit mindestens einer belegbaren Quelle enthalten (Schema-Validierung via `System.Text.Json` + Custom Validator). Antworten ohne Quellenangabe werden mit `500 InternalError` abgewiesen und in den Dead Letter Store geschrieben.

---

## 6. Resilienz & Fehlerbehandlung

| Downstream | Retry-Policy | Circuit Breaker | Timeout | Fallback |
|-----------|-------------|-----------------|---------|---------|
| Microsoft Graph | 3x Exp. Backoff (1s, 5s, 25s), `Retry-After` für 429 | 5 Fehler/30 s → offen 30 s | 30 s | `503` + Fehlermeldung im Response |
| Azure OpenAI | 3x Exp. Backoff, `Retry-After` für 429 | 3 Fehler/60 s → offen 60 s | 60 s (25 s für Streaming-Abbruch) | Read-only-Kontext (ohne AI), Feature-Flag `copilot.ai.enabled` |
| Azure AI Search | 2x Retry | 5 Fehler/15 s → offen 15 s | 5 s | BC-Daten only (kein Search-Context); degradierter Modus |
| BC Custom APIs | 3x Exp. Backoff | 5 Fehler/30 s → offen 30 s | 20 s | `503` + CacheHit falls verfügbar |
| Key Vault | 2x Retry | 3 Fehler/60 s → offen 120 s | 5 s | Startup-fail (Secrets nicht nachladen); keine Fallback-Secrets |

**Gesamt-Request-Timeout:** 25 s (Streaming: 90 s). Bei Überschreitung: `504 Gateway Timeout` mit `analysisId` für spätere Statusabfrage.

**Idempotenz** bei `POST /v1/interactions`: `Idempotency-Key`-Header wird serverseitig (In-Memory-Cache, TTL 10 min) geprüft; doppelte Requests geben `200` mit gecachtem Ergebnis zurück.

---

## 7. Multi-Tenant-Design

- Jeder Request enthält `x-ccc-tenant` (M365-Tenant-ID) und `x-ccc-bc-company` (BC-Company-System-ID).
- Die `TenantContextMiddleware` lädt (und cached) die Tenant-Konfiguration aus Azure App Configuration:
  - BC-API-Basis-URL für diesen Tenant.
  - Erlaubte Graph-Scopes.
  - Feature-Flag-Overrides per Tenant.
  - CMK-Key-Vault-URL (für Blob-Decryption falls nötig).
- **Daten-Isolation:** Search-Queries enthalten immer `tenant_id`-Filter. BC-API-Calls enthalten immer die `companyId` in der URL. Kein Cross-Tenant-Datenleck ist architektonisch möglich (kein shared Cache ohne Tenant-Trennung).
- **Tenant-Onboarding:** Beim ersten Request eines neuen Tenants löst der Service einen asynchronen Onboarding-Workflow aus (Service Bus `ops.events`), der Search-Indizes, Blob-Container und Cosmos-Items anlegt.

---

## 8. Observability

### 8.1 Structured Logging (Serilog → App Insights)

Pflichtfelder auf jedem Log-Event: `correlationId`, `tenantId`, `userId` (Hash), `eventType`, `component=backend`. Keine Klartexte aus Mail-/Teams-Inhalten in Operations-Logs. Vollständige Logging-Konvention: [20-logging-convention.md](20-logging-convention.md).

### 8.2 Distributed Tracing

W3C Trace Context (`traceparent`, `tracestate`) wird in alle ausgehenden Requests propagiert (Graph, AOAI, Search, BC). Vollständiger Trace ist in App Insights Transaction Search (End-to-End) sichtbar.

### 8.3 Custom Audit Events (App Insights Custom Events)

| Event | Felder |
|-------|-------|
| `ai.suggestion.created` | `decisionId`, `capability`, `modelDeployment`, `tokenCount`, `latencyMs`, `sourceCount`, `tenantId` |
| `ai.suggestion.accepted` | `decisionId`, `feedback`, `tenantId` |
| `permission.denied` | `reason`, `requestedResource`, `tenantId` |
| `prompt.injection.detected` | `severity`, `tenantId` (kein Mailinhalt) |
| `interaction.saved` | `interactionId`, `entityLinkCount`, `tenantId` |

### 8.4 KPIs & SLOs

| KPI | SLO | Fenster |
|-----|-----|---------|
| `/v1/mail/analyze` p95-Latenz (ohne Streaming) | ≤ 4 s | 30 Tage |
| Erfolgsrate | ≥ 99 % (excl. 4xx) | 30 Tage |
| Verfügbarkeit | ≥ 99,9 % | kalendermonatlich |
| `/v1/briefing/customer` Async-Jobdauer p95 | ≤ 30 s | 7 Tage |
| `permission.denied`-Rate | Monitoring only | täglich |

Burn-Rate-Alerts auf SLO 1 und 2 in Azure Monitor.

---

## 9. API-Versionierung & Kompatibilität

- **URL-Versionierung:** `/v1/`, `/v2/` bei Breaking Changes.
- Additive Felder (optional, nullable) sind innerhalb einer Version erlaubt (keine Versionsbumps).
- **`Deprecation`- und `Sunset`-Header** (RFC 8594) bei auslaufenden Versionen; parallele Verfügbarkeit min. 12 Monate.
- OpenAPI-Spec wird auf `GET /openapi/v1.json` ausgeliefert (Swashbuckle/NSwag); BC nutzt CSDL über `$metadata`.

---

## 10. Deployment & Skalierung

- **Hosting:** Azure App Service Linux, Plan **P2v3** (2 Kerne, 8 GB RAM) als Mindestgröße; Zone-Redundant ab Pilot-Go-Live.
- **Min. Instanzen:** 2 (HA), Auto-Scale auf CPU > 70 % / RPS-Metriken.
- **Deployment-Slots:** `staging` → `production` Swap; Canary per Azure Front Door (10 % → 50 % → 100 %).
- **Health Probes:** `GET /v1/health` als Liveness-Probe (alle 30 s); `GET /v1/health/deep` als Startup-Probe (einmalig, 120 s Timeout).
- **Container-Image:** Multi-Stage Dockerfile (build → publish → runtime, non-root user, distroless base).
- **Ressourcen-Anforderungen (prod):** 0,5 vCPU Request / 1 vCPU Limit pro Pod (falls ACA genutzt, sonst App Service Scaling-Plan).

---

## 11. Sicherheitsanforderungen

| Anforderung | Umsetzung |
|-------------|-----------|
| TLS 1.2+ erzwungen | App Service TLS-Mindestversion = 1.2; HSTS-Header |
| Keine Secrets in App Settings | Alle Secrets aus Key Vault via Managed Identity |
| OWASP A01-A10 | Input-Validation via `FluentValidation` auf allen Request-Modellen; SecurityHeaders-Middleware (CSP, X-Frame-Options, etc.) |
| Rate Limiting | `Microsoft.AspNetCore.RateLimiting` (Fixed-Window: 60 req/min pro Tenant/User) |
| Audit-Log PII-frei | Keine Mail-Inhalte, Klartextnamen oder E-Mail-Adressen in Operations-Logs |
| Prompt Injection | C7-Klassifikator vorgelagert (§5.2) |
| Dependency Scanning | Dependabot / `dotnet-outdated` in CI |

Vollständiges Sicherheitskonzept: [12-security-compliance.md](12-security-compliance.md).

---

## 12. Abhängigkeiten & Schnittstellen

| Downstream | Protokoll | Auth | Zweck |
|-----------|-----------|------|-------|
| Microsoft Graph | HTTPS/REST | OBO (delegiert) | Mail- und Teams-Daten lesen |
| Business Central Custom APIs | OData v4 / HTTPS | OBO (delegiert) oder S2S (App-Permission) | Stammdaten, Interactions schreiben/lesen |
| Azure AI Search | HTTPS/REST | Managed Identity (Query-Reader) | Hybrid-Suche über alle Indizes |
| Azure OpenAI | HTTPS/REST | Managed Identity | Chat Completions, Embeddings |
| Azure Key Vault | HTTPS | Managed Identity | Secrets, Feature-Keys |
| Azure App Configuration | HTTPS | Managed Identity | Feature Flags, Tenant-Konfig |
| Azure Service Bus | AMQP / HTTPS | Managed Identity (Sender) | Async-Briefing-Jobs, Onboarding-Events |
| Application Insights | HTTPS / SDK | Instrumentation Key (env var) | Telemetrie, Audit |

---

## 13. Projekt-Struktur (Skeleton)

```
src/backend/
├── Directory.Build.props          # net8.0, Nullable, TreatWarningsAsErrors
├── global.json                    # SDK-Version-Pin
├── CommunicationHub.Backend.sln   # (wird in Sprint 1 erstellt)
└── src/
    ├── CommunicationHub.Backend.Api/
    │   ├── Program.cs             # Minimal API Host, DI, Middleware
    │   ├── Endpoints/             # /v1/mail, /v1/teams, /v1/context, …
    │   ├── Middleware/            # TenantContext, RateLimit, SecurityHeaders
    │   └── Models/                # Request/Response DTOs
    └── CommunicationHub.Backend.Core/
        ├── AI/                    # CopilotOrchestrator, Plugins (SK)
        ├── Auth/                  # PermissionResolver, OBO-Helper
        ├── BC/                    # BcApiClient, BcEntityMapper
        ├── Graph/                 # GraphMailClient, GraphTeamsClient
        ├── Search/                # SearchClient, QueryBuilder
        └── Resilience/            # PollyPolicies, CircuitBreakerConfig
```

---

## 14. Offene Punkte (Sprint 0 → Sprint 1)

| # | Frage | Priorität |
|---|-------|-----------|
| B-1 | APIM oder direkte App-Service-Exposition für Outlook/Teams-Clients? | Hoch |
| B-2 | Benötigt das Add-in `streamResponse=true` ab MVP1 oder erst MVP2? | Mittel |
| B-3 | Redis für Token-Cache in Prod (Multi-Instanz) oder In-Memory ausreichend für Pilot (2 Instanzen)? | Mittel |
| B-4 | Dedizierter Audit-Workspace (immutable) oder App-Insights-Custom-Table mit Lock? | Hoch (DSGVO) |
| B-5 | `x-ccc-bc-company` vom Add-in oder server-side aus Tenant-Config ableiten? | Mittel |

---

## 15. Änderungshistorie

| Version | Datum | Änderung |
|---------|-------|---------|
| 0.1.0 | 2026-06-01 | Initial (Sprint 0, Deliverable Nr. 7 aus 00-overview.md) |
