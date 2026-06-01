# 22 – High-Water-Mark Konzept

> Sprint 0 Deliverable, WS-I Task I3.  
> Bezug: [07-ingestion-pipeline.md](07-ingestion-pipeline.md) §6, [21-gap-monitor-spec.md](21-gap-monitor-spec.md).

---

## 1. Zweck

Die Ingestion-Pipeline verarbeitet E-Mails und Teams-Nachrichten für mehrere Postfächer
(Mailboxes) parallel. Um bei Unterbrechungen (Crash, Throttling, Downtime) den Fortschritt
nicht zu verlieren und unnötige Re-Verarbeitung zu verhindern, wird pro Mailbox ein
**High-Water-Mark (HWM)** gespeichert.

Der HWM bezeichnet den letzten erfolgreich verarbeiteten Zeitstempel und die zugehörige
Nachrichten-ID für ein Postfach. Er dient als Wiederaufnahme-Punkt für Backfill-Jobs.

---

## 2. Datenmodell

### 2.1 `HighWaterMark` Record (C#, Sprint 0 Skeleton)

```csharp
// src/ingestion/src/CommunicationHub.Ingestion.Core/Models/HighWaterMark.cs
public sealed record HighWaterMark
{
    public required string TenantId           { get; init; }
    public required string MailboxAddress     { get; init; }
    public string?         LastProcessedMessageId { get; init; }
    public DateTimeOffset? LastProcessedAt    { get; init; }
    public DateTimeOffset  BackfillFrom       { get; init; }
    public DateTimeOffset  BackfillTo         { get; init; }
    public HighWaterMarkStatus Status         { get; init; }
    public int             ProcessedCount     { get; init; }
    public int             FailureCount       { get; init; }
    public string?         LastErrorMessage   { get; init; }
}

public enum HighWaterMarkStatus { Pending, InProgress, Completed, Failed, Paused }
```

### 2.2 `IngestionCheckpoint` (Azure Table Storage – Sprint 1)

Persistenz im Betrieb erfolgt in **Azure Table Storage** (Partition = `TenantId`,
Row = `MailboxAddress`). Felder:

| Spalte | Typ | Beschreibung |
|--------|-----|-------------|
| `PartitionKey` | string | TenantId (GUID) |
| `RowKey` | string | MailboxAddress (lowercase) |
| `LastProcessedMessageId` | string | Internet Message-ID der zuletzt verarbeiteten Nachricht |
| `LastProcessedAt` | DateTimeOffset | Zeitstempel der letzten verarbeiteten Nachricht |
| `BackfillFrom` | DateTimeOffset | Start des aktuellen/letzten Backfill-Fensters |
| `BackfillTo` | DateTimeOffset | Ende des aktuellen/letzten Backfill-Fensters |
| `Status` | string (Enum) | Pending / InProgress / Completed / Failed / Paused |
| `ProcessedCount` | int | Anzahl verarbeiteter Nachrichten in aktuellem Lauf |
| `FailureCount` | int | Anzahl fehlgeschlagener Nachrichten |
| `LastErrorMessage` | string | Letzte Fehlermeldung (kein PII) |
| `GapCount` | int | Erkannte Lücken (inkrementiert durch Gap Monitor) |
| `ETag` | string | Optimistic Concurrency Control |

---

## 3. Semantik der HWM-Zustände

```
Pending  ──→  InProgress  ──→  Completed
                  │                ↑
                  ↓                │
               Failed  ──→  (Paused nach Max-Retries)
                  │
                  ↓
               Paused ──→ InProgress  (Resume durch BackfillResumeTrigger)
```

| Status | Bedeutung |
|--------|-----------|
| **Pending** | Mailbox wurde zum Backfill angemeldet, aber noch nicht gestartet. |
| **InProgress** | Backfill läuft aktiv (Function ist am Laufen). |
| **Completed** | Backfill-Fenster vollständig verarbeitet. |
| **Failed** | Fataler Fehler; manuelle Überprüfung notwendig. |
| **Paused** | Pausiert (Throttle-Backoff, Consent-Widerruf, oder max. Fehleranzahl erreicht). Wird automatisch von `BackfillResumeTrigger` fortgesetzt. |

