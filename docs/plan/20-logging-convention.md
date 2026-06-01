# 20 – Logging Convention

> Sprint 0 Deliverable, WS-H Task H5.  
> Bezug: [01-architecture.md](01-architecture.md) §10, [12-security-compliance.md](12-security-compliance.md) §7.

---

## 1. Ziele

- **Einheitlichkeit**: Alle Komponenten (Backend, Ingestion, BC, Add-ins) produzieren strukturierte Logs mit denselben Feldern.
- **Korrelierbarkeit**: Jede Anfrage ist via `correlationId` von der Ingestion über den Backend-Service bis in BC nachverfolgbar.
- **PII-Freiheit in Operations-Logs**: Klartext-PII (E-Mail-Adressen, Namen, Betreffs) erscheint **nicht** in Application Insights oder Log Analytics. Nur im dedizierten **Audit-Workspace** (append-only, RBAC eingeschränkt) sind gehashte Quellreferenzen erlaubt.
- **Auditierbarkeit**: Sicherheits- und Compliance-Events werden sowohl im BC Audit Log (Tab. 50012) als auch im Azure Audit Workspace doppelt geschrieben.

---

## 2. Log-Levels

| Level | Wann | Beispiele |
|-------|------|-----------|
| **Trace** | Fein-granulares Debugging (nur Dev/Test) | Einzelne Pipeline-Stages, HTTP-Request-Details |
| **Debug** | Entwicklungszeit-Diagnose | Retry-Versuche, Cache-Treffer/-Fehlschläge |
| **Information** | Regulärer Betriebszustand | Subscription erstellt, Interaction persistiert, AI-Aufruf abgeschlossen |
| **Warning** | Unerwarteter, aber verkraftbarer Zustand | Throttle-Backoff, DLQ-Eintrag, Consent abgelaufen |
| **Error** | Fehler, der manuelles Eingreifen erfordert | BC API nicht erreichbar, AOAI-Timeout, Dedup-Konflikt |
| **Critical** | Systemausfall, sofortiger Alert | Key Vault unerreichbar, Service Bus ausgefallen |

In **Production**: Minimum-Level `Information`. Debug und Trace nur in Dev/Test.

---

## 3. Pflichtfelder pro Log-Event

Alle strukturierten Log-Events (JSON) müssen folgende Felder enthalten:

| Feld | Typ | Beschreibung |
|------|-----|-------------|
| `timestamp` | ISO 8601 UTC | Zeitpunkt des Events. |
| `level` | string | Trace / Debug / Information / Warning / Error / Critical |
| `message` | string | Kurze, menschenlesbare Beschreibung (kein PII). |
| `component` | string | Komponent-Bezeichner: `ingestion`, `backend`, `bc-ext`, `outlook-addin`, `teams-app`, `ai-orchestrator` |
| `correlationId` | GUID | Ende-zu-Ende-Korrelation; aus dem eingehenden HTTP-Header `X-Correlation-Id` übernehmen, sonst neu erstellen. |
| `tenantId` | GUID (optional) | M365-Tenant-ID, wenn bekannt. |
| `eventType` | string | Maschinenlesbarer Event-Code (siehe §4). |

Zusätzliche Felder für AI-bezogene Events:

| Feld | Typ | Beschreibung |
|------|-----|-------------|
| `modelDeployment` | string | z. B. `gpt-4.1-eu` |
| `tokenCount` | integer | Gesamte Token (Prompt + Completion) |
| `latencyMs` | integer | Antwortzeit in Millisekunden |
| `promptHash` | string | SHA-256 des gesendeten Prompt-Inhalts (kein Klartext) |
| `outputHash` | string | SHA-256 des empfangenen Outputs (kein Klartext) |

---

## 4. Standard-Event-Codes (`eventType`)

