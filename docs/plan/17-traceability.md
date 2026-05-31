# 17 – Traceability gegen Akzeptanzkriterien

> **Pflicht-Verifikation des Planungsergebnisses** gegen die Akzeptanzkriterien in [`../../instructions.md`](../../instructions.md), Abschnitte „Akzeptanzkriterien" (Hauptteil, H1–H10) und „Ergänzte Akzeptanzkriterien" (Erweiterung, E1–E10).
>
> Begleitdokumente: [00-overview.md](00-overview.md) (Plan-Übersicht), [15-open-questions-next-steps.md](15-open-questions-next-steps.md) (offene Fragen + Go/No-Go), [16-testing-acceptance.md](16-testing-acceptance.md) (Test- und Eval-Konzept).

---

## 1. Einleitung und Methodik

Diese Matrix verifiziert, dass jedes der 20 Akzeptanzkriterien aus `instructions.md` durch konkrete Plan-Komponenten, Funktionen, Endpunkte und Tests adressiert wird. Die Matrix dient drei Zwecken:

1. **Vollständigkeitsnachweis** des Planungsergebnisses gegenüber dem Auftrag.
2. **Navigationshilfe** für Implementierung und Review (jedes Kriterium → klare Plan-Quellen → konkrete Test-Hooks).
3. **Lücken-Analyse** (siehe §5): explizite Benennung etwaiger Restlücken inklusive Empfehlung.

### 1.1 Methodik

- **Wortgetreue Nummerierung** der Kriterien aus `instructions.md`. Die Reihenfolge und der Wortlaut werden nicht verändert.
- Jede Zeile listet:
  - **Komponenten** (verlinkte Plan-Dokumente),
  - **Konkrete Funktionen / Endpunkte / Pages** (Backend-Routen, BC-Pages, AL-Tabellen, Capabilities),
  - **Tests** mit Verweis auf [16-testing-acceptance.md](16-testing-acceptance.md),
  - **Status**: `Plan` (im Konzept enthalten) · `Design vorhanden` (detailliert ausgeführt) · `Test geplant` (im Test-Konzept abgedeckt).
- **Quellen-Konventionen**: `01` = `01-architecture.md`, `02` = `02-bc-data-model.md`, …, `16` = `16-testing-acceptance.md`. Abschnittsverweise als `§n.m`.

### 1.2 Status-Definitionen

| Status | Bedeutung |
|--------|-----------|
| **Plan** | Anforderung im Konzept genannt, Designtiefe noch ausbaufähig (i. d. R. Erweiterungspunkt). |
| **Design vorhanden** | Detailspezifikation in mind. einem Plandokument liegt vor (Schemata, Endpunkte, Sequenzdiagramme). |
| **Test geplant** | Konkreter Test-Hook in [16-testing-acceptance.md](16-testing-acceptance.md) referenziert. |

Mehrfachstatus möglich (häufig: Design vorhanden + Test geplant).

---

## 2. Hauptteil-Akzeptanzkriterien (H1–H10)

> Wortlaut aus `instructions.md`, Abschnitt „Akzeptanzkriterien": *„Die geplante Lösung gilt als erfolgreich, wenn:"*

