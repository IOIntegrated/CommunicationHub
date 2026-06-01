# 21 – Gap Monitor Specification (Lücken-Monitor)

> Sprint 0 Deliverable, WS-I Task I2.  
> Bezug: [07-ingestion-pipeline.md](07-ingestion-pipeline.md) §6, [22-high-water-mark.md](22-high-water-mark.md).

---

## 1. Problemstellung

Die Ingestion-Pipeline basiert auf Microsoft Graph Change Notifications (Webhooks).
Webhooks können in folgenden Szenarien Nachrichten **nicht** liefern:

| Szenario | Ursache |
|----------|---------|
| Subscription abgelaufen | Max. Laufzeit 4230 min; bei Erneuerungsfehler Lücke. |
| Azure Service Bus Throttling | Delivery-Fehler werden nach 10 Versuchen in DLQ verschoben. |
| Graph Subscription Lifecycle | Microsoft kann Subscriptions unerwartet entfernen (Lifecycle Notification). |
| Downtime des Ingestion-Service | Neue Nachrichten werden nicht empfangen. |
| M365 Graph API Outage | Notifications werden nicht gesendet, auch kein Retry von Microsoft. |

Der **Gap Monitor** erkennt solche Lücken und löst einen gezielten Backfill aus.

---

## 2. Erkennungsmethode

### 2.1 Heuristik: Sequenz-Lücke im Internet Message-ID-Thread

Jede E-Mail innerhalb eines Threads besitzt in den Internet-Headern eine `Message-Id` und
ggf. eine `In-Reply-To`-Referenz. Wenn die Ingestion eine Nachricht mit `In-Reply-To = X`
empfängt, aber `X` nicht im BC-Bestand vorliegt, ist eine Lücke vorhanden.

Erkennung: Beim Persistieren einer Interaction prüft Stage 6 (Dedup/Sequence) in der
Ingestion Core, ob der Parent (`In-Reply-To`) bereits unter `Communication Interaction.Internet Message Id` gespeichert ist.

### 2.2 Heuristik: High-Water-Mark-Zeitfenster-Lücke

Der Timer-Trigger `GapMonitorTrigger` (Codeunit, 5-Minuten-Takt) prüft für jede aktive
Mailbox, ob die letzte `LastProcessedAt` der High-Water-Mark (Tab. `IngestionCheckpoint`)
mehr als `MaxLagMinutes` (Default: 15 min) zurückliegt.

```
if (Now() - HighWaterMark.LastProcessedAt) > MaxLagMinutes
    → Queue BackfillJob für diese Mailbox
```

### 2.3 Subscription-Health-Check

Der `SubscriptionRenewalTrigger` (Timer, stündlich) ruft `GET /v1.0/subscriptions`
auf und vergleicht die bekannten Subscription-IDs aus dem lokalen State-Store.
Fehlende Subscriptions lösen sofort `subscription.removed` (Warn) + Backfill aus.

---

## 3. Backfill-Auslösung

Wenn eine Lücke erkannt wird:

1. Eine `BackfillRequest`-Nachricht wird in die Service Bus Queue `ingestion-backfill` geschrieben.
2. Message enthält: `TenantId`, `MailboxAddress`, `BackfillFrom` (UTC), `BackfillTo` (UTC), `TriggeredBy` (Enum: GapDetected | SubscriptionLost | ManualRequest).
3. Der `BackfillResumeTrigger` (Azure Function, Timer 15 min, siehe [I4](19-repo-scaffolding.md)) verarbeitet die Queue.
4. Backfill ruft `GET /v1.0/users/{mailbox}/messages?$filter=receivedDateTime ge {from}` auf, paginiert durch Delta-Seiten und persistiert fehlende Interactions.

---

## 4. Gap Monitor Konfiguration

| Parameter | Default | Beschreibung |
|-----------|---------|-------------|
| `MaxLagMinutes` | 15 | Maximale tolerierte Verarbeitungsverzögerung pro Mailbox. |
| `BackfillLookbackHours` | 24 | Standard-Rückblickfenster für einen Gap-Backfill. |
| `MaxBackfillDaysPerRun` | 7 | Maximale Anzahl Tage die ein einzelner Backfill abdeckt (Schutz vor Timeout). |
| `GapMonitorCronExpression` | `0 */5 * * * *` | Azure Functions CRON für Gap-Monitor-Trigger. |
| `SubscriptionHealthCronExpression` | `0 0 * * * *` | Stündlicher Subscription-Health-Check. |

---

## 5. Persistenz der Gap-Erkennungen

Gap-Events werden als Audit-Einträge (Tab. 50012) mit `Event Type = "gap.detected"` und als
Azure Log Analytics Warning (`eventType = "gap.detected"`) geschrieben.

Zusätzlich wird in der `IngestionCheckpoint`-Tabelle (I3) das Feld `GapCount` inkrementiert.
Ab `GapCount >= 3` (konfigurierbar) wird Alert an IT via Service Health-Kanal ausgelöst.

---

## 6. Nicht-Ziele

- Der Gap Monitor ist kein vollständiger Full-Sync (das ist der initiale Backfill, I3).
- Er ersetzt nicht die reguläre Webhook-Ingestion für laufenden Betrieb.
- Er behandelt keine bewusst ausgeschlossenen Nachrichten (Consent=Withdrawn).

---

## 7. Acceptance Criteria (Sprint 1)

- [ ] `GapMonitorTrigger` Function erstellt und loggt korrekte Events.
- [ ] Unit-Test: Gap wird erkannt wenn `LastProcessedAt > MaxLagMinutes`.
- [ ] Unit-Test: Subscription-Lücke löst Backfill-Request aus.
- [ ] Backfill-Request erscheint in Service Bus `ingestion-backfill` Queue.
- [ ] `gap.detected`-Events erscheinen in Log Analytics und BC Tabelle 50012.

---

## 8. Änderungshistorie

| Version | Datum | Änderung |
|---------|-------|---------|
| 0.1.0 | 2026-06-01 | Initial (Sprint 0, I2) |