| Code | Level | Beschreibung |
|------|-------|-------------|
| `subscription.created` | Info | Graph Change Notification Subscription angelegt. |
| `subscription.renewed` | Info | Subscription verlängert. |
| `subscription.removed` | Warn | Subscription unerwartet entfernt (Lifecycle Notification). |
| `webhook.received` | Info | Roh-Event von Graph empfangen, in Queue gestellt. |
| `stage.started` | Debug | Ingestion-Stage gestartet (enthält Stage-Nr. und Interaction-Hash). |
| `stage.completed` | Debug | Ingestion-Stage abgeschlossen. |
| `stage.failed` | Error | Stage fehlgeschlagen; geht in DLQ. |
| `interaction.persisted` | Info | Interaction in BC persistiert. |
| `interaction.deduplicated` | Info | Duplikat erkannt und übersprungen. |
| `consent.check.passed` | Debug | Stage-0-Consent-Prüfung erfolgreich. |
| `consent.check.failed` | Warn | Stage-0-Consent-Prüfung fehlgeschlagen; Verarbeitung gestoppt. |
| `ai.call.started` | Debug | AOAI-Aufruf gestartet. |
| `ai.call.completed` | Info | AOAI-Aufruf abgeschlossen (enthält tokenCount, latencyMs, promptHash, outputHash). |
| `ai.call.failed` | Error | AOAI-Aufruf fehlgeschlagen (Fehlercode, kein Klartext). |
| `ai.suggestion.created` | Info | AI-Vorschlag (Reply/Summary) erstellt und in BC gespeichert. |
| `ai.suggestion.accepted` | Info | Benutzer hat AI-Vorschlag akzeptiert. |
| `permission.denied` | Warn | Pre-AI-Permission-Resolver hat Anfrage abgewiesen. |
| `prompt.injection.detected` | Warn | Möglicher Prompt-Injection-Versuch im Mailtext erkannt. |
| `consent.granted` | Info | Consent für Mailbox erteilt (Audit, BC+Azure). |
| `consent.withdrawn` | Warn | Consent für Mailbox widerrufen. |
| `consent.expired` | Warn | Consent abgelaufen (Pilot Until überschritten). |
| `audit.chain.verified` | Info | Hash-Chain-Integritätsprüfung erfolgreich. |
| `audit.chain.broken` | Critical | Hash-Chain-Bruch erkannt – mögliche Manipulation. |
| `backfill.started` | Info | Backfill-Job für Mailbox gestartet. |
| `backfill.completed` | Info | Backfill-Job abgeschlossen. |
| `backfill.resumed` | Info | Unterbrochener Backfill-Job fortgesetzt (High-Water-Mark). |

---

## 5. Sink-Routing

| Log-Kategorie | Sink | Retention | Zugriff |
|---------------|------|-----------|---------|
| Operations (alle Levels) | Application Insights + Log Analytics **Operations-Workspace** | 90 Tage | Entwickler, SRE |
| Compliance-/Audit-Events (`consent.*`, `permission.denied`, `audit.*`, `prompt.injection.*`) | Log Analytics **Audit-Workspace** (append-only) + BC Tabelle 50012 | 7 Jahre | DSB, Security (read-only) |
| BC-Extension-Events | BC Tabelle 50012 (Hash-Chain, append-only) | gemäß Retention-Policy | DSB, IOI_COMM_HUB_ADMIN |

**Doppelschreibung** für Compliance-Events: sowohl in Azure Audit Workspace als auch in BC Tabelle 50012. Bei Divergenz gilt das BC-Log als primäre Rechtsquelle (weil manipulationsgeschützt durch Hash-Chain).

---

## 6. .NET / C# Implementierungsregeln

```csharp
// Strukturiertes Logging mit Serilog/ILogger – kein string.Format in Log-Calls
_logger.LogInformation(
    "Interaction persisted. CorrelationId={CorrelationId} TenantId={TenantId} EventType={EventType}",
    correlationId, tenantId, "interaction.persisted");

// FALSCH – enthält PII:
_logger.LogInformation("Persisted message from {EmailAddress}", emailAddress);

// RICHTIG – nur Hash:
_logger.LogInformation("Persisted message. SourceHash={SourceHash}", sourceHash);
```

- Alle HTTP-Handler lesen / erzeugen `X-Correlation-Id` Header (GUID).
- Correlation ID wird in `ILogger`-Scope gesetzt: `using (_logger.BeginScope(...))`.
- `ILogger<T>` verwenden, nie `Console.Write` oder `Debug.Write`.

---

## 7. AL / BC Implementierungsregeln

- Alle Audit-Writes **ausschließlich** über `Codeunit "Comm. Audit Management".WriteAuditEntry()` (kein direktes Table-Insert).
- Kein PII in `Message`-Feld der Audit-Tabelle.
- `Event Type` muss einem der Standard-Codes aus §4 entsprechen; für neue Codes zuerst hier eintragen.

---

## 8. Änderungshistorie

| Version | Datum | Änderung |
|---------|-------|---------|
| 0.1.0 | 2026-06-01 | Initial (Sprint 0, H5) |