| ID | Akzeptanzkriterium (wortgetreu) | Komponenten (Plan-Docs) | Konkrete Funktionen / Endpunkte / Pages | Tests (Verweis auf 16) | Status |
|----|---------------------------------|------------------------|-----------------------------------------|------------------------|--------|
| **H1** | Eine eingehende Kunden-E-Mail automatisch einem BC-Kunden vorgeschlagen werden kann. | [07 §4 Stage 6/7](07-ingestion-pipeline.md) · [10](10-matching.md) · [03 §3 #12](03-bc-apis.md) · [02 §3.4, §3.13](02-bc-data-model.md) | Ingestion Stages 6/7; `POST /matching/suggestCustomer` (BC); Backend `POST /v1/matching` und `POST /v1/mail/analyze`; AL-Tabelle `Communication Match Suggestion` (50013); LR-Reranker + optional LLM-Reranker (C6) | [16 §5.1 E2E Mail→BC](16-testing-acceptance.md) · [16 §6.1 C6 nDCG@5 ≥ 0,85](16-testing-acceptance.md) · [16 §4.3 Externe-Beteiligung-Heuristik](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H2** | Der Benutzer relevante BC-Informationen direkt in Outlook sieht. | [04 §3.4, §4](04-outlook-addin.md) · [03 §3 #13–#17](03-bc-apis.md) · [02 §8.5–§8.8](02-bc-data-model.md) | Outlook Side Panel Tabs „Übersicht/Verlauf/Dokumente/Aufgaben"; Backend `GET /v1/customer/{id}/context`, `/timeline`, `/documents`, `/openItems`; BC-Pages Customer/Contact/Project Communication Timeline + FactBoxes (8.6–8.8) | [16 §5.3 Antwortvorschlag → Outlook](16-testing-acceptance.md) · [16 §4.5 Outlook-Komponenten + Visual Tests](16-testing-acceptance.md) · [16 §4.1 TestPage Customer Timeline](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H3** | Der Assistent Fragen aus der E-Mail erkennt. | [08 §2 C2](08-ai-orchestration.md) · [08 §5.4 ExtractionResult v1](08-ai-orchestration.md) · [02 §3.8](02-bc-data-model.md) | Capability C2 (Strukturierte Extraktion); JSON-Schema `ExtractionResult v1` mit `questions[]`, `tasks[]`, `risks[]`; Backend `POST /v1/analyze`; AL-Tabelle `Communication Action Item` (50007) | [16 §6.1 C2 Slot-F1 ≥ 0,75](16-testing-acceptance.md) · [16 §4.4 Schema-Validierung Pflicht-`questions`](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H4** | Der Assistent einen Antwortentwurf erzeugt. | [08 §2 C3](08-ai-orchestration.md) · [08 §5.5 ReplySuggestion v1](08-ai-orchestration.md) · [04 §3.5](04-outlook-addin.md) · [03 §3 #20](03-bc-apis.md) · [02 §3.10](02-bc-data-model.md) | Capability C3; JSON-Schema `ReplySuggestion v1`; Backend `POST /v1/mail/{id}/suggest-reply` (SSE-Streaming); BC `POST /replyDrafts`; AL-Tabelle `Communication Reply Draft` (50009) | [16 §6.1 C3 Faithfulness ≥ 0,95](16-testing-acceptance.md) · [16 §4.4 Grounding-Test](16-testing-acceptance.md) · [16 §5.3](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H5** | Der Antwortentwurf verwendete Quellen anzeigt. | [08 §1 L2 Quellenpflicht](08-ai-orchestration.md) · [08 §5.5 `sources[]` minItems=1](08-ai-orchestration.md) · [04 §3.5, §9](04-outlook-addin.md) · [02 §3.9](02-bc-data-model.md) · [03 §3 #20](03-bc-apis.md) | Pflichtfeld `sources[]` im AI-Schema (Schema-Validierung mit `strict:true`); Quellen-Chips im Outlook Side Panel; AL-Tabelle `Communication Source Reference` (50008); Source-URI-Schema `src://{search,bc,graph}/...` | [16 §6.1 Citation Coverage ≥ 0,95](16-testing-acceptance.md) · [16 §4.4 Schema-Verletzung = Testfehler](16-testing-acceptance.md) · [16 §6.2 Faithfulness-LLM-as-Judge](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H6** | Der Benutzer den Entwurf bearbeiten und selbst senden kann. | [04 §1.2, §3.5, §4.1, §9](04-outlook-addin.md) · [05 §6](05-teams-app.md) · [08 §1 L4](08-ai-orchestration.md) | Antwortvorschlag im editierbaren `<textarea>`; Bestätigungs-Modal vor `Office.context.mailbox.item.body.setAsync`; **kein** `Office.context.mailbox.item.send()`; Tool-Allowlist ohne `sendMail`/`postTeamsMessage` ([08 §6](08-ai-orchestration.md)) | [16 §5.3 Compose-Insert ohne Auto-Send](16-testing-acceptance.md) · [16 §4.4 Tool-Allowlist-Test](16-testing-acceptance.md) · [16 §7.1 „Externe Aktion"-Injection-Test](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H7** | Die Kommunikation als Timeline-Eintrag in BC abgelegt werden kann. | [02 §3.1, §8.1–§8.5](02-bc-data-model.md) · [03 §3 #3, #10, #18](03-bc-apis.md) · [04 §6](04-outlook-addin.md) · [05 §8.1](05-teams-app.md) · [07 §4 Stage 12](07-ingestion-pipeline.md) | AL-Tabellen `Communication Interaction` (50000), `Participant` (50001), `Entity Link` (50003); Backend `POST /v1/interactions`; BC `POST /interactions`, `POST /interactions({id})/entityLinks`, `POST /summaries`; BC-Pages Customer/Contact/Project Communication Timeline | [16 §5.1 E2E Mail→BC-Timeline](16-testing-acceptance.md) · [16 §5.2 Teams→BC](16-testing-acceptance.md) · [16 §4.1 TestPage Timeline + Permission-Set-Tests](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H8** | Teams-Nachrichten später analog verarbeitet werden können. | [05](05-teams-app.md) · [07 §3, §4 Stages](07-ingestion-pipeline.md) · [11 §3](11-graph-feasibility.md) · [02 §3.1 Source-Felder Chat/Team/Channel/Meeting](02-bc-data-model.md) | Teams App (Bot, ME, Tab); Channel/Chat-Subscriptions mit Encrypted Content; AL-Felder `Chat Id`, `Team Id`, `Channel Id`, `Online Meeting Id`; RSC-First-Strategie | [16 §5.2 E2E Teams→BC](16-testing-acceptance.md) · [16 §4.6 Teams App Tests](16-testing-acceptance.md) · [16 §13 R1 Transkript-Limitation](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H9** | Die Architektur Mandantenfähigkeit, Sicherheit und Berechtigungen berücksichtigt. | [01 §8](01-architecture.md) · [02 §1 Designprinzip 2, §6, §7](02-bc-data-model.md) · [03 §10](03-bc-apis.md) · [09 §6, §3 ACL-Felder](09-data-search.md) · [12 §1, §2, §5, §6, §7](12-security-compliance.md) | Mandant ≜ (M365-Tenant × BC-Company); Pflicht-Header `x-ccc-tenant`/`x-ccc-bc-company`; AL Codeunit `IOI_CommHub_Security.ApplyVisibilityFilter`; Permission Sets `IOI_COMMHUB_READ/USER/ADMIN/API/AUDIT`; Search-Index-Set pro Tenant + ACL-Filterstring; Pre-Inference-Berechtigungsprüfung | [16 §7.2 Berechtigungstests](16-testing-acceptance.md) · [16 §7.4 Cross-Tenant-Negativtests](16-testing-acceptance.md) · [16 §4.1 Permission-Set-Tests](16-testing-acceptance.md) · [16 §1.3 Tenant-Isolation in jedem Test](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **H10** | Die Lösung erweiterbar für Dokumente, Meetings und proaktive Assistenz bleibt. | [01 §1, §4](01-architecture.md) · [02 §1 Designprinzip 6 (`Extensible = true`)](02-bc-data-model.md) · [07 §3, §4 Stage 10/11](07-ingestion-pipeline.md) · [08 §2 C4d Meeting-Briefing, C4c Kunden-Briefing](08-ai-orchestration.md) · [09 §2 Indizes `documents`, `transcripts`](09-data-search.md) · [11 §3.4, §4](11-graph-feasibility.md) | MVP-Schichtung 3+4 (Dokumente/Meetings/proaktiv); Erweiterbare Enums (`Communication Channel`, `Source System`, `Storage Location`); separate Indizes `documents-{tenantId}` + `transcripts-{tenantId}`; Capabilities C4a–C4e + C5; Channel-Enum offen für Telefonie (Erweiterungspunkt) | [16 §5.4 Cross-Komponenten Korrektur](16-testing-acceptance.md) · [16 §13 R1 Transkript-Pfad](16-testing-acceptance.md) | Design vorhanden · Test geplant (für MVP3) |

---

## 3. Erweiterte Akzeptanzkriterien (E1–E10)

> Wortlaut aus `instructions.md`, Abschnitt „Ergänzte Akzeptanzkriterien": *„Die Lösung gilt zusätzlich als erfolgreich, wenn:"*

| ID | Akzeptanzkriterium (wortgetreu) | Komponenten (Plan-Docs) | Konkrete Funktionen / Endpunkte / Pages | Tests (Verweis auf 16) | Status |
|----|---------------------------------|------------------------|-----------------------------------------|------------------------|--------|
| **E1** | Externe E-Mails aus mehreren Mitarbeiterpostfächern automatisch erkannt werden. | [07 §3, §4 Stages 1–2](07-ingestion-pipeline.md) · [11 §2](11-graph-feasibility.md) · [12 §3, §4.1](12-security-compliance.md) | Graph-Mail-Subscriptions pro Pilot-Postfach (`Mail.Read` Application + Application Access Policy); `Subscription Manager` mit Renewal alle ~2 Tage; Webhook-Endpoint mit `clientState`-Validation | [16 §4.3 Subscription-Renewal](16-testing-acceptance.md) · [16 §5.1 E2E mehrere Postfächer](16-testing-acceptance.md) · [16 §13 R3 App-Access-Policy-Health-Check](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **E2** | Rein interne Kommunikation zuverlässig ausgeschlossen wird. | [07 §4 Stage 2/3](07-ingestion-pipeline.md) · [02 §3.12 Internal Domain](02-bc-data-model.md) · [12 §10.1](12-security-compliance.md) | Erfassungslogik 1–8 als Stage 2; Ausschlussregeln Stage 3 (interne Domain, Newsletter, Sensitivity „Privat"); AL-Tabelle `Communication Internal Domain` (50011); Override-Bestätigungs-Flow im Teams Bot ([05 §9.2](05-teams-app.md)) | [16 §4.3 Externe-Beteiligung-Heuristik (8 Regeln)](16-testing-acceptance.md) · [16 §4.3 Ausschluss-Regeln](16-testing-acceptance.md) · [16 §5.2 Negativ: rein internes Channel](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **E3** | E-Mails automatisch passenden BC-Kunden oder Kontakten vorgeschlagen werden. | [10](10-matching.md) · [07 §4 Stage 6/7](07-ingestion-pipeline.md) · [03 §3 #12](03-bc-apis.md) · [09 §2 Index `bc-master-{tenantId}`](09-data-search.md) | Hybrid-Matching (Regelbasis S1–S11 + LR-Reranker + optional LLM-Reranker); Schwellenmodell auto-confirm ≥ 0,85 / needs-review 0,60–0,84 / low-confidence < 0,60; AL-Tabelle `Communication Entity Link` (50003) | [16 §6.1 C6 nDCG@5 ≥ 0,85](16-testing-acceptance.md) · [16 §6.1 C2 F1 Kunde/Beleg ≥ 0,85](16-testing-acceptance.md) · [10 §12 Eval-Set ≥ 500](10-matching.md) | Design vorhanden · Test geplant |
| **E4** | Teams-Kommunikation mit externen Teilnehmern technisch bewertet und im Pilot erfasst werden kann. | [11 §3, §6, §7](11-graph-feasibility.md) · [05 §2.3 RSC](05-teams-app.md) · [07 §3, §13](07-ingestion-pipeline.md) · [12 §3.1, §3.2](12-security-compliance.md) | Capability Matrix (Mail/Chat/Channel/Meeting/Transcript/Recording); RSC-First (`ChannelMessage.Read.Group`, `ChatMessage.Read.Chat`); Pay-per-use Modellrechnung §6.1; Encrypted Content Subscriptions; Externe-Teilnehmer-Erkennung via `tenantId` | [16 §5.2 RSC-Pfad Teams→BC](16-testing-acceptance.md) · [16 §13 R1/R2 Lizenz-/Kosten-Risiken dokumentiert](16-testing-acceptance.md) · [16 §4.6 RSC-Manifest-Validierung](16-testing-acceptance.md) | Design vorhanden · Test geplant (Pilot) |
| **E5** | Jeder erfasste Eintrag eine Quelle und einen Capture-Zeitpunkt hat. | [02 §3.1 Pflichtfelder](02-bc-data-model.md) · [07 §4 Stage 12](07-ingestion-pipeline.md) · [03 §4.1 Beispiel-Payload](03-bc-apis.md) | Pflichtfelder in `Communication Interaction` (50000): `Capture Method`, `Capture Timestamp`, `Source System`, `Source Message Id`, `Internet Message Id`, `Conversation Id`, `Mailbox UPN`, `Permalink Url`; serverseitige Validierung in Custom-API-Codeunit | [16 §4.1 TestPage-Tests Pflichtfelder](16-testing-acceptance.md) · [16 §4.2 API Surface Snapshot](16-testing-acceptance.md) · [16 §5.1 Assertion Source-Felder gesetzt](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **E6** | Dubletten und mehrfach empfangene E-Mails nicht mehrfach falsch angezeigt werden. | [07 §4 Stage 4, §6 Idempotenz](07-ingestion-pipeline.md) · [03 §5](03-bc-apis.md) · [02 §3.1 SK10 unique](02-bc-data-model.md) | Idempotenz-Key (Mail: `internetMessageId`; Teams: `chatId+messageId+etag`); Inbox-Pattern (Webhook) + Outbox-Pattern (BC-Persistenz); SqlIndex unique auf (`Tenant Id`, `External Hash`); Service-Bus Duplicate Detection (10 min); Redis-Dedup TTL 90 d | [16 §4.3 Idempotenz Doppel-Webhook](16-testing-acceptance.md) · [16 §5.1 Wiederholter Eingang ⇒ kein zweiter Datensatz](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **E7** | Benutzer nur Inhalte sehen, für die sie berechtigt sind. | [02 §6, §7](02-bc-data-model.md) · [03 §10](03-bc-apis.md) · [09 §6](09-data-search.md) · [12 §5, §6](12-security-compliance.md) · [04 §9](04-outlook-addin.md) · [05 §10](05-teams-app.md) | BC `IsAllowedToView`-Prüfung in jeder Page/API vor `IsEmpty`/`FindSet`; Visibility-Scope-Filter (`Owner`/`Owner Team`/`Company`); ACL-Pflichtfilter in Search-Queries (`aclUserIds`/`aclGroupIds`/`visibilityScope`); Sensitivity-Schwelle aus BC-Rolle abgeleitet | [16 §7.2 Visibility-Scope-Test](16-testing-acceptance.md) · [16 §4.1 Permission-Set-Tests Negativ/Positiv](16-testing-acceptance.md) · [16 §4.4 Briefing-Leak-Test](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **E8** | AI-Zusammenfassungen keine Berechtigungen umgehen. | [08 §1 L5 Pre-Inference-Filter](08-ai-orchestration.md) · [12 §6](12-security-compliance.md) · [09 §6 Pre-Filter](09-data-search.md) · [10 §11](10-matching.md) | Permission Resolver entfernt nicht-berechtigte Quellen **vor** dem Prompt (kein Post-hoc-Redaction); ACL-Filter in jeder Search-Query verbindlich; Tenant-/Company-Cross-Check in Custom API; Cache-Bust bei Berechtigungsänderung ([08 §8](08-ai-orchestration.md)) | [16 §7.2 Briefing-Leak-Test mit Trigger-Tokens](16-testing-acceptance.md) · [16 §7.4 Filter-Bypass-Versuch](16-testing-acceptance.md) · [16 §4.4 Tool-Allowlist](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **E9** | Es gibt ein Audit-Log für Erfassung, Verarbeitung und Anzeige. | [02 §3.13 Audit Log Entry](02-bc-data-model.md) · [01 §10 Custom Audit Events](01-architecture.md) · [08 §1 L6, §15.2](08-ai-orchestration.md) · [12 §13](12-security-compliance.md) · [07 §4 Stage 15](07-ingestion-pipeline.md) | AL-Tabelle `Communication Audit Log Entry` (50012, append-only); App-Insights Custom Events (`interaction.persisted`, `ai.suggestion.created`, `permission.denied`, `prompt.injection.detected`, `match.evaluated`, `rtbf.completed` …); Hash-Kette pro Audit-Zeile; SIEM-Forwarding | [16 §7.5 Audit-Log-Manipulationsschutz](16-testing-acceptance.md) · [16 §1.3 Audit-Pflicht in Eval-Runs](16-testing-acceptance.md) · [16 §5.1 Audit-Eintrag mit `correlation_id`](16-testing-acceptance.md) | Design vorhanden · Test geplant |
| **E10** | Datenschutz- und Compliance-Risiken sind explizit bewertet. | [12 §1.2 STRIDE](12-security-compliance.md) · [12 §8 DSGVO + DPIA](12-security-compliance.md) · [12 §10 Mitarbeiterüberwachung](12-security-compliance.md) · [11 §6 Risiken](11-graph-feasibility.md) · [08 §16 AI-Risiken](08-ai-orchestration.md) · [15 §2.3, §7](15-open-questions-next-steps.md) | STRIDE-Kurzanalyse pro Komponente; DSFA-Bausteine; § 87 BetrVG Mitbestimmung; Pay-per-use Modellrechnung; AI-Top-3-Risiken (Prompt Injection, Halluzination, Kosten); Go/No-Go-Kriterien GG-01…GG-16 in [15 §7](15-open-questions-next-steps.md) | [16 §7.1 Prompt-Injection-Suite](16-testing-acceptance.md) · [16 §7.3 DSGVO-Workflow-Tests](16-testing-acceptance.md) · [16 §13 Risiko-Tabelle R1–R10](16-testing-acceptance.md) | Design vorhanden · Test geplant |

---

## 4. Kreuzreferenz: welche Akzeptanzkriterien je Plan-Dokument bedient werden

| Plan-Dokument | Bediente Akzeptanzkriterien |
|---------------|------------------------------|
| [00-overview.md](00-overview.md) | Übergreifend (Navigations-/Übersichtsdokument); keine direkte Anforderungs-Erfüllung. |
| [01-architecture.md](01-architecture.md) | H2, H7, H8, **H9**, H10, E1, **E5**, **E9**, E10 |
| [02-bc-data-model.md](02-bc-data-model.md) | H2, **H7**, H8, **H9**, H10, **E5**, **E6**, **E7**, **E9** |
| [03-bc-apis.md](03-bc-apis.md) | **H1**, H2, **H4**, **H5**, **H7**, **H9**, **E5**, **E6**, **E7** |
| [04-outlook-addin.md](04-outlook-addin.md) | **H2**, **H4**, **H5**, **H6**, H7, **E7** |
| [05-teams-app.md](05-teams-app.md) | H4, H6, **H8**, E2, **E4**, **E7** |
| [07-ingestion-pipeline.md](07-ingestion-pipeline.md) | **H1**, H7, **H8**, H10, **E1**, **E2**, **E3**, E4, E5, **E6**, E9 |
| [08-ai-orchestration.md](08-ai-orchestration.md) | **H3**, **H4**, **H5**, **H6**, H10, E3, **E8**, E9, E10 |
| [09-data-search.md](09-data-search.md) | H2, **H8**, **H9**, H10, **E3**, **E7**, **E8** |
| [10-matching.md](10-matching.md) | **H1**, **E3**, E7 |
| [11-graph-feasibility.md](11-graph-feasibility.md) | **H8**, H10, **E1**, **E4**, E10 |
| [12-security-compliance.md](12-security-compliance.md) | H6, **H9**, H10, E2, **E7**, **E8**, **E9**, **E10** |
| [13-mvp-roadmap.md](13-mvp-roadmap.md) | **H8**, **H10**, **E1**, **E4** (MVP-Schnitt, Aufwand, Pilot-Voraussetzungen) |
| [14-risks-decisions.md](14-risks-decisions.md) | **E10**, querschnittlich H6/H9/E7/E8 (Risiko-Register, ADRs) |
| [15-open-questions-next-steps.md](15-open-questions-next-steps.md) | übergreifend (offene Fragen + Go/No-Go-Kriterien zu allen H/E) |
| [16-testing-acceptance.md](16-testing-acceptance.md) | **alle H1–H10, E1–E10** (Test-Hooks pro Kriterium) |

(Fett = Hauptverantwortung des Dokuments; nicht-fett = ergänzende Beiträge.)

---

## 5. Lücken-Analyse

### 5.1 Abdeckungs-Status

Alle 20 Akzeptanzkriterien (H1–H10, E1–E10) sind durch mindestens ein Plan-Dokument **im Design** adressiert und durch mindestens einen Test-Hook in [16-testing-acceptance.md](16-testing-acceptance.md) abgedeckt. **Keine inhaltliche Lücke** im Design.

### 5.2 Strukturelle Hinweise

Folgende strukturelle Punkte sind im Konzept benannt, aber nicht in eigenen Dokumenten ausformuliert. Sie blockieren MVP1 nicht, sollten aber für die Vollständigkeit der Plan-Auslieferung ergänzt werden:

| # | Hinweis | Empfehlung |
|---|---------|------------|
| L-01 | *(erledigt)* `13-mvp-roadmap.md` und `14-risks-decisions.md` wurden parallel zu dieser Traceability erstellt und sind im Verzeichnis vorhanden. MVP-Schnitt, Aufwand (≈ 290–530 PW Gesamt), 32 Risiken und 26 ADRs sind dokumentiert. | Keine offene Aktion; Status für H10/E10 ist damit vollständig auf `Design vorhanden + Test geplant` gehoben. |
| L-02 | **MVP3 Meeting-Transkripte** sind lizenz-/policy-abhängig (Teams Premium / Copilot beim Organisator). Im Plan ist das Risiko explizit benannt ([11 §3.4, §6.2](11-graph-feasibility.md), [16 §13 R1](16-testing-acceptance.md)), aber ohne Lizenz-Klärung ist H10 für den Meeting-Anteil **technisch nicht voll erfüllbar**. | Lizenz-Inventur (OF-L-02 in [15 §2.2](15-open-questions-next-steps.md)) mit Option, MVP3-Scope auf vorhandene Premium-Pilotorganisatoren zu beschränken. Negativpfad „Transkript fehlt → graceful degrade" ist als Pflichttest in [16 §13 R1](16-testing-acceptance.md) bereits vorgesehen. |
| L-03 | **Proaktive Auswertung** (H10 Teil „proaktive Assistenz") ist als MVP4 geplant; DSGVO-Bewertung steht aus (OF-D-07, OF-S-05 in [15 §2.3](15-open-questions-next-steps.md)). | Dedizierte DPIA-Erweiterung für proaktive Push-Benachrichtigungen vor MVP4; bis dahin Status `Plan` (nicht `Design vorhanden`) für den proaktiven Anteil von H10. |
| L-04 | **AppSource-Object-ID-Range** (OF-B-01) ist als Annahme A13 dokumentiert, aber für ISV-Lieferung (OF-S-01) noch nicht beantragt. Beeinflusst nicht die fachliche Erfüllung der Akzeptanzkriterien, wohl aber die Auslieferungsform. | Antrag bei Microsoft vor MVP4 stellen; für interne Lieferung unkritisch. |

### 5.3 Statement zur Vollständigkeit

> **Die geplante Lösung adressiert alle 20 Akzeptanzkriterien aus `instructions.md` (H1–H10, E1–E10) durch mindestens ein Plan-Dokument im Design und mindestens einen geplanten Test in [16-testing-acceptance.md](16-testing-acceptance.md). Es bestehen keine inhaltlichen Lücken. Strukturelle Empfehlungen (L-01 bis L-04) betreffen Auslieferung, Lizenzierung und ISV-Pfad, nicht die fachliche Erfüllung des Auftrags.**

---

## 6. Hinweis zur Verbindlichkeit

Dieses Dokument ist die **Pflicht-Verifikation des Planungsergebnisses** gegen die Akzeptanzkriterien aus `instructions.md`. Es ist Voraussetzung für den Übergang von Planung zu Implementierung und Bestandteil der Go/No-Go-Entscheidung vor MVP1 (siehe [15 §7](15-open-questions-next-steps.md)).

Änderungen an Akzeptanzkriterien oder Plandokumenten ziehen eine Aktualisierung dieser Matrix nach sich. Für jede Änderung:

1. Wortlaut des Kriteriums prüfen.
2. Zuordnung zu Komponenten / Funktionen / Tests aktualisieren.
3. Status (`Plan` / `Design vorhanden` / `Test geplant`) neu setzen.
4. Lücken-Analyse §5 erneut bewerten.

---

*Verweise: [00-overview.md](00-overview.md) · [15-open-questions-next-steps.md](15-open-questions-next-steps.md) · [16-testing-acceptance.md](16-testing-acceptance.md) · [`../../instructions.md`](../../instructions.md).*