---

## 4. Fortschritts-Aktualisierung

Innerhalb eines Backfill-Runs wird die HWM nach **jeder erfolgreich in BC persistierten
Nachricht** atomisch aktualisiert (`ETag`-basiertes Optimistic Concurrency Update in
Azure Table Storage). Dies minimiert Re-Verarbeitung bei Unterbrechungen.

```
Nachricht N verarbeitet:
  1. Interaction in BC persistiert (via Backend API).
  2. HWM.LastProcessedMessageId = N.InternetMessageId
  3. HWM.LastProcessedAt       = N.ReceivedDateTime
  4. HWM.ProcessedCount        += 1
  5. Azure Table Storage Update mit ETag-Check (bei Konflikt: lesen, erneut versuchen).
```

Bei **Fehler** einer Nachricht:
```
  1. HWM.FailureCount += 1
  2. HWM.LastErrorMessage = sanitisierte Fehlermeldung (kein PII)
  3. Wenn FailureCount >= MaxConsecutiveFailures (Default: 5):
       → Status = Paused
       → eventType = "backfill.paused" in Audit Log
```

---

## 5. Wiederaufnahme (Resume)

Der `BackfillResumeTrigger` (Azure Function, Timer 15 min) sucht alle HWM-Einträge
mit `Status = Paused` und `LastProcessedAt < (Now - ResumeDelayMinutes)`.

Wiederaufnahme:
1. HWM.Status → InProgress, HWM.FailureCount → 0
2. Backfill beginnt bei `BackfillFrom = HWM.LastProcessedAt` (nicht von vorn).
3. Event `backfill.resumed` in Audit Log.

---

## 6. Isolation und Parallelität

- Pro Mailbox ein HWM-Eintrag → vollständige Isolation zwischen Postfächern.
- Mehrere Function-Instanzen können gleichzeitig unterschiedliche Mailboxes verarbeiten.
- Eine Mailbox wird **nie** gleichzeitig von zwei Instanzen verarbeitet:
  `Status = InProgress` mit ETag-Lock verhindert Doppelverarbeitung.

---

## 7. Monitoring

HWM-Metriken werden in Application Insights als Custom Metrics geschrieben:

| Metrik | Beschreibung |
|--------|-------------|
| `ingestion.hwm.processedCount` | Verarbeitete Nachrichten pro Run |
| `ingestion.hwm.failureCount` | Fehler pro Run |
| `ingestion.hwm.lagMinutes` | `Now - LastProcessedAt` in Minuten |
| `ingestion.hwm.statusTransition` | State-Übergänge (Dimension: Status) |

Dashboard-Alert: `ingestion.hwm.lagMinutes > 30` → Severity 2 Alert.

---

## 8. Nicht-Ziele

- Der HWM steuert **keine** Webhook-Ingestion (nur Backfill).
- Er ist kein vollständiger Audit-Trail (das ist BC Tabelle 50012).
- Er enthält kein PII außer `MailboxAddress` (Zugriff via RBAC eingeschränkt).

---

## 9. Acceptance Criteria (Sprint 1)

- [ ] `IngestionCheckpointRepository` implementiert mit Azure Table Storage.
- [ ] Unit-Test: HWM wird nach erfolgreicher Persistenz korrekt aktualisiert.
- [ ] Unit-Test: HWM wechselt zu `Paused` nach `MaxConsecutiveFailures`.
- [ ] `BackfillResumeTrigger` nimmt pausierte Jobs korrekt wieder auf.
- [ ] Application Insights Custom Metrics erscheinen im Dashboard.

---

## 10. Änderungshistorie

| Version | Datum | Änderung |
|---------|-------|---------|
| 0.1.0 | 2026-06-01 | Initial (Sprint 0, I3) |
