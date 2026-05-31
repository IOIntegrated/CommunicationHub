# 14 – Risiken & Entscheidungen (konsolidiert)

> Konsolidierte Risiko- und Entscheidungs-Übersicht zum **Customer Communication Copilot**. Quellen: [`../../instructions.md`](../../instructions.md), [00-overview.md](00-overview.md), [01-architecture.md](01-architecture.md), [02-bc-data-model.md](02-bc-data-model.md), [03-bc-apis.md](03-bc-apis.md), [04-outlook-addin.md](04-outlook-addin.md), [05-teams-app.md](05-teams-app.md), [07-ingestion-pipeline.md](07-ingestion-pipeline.md), [08-ai-orchestration.md](08-ai-orchestration.md), [09-data-search.md](09-data-search.md), [10-matching.md](10-matching.md), [11-graph-feasibility.md](11-graph-feasibility.md), [12-security-compliance.md](12-security-compliance.md), [16-testing-acceptance.md](16-testing-acceptance.md).
>
> Konsolidierung – keine neuen Themen. Owner-Rollen sind Vorschläge.

Score-Berechnung: $Score = P \times I$ mit $P, I \in \{1=L, 2=M, 3=H\}$ → Bereich 1–9.

---

## 1. Risikoregister

| ID | Risiko | Quelle | Kategorie | P | I | Score | Mitigation | Owner | Status |
|---|---|---|---|---|---|---|---|---|---|
| R-01 | Teams Export API Pay-per-use erzeugt nicht budgetierte Kosten bei tenant-weiter Erfassung (>1 Mio Msg/Mo ⇒ ~750 USD/Mo, 10×→7.500 USD/Mo, Backfill ~9.000 USD einmalig) | [11 §3.1, §6.1](11-graph-feasibility.md), [12 §3.2](12-security-compliance.md) | Lizenz/Finanziell | M | H | 6 | RSC-First für Pilot-Teams; Pay-per-use nur in Pilot-Scope mit Budget-Cap; Cost-Counter pro Tenant/Tag; tenant-weiter Rollout erst nach Vertragsfreigabe (MVP4) | Product Owner / Finance | Offen |
| R-02 | Teams-Premium / Copilot-Lizenz beim Meeting-Organisator fehlt → Transkripte sind nicht abrufbar, Capability „Meeting-Briefing" lückenhaft | [11 §3.4, §6.2](11-graph-feasibility.md) | Lizenz | L | M | 2 | **Mitigiert durch E-D3 / ADR-30** ([15 A18](15-open-questions-next-steps.md)): Teams Premium / M365 Copilot für alle Pilot-Teilnehmenden vorhanden; Pflicht-Setup-Check + Lizenz-Drift-Monitor (Graph) als Alert; bei externer Organisatorrolle weiterhin dokumentierter Funktionsverzicht. | Lizenzmanagement | **Mitigiert (E-D3)** |
| R-03 | RSC-Skalierung: App muss in jedes Pilot-Team installiert werden (500 Teams ⇒ 500 Installationen), Auto-Install politisch sensibel | [11 §6.3](11-graph-feasibility.md), [05 §2.3](05-teams-app.md) | Operativ | M | M | 4 | Setup-Policy + Graph `installedApps`-Skript; Subscription auf `groups`-Erstellung für Neuanlagen; Betriebsrat einbeziehen | M365-Admin | Offen |
| R-04 | Encrypted Content Subscriptions (Teams `getAllMessages`): Verlust des Private Key macht Notifications nicht wiederherstellbar | [11 §6.4](11-graph-feasibility.md) | Tech | L | H | 3 | Key Vault Premium/HSM, Cert-Backup, Cert-Rotation-Runbook, Decrypt-Failure-Monitoring | Platform/Security | Offen |
| R-05 | Microsoft Graph Throttling (10k Req/10min/Mailbox; Teams enger) führt zu Backlog und verzögerter BC-Sichtbarkeit | [11 §6.5](11-graph-feasibility.md), [07 §7](07-ingestion-pipeline.md) | Tech | M | M | 4 | `Retry-After` respektieren, exp. Backoff, Concurrency-Caps (16/Mailbox, 8 tenant-weit Teams), Backfill in Low-Prio-Queue | Backend-Team | Offen |
| R-06 | Subscription-Renewal-Fehler (Mail ≤3d, Teams ≤60min) führen zu Datenlücken | [11 §2.2, §6.6](11-graph-feasibility.md), [07 §9](07-ingestion-pipeline.md) | Operativ | M | H | 6 | Subscription Manager (Timer), Lifecycle-Notifications, Restlaufzeit-Alarm < 25 %, Delta-Query-Fallback | Platform | Offen |
| R-07 | Application Permission `Mail.Read` ohne Application Access Policy gewährt Tenant-weiten Mailzugriff | [11 §2.4](11-graph-feasibility.md), [12 §3, §4](12-security-compliance.md) | Datenschutz/Compliance | M | H | 6 | `New-ApplicationAccessPolicy` auf Pilot-Sicherheitsgruppe; Monitoring auf Policy-Änderungen; Pilotabgrenzung verbindlich | Exchange-Admin / DPO | Offen |
| R-08 | Mitbestimmungspflichtigkeit (§87 BetrVG / nationales Pendant) – Produktivnahme ohne Betriebsvereinbarung in DE/AT blockiert | [12 §10, §10.3, §18](12-security-compliance.md) | Compliance | M | H | 6 | **Wahrscheinlichkeit reduziert durch E-D2 / ADR-28** (Pilot ohne BV unter Schutzbedingungen: Opt-in-Consent ≤ 50 MA, nur externe Kommunikation, keine Leistungs-/Verhaltenskontrolle, Befristung 6 Monate mit Re-Consent, Information BR/SA/MAV vor Start, Verzicht auf Telefonie/Privat-Erfassung). **Auswirkung bleibt H** für MVP4: BV gem. §87 BetrVG **zwingend vor MVP4** (Voll-Rollout). **Restrisiko**: Freiwilligkeit unter Abhängigkeit (§26 Abs. 2 BDSG); BV-Verhandlung kann MVP4 verzögern. Maßnahmen: Default-Ausschluss interner Kommunikation, keine Mitarbeiter-Aggregation, Vier-Augen-Prinzip, frühzeitiger BR-Dialog ab MVP3. | HR / DSB | **Mitigiert für Pilot, offen für MVP4 (E-D2)** |
| R-09 | DPIA (Art. 35 DSGVO) erforderlich – ohne Abschluss keine Produktivnahme | [12 §8.4, §18](12-security-compliance.md) | Compliance | M | H | 6 | DPIA-Auftrag früh, dieses Dokument liefert Bausteine; Risikobewertung iterativ | DSB | Offen |
| R-10 | Prompt-Injection aus Mail-/Dokumenten-Inhalten manipuliert Modellverhalten (Datenexfiltration, falsche Vorschläge) | [12 §12](12-security-compliance.md), [08 §1 L8, §5](08-ai-orchestration.md) | Tech/Compliance | M | H | 6 | System-Prompt-Härtung, `<UNTRUSTED_SOURCE>`-Tags, Tool-Allowlist (kein send/buchen), Output-Filter, Prompt-Injection-Klassifikator C7, SIEM-Counter | AI-Eng / Security | Offen |
| R-11 | AI-Halluzination mit Geschäftsfolge: falscher Liefertermin/Preis im Antwortvorschlag wird vom Benutzer übernommen | [12 §18 #11](12-security-compliance.md), [08 §1 L1–L3](08-ai-orchestration.md) | Tech/Operativ | M | H | 6 | Grounded AI mit Pflicht-Citations, `confidence`-Feld, „Source-First UI", Trainings, Schema-Validation, Konfidenz-Cutoff | AI-Eng / UX | Offen |
| R-12 | Cross-Tenant-Leakage über Azure AI Search (fehlende `tenantId`/`companyId`-Filter) | [12 §1.2, §7](12-security-compliance.md), [09 §1 P3, §4](09-data-search.md) | Datenschutz | L | H | 3 | Pflichtfilter serverseitig generiert, Index pro Tenant, Cross-Tenant-Negativtests in CI, ACL-Felder pro Doc | Backend / QA | Offen |
| R-13 | AI-Aufruf umgeht BC-/M365-Berechtigungen (Post-hoc-Redaction statt Pre-Filter) | [12 §6](12-security-compliance.md), [08 §1 L5](08-ai-orchestration.md) | Datenschutz | L | H | 3 | Pre-AI Permission Resolver verbindlich; jede Quelle vor Prompt geprüft; Audit `ai.suggestion.*` | Backend | Akzeptiert / Mitigiert |
| R-14 | Restore aus Backup reaktiviert gelöschte Daten ⇒ Verstoß gegen Recht auf Löschung | [12 §9.4, §16.4, §18 #7](12-security-compliance.md) | Datenschutz/Operativ | L | H | 3 | Replay-Mechanismus für Lösch-Tickets, Crypto-Shredding via CMK-Rotation, Restore-Runbook mit verpflichtendem Replay-Schritt | Platform / DPO | Offen |
| R-15 | Erfassung privater Mails / sensibler Mitarbeiterdaten trotz Default-Ausschluss (Heuristik unzuverlässig, Sensitivity Labels nicht gesetzt) | [12 §10.2, §11, §18 #8](12-security-compliance.md), [11 §2.5](11-graph-feasibility.md) | Datenschutz | M | H | 6 | MIP-Labels respektieren, Privat-Ordner/-Kategorie hart skip, Beanstandungs-Workflow mit Sofortlöschung, Schulung | DPO / Betriebsrat | Offen |
| R-16 | Federated Konzern-Tenants werden fälschlich als „extern" klassifiziert oder umgekehrt | [11 §3.6, §8 #6](11-graph-feasibility.md), [10 §7](10-matching.md) | Operativ | M | M | 4 | Konfigurierbare „interne Tenants"-Liste, Domain-Allowlist, Setup-Tabelle `Communication Internal Domain` | M365-Admin | Offen |
| R-17 | Falsch-Matching: Verteiler / generische Postfächer / Reseller-Gateways führen zu falschen Customer-Zuordnungen | [10 §7](10-matching.md) | Tech/Operativ | M | M | 4 | Schwellen ≥0,85/0,60, Tie-Break über S5/S7/S10, manuelle Korrektur lernt Prior, generische Domains gedämpft | AI-Eng | Mitigiert |
| R-18 | Modellwechsel (z. B. GPT-4.1 → Nachfolger) bricht Prompt-/Schema-Verträge | [08 §3.3, §15](08-ai-orchestration.md) | Tech | M | M | 4 | Modell-Pinning, Canary, Prompt-Template-Versioning, Schema-Validation, Modell-Changelog | AI-Eng | Offen |
| R-19 | Embedding-Modell-Wechsel macht Index inkompatibel; Re-Embed teuer | [09 §1 P7, §2](09-data-search.md), [08 §3.1](08-ai-orchestration.md) | Tech/Finanziell | L | M | 2 | Alias-Switch-Strategie (`*-current` → `vN+1`), `embeddingModelVersion` pro Doc, kalkulierter Re-Embed-Job | AI-Eng | Offen |
| R-20 | Azure OpenAI EU Data Boundary: Abuse-Monitoring-Logs erfassen vertrauliche Inhalte | [08 §3.2](08-ai-orchestration.md), [12 §8.2, §18 #5](12-security-compliance.md) | Datenschutz | M | M | 4 | „No Log Retention / Abuse Monitoring opt-out" mit Microsoft beantragen; Modell-Region pinnen | DPO / Procurement | Offen |
| R-21 | DLQ-Stau / Poison Messages durch Schema-Drift in Graph-Payloads | [01 §7, §10](01-architecture.md), [07 §8](07-ingestion-pipeline.md) | Tech | M | M | 4 | DLQ-Alerts, MaxDelivery=5, Operator-Replay-Tool, Contract-Tests gegen Graph-Schema | Backend | Offen |
| R-22 | BC SaaS Custom-API-Limits / Throttling bei Burst-Schreiben aus Ingestion | [03 §1](03-bc-apis.md), [01 §11](01-architecture.md) | Tech | M | M | 4 | S2S-Token-Caching, Bulk-PATCH wo möglich, Outbox-Pattern mit Backoff, Sessions auf `conversationId` reduzieren Parallelität pro Thread | BC-Team | Offen |
| R-23 | Anhänge-Strategie unklar: Originale in BC vs. nur Referenz – beeinflusst DSGVO-Lösch- und Auskunftsprozesse | [01 §13 #7](01-architecture.md), [02 §1](02-bc-data-model.md), [11 §4](11-graph-feasibility.md) | Datenschutz/Tech | M | M | 4 | Beschluss „nur Referenz auf SharePoint/Blob" (ADR-20), `Communication Attachment` mit `webUrl`+`driveItemId`, kein Re-Hosting | Architektur | Mitigiert |
| R-24 | Mandant↔BC-Company-Mapping (1:n / n:m) ungeklärt → Multi-Tenant-Isolation fehleranfällig | [01 §8, §13 #3](01-architecture.md), [12 §7](12-security-compliance.md) | Tech | M | H | 6 | Tenant-Config-Repo (Cosmos), Pflichtfelder `tenantId`+`companyId` in jedem Datensatz, Cross-Check Token-Claim ↔ Header | Architektur | Offen |
| R-25 | DR-Strategie: aktiver Failover Sweden ↔ West Europe vs. Restore – Datenresidenz vs. RTO | [01 §11, §13 #9](01-architecture.md), [12 §16.4](12-security-compliance.md) | Operativ | L | M | 2 | Re-Index aus Source-of-Truth (BC + Graph) als DR-Strategie für AI Search; Cosmos PITR; Region-Failover-Runbook; RPO ≤15min, RTO ≤4h Tier 1 | Platform | Offen |
| R-26 | Kostenexplosion Azure OpenAI Token-Verbrauch bei wachsendem Volumen | [08 §2, §3](08-ai-orchestration.md), [11 §6.1](11-graph-feasibility.md) | Finanziell | M | M | 4 | Modell-Routing (Mini für Klassifikation/Extraktion/Reranker, GPT-4.1 nur für Briefings/Reply), Token-Quota pro Mandant, semantischer Cache | AI-Eng / Finance | Offen |
| R-27 | Outlook Add-in `ReadWriteMailbox` würde Admin Consent erzwingen und Risikobewertung erschweren | [04 §2.2](04-outlook-addin.md) | Compliance | L | M | 2 | Begrenzung auf `ReadWriteItem`; serverseitiger Graph-Zugriff via OBO; Aufgaben-Anlage server-seitig | Add-in-Team | Mitigiert |
| R-28 | Audit-Manipulation in Log Analytics (kein immutabler Speicher) gefährdet Nachweispflicht | [12 §13](12-security-compliance.md) | Compliance | L | H | 3 | Dedizierter Audit-Workspace (immutable / Lock), getrennte Schreib-/Leserechte, optional Hash-Chain, SIEM-Anbindung | Security | Offen |
| R-29 | Doppelerfassung (Add-in + Ingestion) führt zu Duplikaten in BC-Timeline | [07 §1, §6](07-ingestion-pipeline.md) | Tech | L | M | 2 | Idempotenz-Key `tenantId|resource|etag`, Source of Truth `Communication Source Reference`, Update statt Insert | Backend | Mitigiert |
| R-30 | Sites.Selected pro Tenant aktiv konfigurierbar / Roll-out-Aufwand unklar | [12 §18 #9](12-security-compliance.md), [11 §4](11-graph-feasibility.md) | Operativ | M | M | 4 | Site-Allowlist je Tenant, Onboarding-Workflow, Tenant-Bootstrap-IaC | M365-Admin | Offen |
| R-31 | Externe S/MIME-/OME-verschlüsselte Mails liefern nur verschlüsselten Body → keine Inhaltsanalyse | [11 §2.5](11-graph-feasibility.md) | Tech | L | L | 1 | Heuristik „encrypted" → nur Metadaten, kein AI-Pfad, klar dokumentiert | Backend | Akzeptiert |
| R-32 | Telefonie / Teams Phone: nur Metadaten verfügbar, keine Inhalte/Transkripte | [11 §6.7](11-graph-feasibility.md) | Tech | M | L | 2 | Aus MVP-Scope entfernen oder als Erweiterungsoption markieren | Product Owner | Akzeptiert |

**Anzahl Risiken:** 32.

---

## 2. Risiko-Heatmap (P × I)

| | **I = Niedrig** | **I = Mittel** | **I = Hoch** |
|---|---|---|---|
| **P = Hoch** | – | – | R-08 (9), R-02 (6) |
| **P = Mittel** | R-32 | R-03, R-05, R-16, R-17, R-18, R-20, R-21, R-22, R-26, R-30 | R-01, R-06, R-07, R-09, R-10, R-11, R-15, R-24 (je 6) |
| **P = Niedrig** | R-31 | R-19, R-25, R-27, R-29 | R-04, R-12, R-13, R-14, R-28 (je 3) |

Legende Score: 9 kritisch · 6 hoch · 3–4 mittel · 1–2 niedrig.

---

## 3. Top-10-Risiken im Detail

### R-08 – Mitbestimmungspflichtigkeit (Score 9)

- **Beschreibung:** Das System ist nach §87 Abs. 1 Nr. 6 BetrVG (DE, äquivalent AT/CH) **mitbestimmungspflichtig**, da es geeignet ist, Verhalten/Leistung der Beschäftigten zu überwachen – auch wenn das nicht der Zweck ist.
- **Wirkung:** Ohne Betriebsvereinbarung **keine Produktivnahme** in DE/AT möglich; Pilot rechtlich unsicher.
- **Mitigation:** Frühzeitig Verhandlungen aufnehmen; verbindlicher Default-Ausschluss interner Kommunikation; technisches Verbot von Mitarbeiter-Aggregationen (kein Filter `Owner = <Person>`); Vier-Augen-Prinzip für Setup-Änderungen; Transparenz-Hinweise; Schulungen.
- **Trigger / Frühwarnsignale:** BR-Verhandlung stockt; Anfragen aus HR; Beschwerden Beschäftigter; ungewollte Reports werden gewünscht.
- **Akzeptanz / Restrisiko:** Ohne Abschluss bleibt Produktivstart blockiert. Nach Abschluss Restrisiko mittel: laufendes Monitoring auf Zweckabweichung.

### R-01 – Pay-per-use-Kostenexplosion Teams Export API (Score 6)

- **Beschreibung:** Tenant-weite Application-Permission auf Teams-Nachrichten ist über Modell B kostenpflichtig (~0,00075 USD/Msg). Bei 1 Mio Msg/Mo: ~750 USD/Mo; 10×: ~7.500 USD/Mo; Backfill: ~9.000 USD einmalig.
- **Wirkung:** Erhebliche, nicht budgetierte Betriebskosten; Skalierung blockiert ohne Freigabe.
- **Mitigation:** RSC-First für Pilot-Teams (kein Pay-per-use), Pay-per-use nur in Pilot-Scope mit Budget-Cap und Cost-Counter pro Tenant/Tag, Vorfilter externer Beteiligung **vor** Inhaltsabruf, tenant-weiter Rollout erst nach Vertrags- und Compliance-Freigabe (MVP4).
- **Trigger:** Kosten-Counter > Schwellwert/Tag; Backfill geplant; Anfrage tenant-weiter Rollout.
- **Restrisiko:** Mittel – RSC reduziert Kosten, organisatorischer Rollout-Aufwand bleibt.

### R-02 – Fehlende Teams-Premium/Copilot-Lizenz für Transkripte (Score 6)

- **Beschreibung:** Transkripterzeugung erfordert Teams Premium oder M365 Copilot beim Organisator. Ohne Lizenz existiert kein Transkript zum Lesen.
- **Wirkung:** Capability „Meeting-Briefing" lückenhaft; Meetings mit externer Organisatorrolle haben nie Transkript bei uns.
- **Mitigation:** Lizenzbestand erheben; Capability als optional kennzeichnen; bei externer Organisatorrolle dokumentierter Funktionsverzicht; alternative Pull-Strategie via OneMeeting-Recording-Artifact wenn vorhanden.
- **Trigger:** UAT meldet „kein Transkript"; KPI „Transkript-Abdeckung" fällt unter Schwelle.
- **Restrisiko:** Hoch – Lizenzpolitik außerhalb unserer Kontrolle.

### R-06 – Subscription-Renewal-Fehler ⇒ Datenlücken (Score 6)

- **Beschreibung:** Mail-Subscriptions ≤3 Tage, Teams ≤60 min Lebensdauer. Renewer-Ausfall ⇒ verpasste Notifications; `missed`/`reauthorizationRequired` ohne Behandlung ⇒ stille Datenlücke.
- **Wirkung:** Unvollständige Kommunikationshistorie, fehlende Compliance-Nachweise.
- **Mitigation:** Subscription Manager (Timer), Lifecycle-Notifications, Restlaufzeit-Alarm < 25 %, Delta-Query als Backfill/Fallback, periodischer Reconcile-Sync alle 6 h pro Quelle.
- **Trigger:** Missed-Lifecycle-Events; Alarm „Restlaufzeit < 25 %"; Decrypt-Fail-Spitzen.
- **Restrisiko:** Niedrig nach Implementation der Mehrfach-Sicherung.

### R-07 – Mail-Application-Permission ohne Access Policy (Score 6)

- **Beschreibung:** `Mail.Read` (Application) gewährt per Default Tenant-weiten Zugriff auf alle Postfächer. Application Access Policy ist Exchange-Admin-Setting, nicht in Graph sichtbar; Änderungen können den Service still abschneiden.
- **Wirkung:** Datenschutzverletzung wenn Policy fehlt/falsch; alternativ stiller Funktionsverlust.
- **Mitigation:** `New-ApplicationAccessPolicy` auf Pilot-Sicherheitsgruppe verbindlich vor Produktivnahme; Monitoring auf Policy-Änderungen; dokumentierte Pilotabgrenzung; regelmäßige Verifikation via Test-Postfach außerhalb der Pilotgruppe.
- **Trigger:** Test-Postfach liefert Daten; Pilotgruppe wächst ungeprüft.
- **Restrisiko:** Mittel; mit Monitoring beherrschbar.

### R-09 – DPIA-Pflicht (Score 6)

- **Beschreibung:** DPIA (Art. 35 DSGVO) ist erforderlich (großer Umfang personenbezogener Daten, neue Technologie LLM, Beschäftigtenbezug).
- **Wirkung:** Ohne DPIA-Abschluss keine Produktivnahme; Aufsichtsbehörden-Risiko.
- **Mitigation:** DPIA frühzeitig beauftragen; dieses Dokument + 12-security-compliance.md liefern Bausteine; iterative Risikoanpassung; ggf. Architektur-Rückkopplung.
- **Trigger:** DPIA-Termin verschoben; neue Datenkategorien/Quellen ergänzt; Subprozessor-Wechsel.
- **Restrisiko:** Mittel.

### R-10 – Prompt Injection (Score 6)

- **Beschreibung:** Mail-/Dokumenten-Inhalte sind Untrusted Content. Versuche, Modellverhalten zu ändern, Daten zu exfiltrieren oder falsche Vorschläge zu erzwingen.
- **Wirkung:** Datenexfiltration, falsche Antwortvorschläge, Reputationsschaden.
- **Mitigation:** System-Prompt-Härtung; `<UNTRUSTED_SOURCE>`-Tags; Tool-Allowlist (kein send/buchen); Output-Filter (Schema-Validation, Empfänger-Allowlist, Secret-Regex); vorgelagerter Prompt-Injection-Klassifikator (C7); SIEM-Counter und Pen-Test mit LLM-Red-Teaming.
- **Trigger:** Counter `prompt_injection_attempts` Spitze; ungewöhnliche Output-Felder; Schema-Validation-Fails.
- **Restrisiko:** Mittel – LLM-Sicherheitsforschung im Fluss; jährliches Red-Teaming.

### R-11 – AI-Halluzination mit Geschäftsfolge (Score 6)

- **Beschreibung:** Modell erfindet Liefertermin/Preis/Vertragsdetails; Benutzer übernimmt Vorschlag ungeprüft.
- **Wirkung:** Falsche Zusagen an Kunden, finanzieller/juristischer Schaden.
- **Mitigation:** Grounded AI mit Pflicht-Citations (≥1 Quelle pro Aussage), `confidence`-Feld, „Source-First UI" (Quellen vor Vorschlag), Schema verbietet Aussagen ohne Quelle, Konfidenz-Cutoff → „unsicher, manuelle Bearbeitung", Schulung, Trainings.
- **Trigger:** Akzeptanzrate ohne Korrektur sehr hoch; Kundenbeschwerden; Stichproben-Reviews.
- **Restrisiko:** Mittel – „No Auto-Send" begrenzt Schaden, finale Verantwortung beim Benutzer.

### R-15 – Erfassung privater Inhalte (Score 6)

- **Beschreibung:** Default-Ausschluss interner Kommunikation, Privatmarkierungen unzuverlässig; nicht jeder Mitarbeiter nutzt Sensitivity Labels.
- **Wirkung:** Verstoß gegen Beschäftigtendatenschutz, Vertrauensverlust.
- **Mitigation:** MIP-Labels respektieren, Privat-Ordner/-Kategorie hart skip, Newsletter-Heuristik, Beanstandungs-Workflow „Eintrag irrtümlich erfasst → Sofortlöschung", Schulung, regelmäßige Stichproben durch DSB.
- **Trigger:** Meldungen via Beanstandungs-Workflow; auffällige PII-Detection-Quote.
- **Restrisiko:** Mittel – Heuristik bleibt fehleranfällig.

### R-24 – Mandant ↔ BC-Company-Mapping (Score 6)

- **Beschreibung:** 1:n / n:m unklar; Multi-Tenant-Isolation hängt am Mapping.
- **Wirkung:** Cross-Tenant-/Cross-Company-Leakage; falsche Berechtigungsentscheidungen.
- **Mitigation:** Tenant-Config-Repo (Cosmos), Pflichtfelder `tenantId`+`companyId` in jedem Datensatz und Index; Cross-Check Token-Claim `tid` ↔ Header `x-ccc-tenant`; BC-API prüft `x-ccc-bc-company` ↔ Datensatz; Cross-Tenant-Negativtests in CI.
- **Trigger:** Onboarding eines neuen Mandanten; Konzern-Konstellation; Audit-Findings.
- **Restrisiko:** Niedrig nach Mapping-Beschluss + Tests.

---

## 4. Architecture Decision Records (ADR-light)

> Konsolidiert aus den Plandokumenten. Status: **A**=Akzeptiert, **V**=Vorgeschlagen, **G**=Geprüft & Akzeptiert mit Restrisiko, **E**=Ersetzt.

| ID | Titel | Status | Kontext | Entscheidung | Begründung | Konsequenzen | Quelle |
|---|---|---|---|---|---|---|---|
| ADR-01 | Hosting Copilot API | A | Synchroner, latenzkritischer HTTP-Workload mit Streaming | Azure App Service (Linux Container, P1v3+, .NET 8 / ASP.NET Core) | Bessere Kontrolle Cold-Starts, Networking, Sidecars; Slots/Canary; Auto-Scale | App-Service-Plan-Kosten; Container-Build-Pipeline | [01 §12](01-architecture.md) ADR-01 |
| ADR-02 | Hosting Ingestion + Worker | A | Event-getrieben (Webhook, Service Bus, Timer) | Azure Functions Premium EP1, .NET 8 isolated, Pre-Warmed | Auto-Scale nach Backlog, VNet-Integration, integrierte Trigger | Premium-Mindestkapazität-Kosten | [01 §12](01-architecture.md) ADR-02, [07 §2](07-ingestion-pipeline.md) |
| ADR-03 | Messaging | A | Entkopplung Ingestion ↔ Worker, FIFO je Conversation | Azure Service Bus Premium, Topics + Subscriptions, Sessions auf `conversationId`/`chatId`, DLQ, Duplicate Detection | FIFO je Konversation, Pub/Sub-Fanout, Private Endpoint, Premium-Latenz | Höhere Kosten als Storage Queue; Storage Queue/Event Grid ungeeignet | [01 §7, §12](01-architecture.md) ADR-03 |
| ADR-04 | Suche / Wissensspeicher | A | RAG-Quelle, Multi-Tenant, ACL | Azure AI Search Hybrid (BM25 + Vector + Semantic Ranker), Index pro Tenant | Weniger Eigenbau, integrierte Vektorisierer, ACL-Filter, Skillsets | Lizenz S2 Prod; Re-Embed bei Modellwechsel | [01 §12](01-architecture.md) ADR-04, [09 §1, §2](09-data-search.md) |
| ADR-05 | Indexierungs-Strategie | A | Near-real-time vs. Batch | Push-Indexierung (Worker schreibt) statt Pull-Indexer | Latenz P95 ≤ 60 s, ACL-Anreicherung im Worker, kontrolliertes Schema | Worker trägt Indexier-Last; Bulk-Backfill via separater Pull-Indexer-Pfad optional | [09 §2, §5.1](09-data-search.md) |
| ADR-06 | LLM | A | Reasoning, Reply-Drafts, Briefings, Klassifikation | Azure OpenAI Sweden Central – GPT-4.1 (Reasoning/Reply), GPT-4o-mini (Klassifikation/Extraktion/Reranker), text-embedding-3-large (3072) | EU Data Boundary, Function Calling, Streaming, Modell-Pinning, Kosten-/Qualitäts-Routing | Modell-Versionierung Pflicht; Re-Embed bei Wechsel | [08 §3](08-ai-orchestration.md), [01 §12](01-architecture.md) ADR-05 |
| ADR-07 | AuthN/AuthZ | A | UI vs. Background, Least Privilege | Entra ID + OBO + App Roles; Application Permissions nur für Ingestion mit Application Access Policy | Delegated für UI, Application für Background; klare Audit-Trennung | Mailbox Application Access Policy operativ Pflicht | [01 §12](01-architecture.md) ADR-06, [12 §2, §4](12-security-compliance.md) |
| ADR-08 | BC-Integration | A | Stabile Vertragsschicht, kein Direktzugriff | BC Custom APIs (OData v4, AL) mit S2S-OAuth (Azure AD App + Cert) | Stabile API, Audit, keine BC-Tabellen-Direktzugriffe | Versionierung, Permission-Set `IOI_COMMHUB_API` | [01 §12](01-architecture.md) ADR-07, [03 §1](03-bc-apis.md) |
| ADR-09 | Add-in-Backend-Topologie | A | Add-ins benötigen BC-Daten | S2S-only für BC-APIs, Add-ins nur via Backend-Proxy (kein direkter Add-in→BC-Aufruf) | Einheitliche Berechtigungs-, Quellen- und Audit-Logik im Backend | Backend wird zum Pflicht-Hop; Latenz-Budget berücksichtigen | [03 §1](03-bc-apis.md), [04 §1.3](04-outlook-addin.md) |
| ADR-10 | Datenresidenz | A | DSGVO, Kunden-Erwartung | EU – Sweden Central / West Europe; alle PaaS in EU; CMK in Mandanten-Key-Vault | Azure OpenAI EU Boundary, geringere Drittlandsrisiken | Kein Cross-Region-Failover ohne DS-Freigabe | [01 §12](01-architecture.md) ADR-08, [09 §1 P6](09-data-search.md) |
| ADR-11 | Volltextspeicherung | A | „Keine unnötige Volltextspeicherung in BC" | BC speichert nur Metadaten / Snippets / Kurzzusammenfassungen / Quellenreferenzen | Datenminimierung, BC bleibt schlank, Volltext in AI Search/Blob/SharePoint | BC ≠ DR-Quelle für Volltexte; Re-Index aus SoT nötig | [02 §1](02-bc-data-model.md), [09 §1 P1](09-data-search.md), [11 §4](11-graph-feasibility.md) |
| ADR-12 | Visibility Scope | A | Berechtigungstreue Anzeige in BC | `User Visibility Scope` (Owner / Owner Team / Company) in BC, Default `Owner Team`; serverseitige `IsAllowedToView`-Codeunit | Default-Deny-Erweiterung; keine Manager-Sicht auf Mitarbeiter; ACL-Filter im Search-Index spiegelt Scope | Setup-Aufwand für Owner Teams; Migration-Skript bei Bestand | [02 §1, §7](02-bc-data-model.md), [12 §5](12-security-compliance.md) |
| ADR-13 | Pilot-Mailbox-Scope | A | Mail.Read ist tenant-weit per Default | Application Access Policy (Exchange) auf Pilot-Mail-Sicherheitsgruppe als verpflichtende Pilot-Beschränkung | DSGVO/Betriebsvereinbarung; kontrollierte Pilotabgrenzung | Operative Pflege per Exchange Admin; Monitoring auf Policy-Änderungen | [11 §2.4, §7](11-graph-feasibility.md), [12 §3, §4](12-security-compliance.md) |
| ADR-14 | Teams-Erfassungsstrategie MVP2 | A | Pay-per-use vs. RSC | RSC-First (`ChannelMessage.Read.Group`, `ChatMessage.Read.Chat`) für definierte Pilot-Teams; kein Pay-per-use im MVP1/MVP2 | Kostenkontrolle, granulare Auditierung; tenant-weiter Application-Pfad bleibt für MVP4 reserviert | RSC-Installation pro Team; Auto-Install-Skript + Subscription auf `groups`-Erstellung | [11 §3, §7](11-graph-feasibility.md), [05 §2.3](05-teams-app.md) |
| ADR-15 | Pre-AI-Permission-Resolver | A | AI darf keine Berechtigungen umgehen | Permission Resolver prüft jede Quelle **vor** Prompt-Aufnahme; Post-hoc-Redaction verboten | Verbindliches Sicherheits-Pattern; Index-Existenz ist kein Beweis für Leserecht | Höhere Latenz; Caching von Permission-Resolutions kurz (TTL) | [12 §6](12-security-compliance.md), [08 §1 L5](08-ai-orchestration.md) |
| ADR-16 | Reranker-Default | A | Latenz, Kosten, Erklärbarkeit | Logistic Regression Reranker als Default; LLM-Reranker nur bei Mehrdeutigkeit (Score 0,40–0,75 oder Top-5 innerhalb 0,05) | Schnell (<5 ms), kalibrierte Wahrscheinlichkeit, erklärbar; LLM nur wo nötig | Periodisches Retraining (monatlich); Per-Tenant-Modell ab ≥5.000 Events | [10 §3.2](10-matching.md) |
| ADR-17 | Idempotency-Key | A | Doppelerfassung, at-least-once | Idempotency-Key = External Hash (`SHA-256(tenantId + Source Message Id + Etag)`); Unique-Index in BC, Cache 90 d in Redis/Cosmos | Update statt Re-Insert; Doppel-Notifications no-op | Retention der Cache-Schicht 90 Tage | [02 §3.1](02-bc-data-model.md), [07 §6](07-ingestion-pipeline.md) |
| ADR-18 | Audit-Speicher | A | Manipulationsschutz, 7 Jahre | Audit in dediziertem immutable Log-Analytics-Workspace (Lock) + BC Audit-Tabelle für UI/Korrektur-Events; SIEM-Connector | Append-only, RBAC restriktiv, 7 Jahre | Workspace-Kosten; Hash-Chain optional | [12 §13](12-security-compliance.md), [01 §10](01-architecture.md) |
| ADR-19 | Outlook Add-in Permission | A | Manifest-Berechtigung minimal | `ReadWriteItem` statt `ReadWriteMailbox`; Mailbox-Zugriff serverseitig via OBO | Vermeidet Admin-Consent-Hürde; geringere Risiko-/Compliance-Bewertung | Aufgabenanlage server-seitig; Mobile-Kompromisse | [04 §2.2](04-outlook-addin.md) |
| ADR-20 | Anhänge-Strategie | A | DSGVO-Lösch-/Auskunftsprozesse, Speicherkosten | Anhänge nur referenzieren (SharePoint/Blob `webUrl` + `driveItemId`), nicht in BC speichern | Datenminimierung, einfacheres Löschen, kein Re-Hosting | Permission-Resolver muss SP-/Drive-ACL prüfen; Permalink-Stabilität | [02 §3](02-bc-data-model.md), [11 §4, §7](11-graph-feasibility.md), [01 §13 #7](01-architecture.md) |
| ADR-21 | Source-First UI | A | Grounded-AI-Erfahrung | „Source-First UI" als verpflichtendes UX-Pattern: Quellen-Chips sichtbar, Vorschlag erst nach Quellen, Edit zeigt Diff | Vertrauen, Auditierbarkeit, Halluzinations-Mitigation | UX-Aufwand höher; geringerer „Magic"-Effekt | [08 §1 L2](08-ai-orchestration.md), [04 §3](04-outlook-addin.md), [05 §3.1](05-teams-app.md) |
| ADR-22 | DR-Strategie AI Search | A | RPO/RTO ohne Geo-Replikation | Re-Index aus Source-of-Truth (BC + Graph + SharePoint) als DR-Strategie für AI Search | Index ≠ Backup; jederzeit rebuildbar; günstiger als Geo-Replikation | Re-Index-Dauer in RTO-Budget berücksichtigen | [09 §1 P8, §8/§9](09-data-search.md), [01 §11](01-architecture.md) |
| ADR-23 | Embedding-Versionierung | A | Modellwechsel ohne Downtime | Index-Aliase `*-current`, physische Indizes `*-{tenantId}-vN`; Re-Embed bei Modellwechsel ⇒ neuer `vN+1` und Alias-Switch | Zero-Downtime-Wechsel; alte Indizes parallel verfügbar | Re-Embed-Kosten; `embeddingModelVersion` pro Doc | [09 §2](09-data-search.md), [08 §3.1, §3.3](08-ai-orchestration.md) |
| ADR-24 | Multi-Tenant-Isolation | A | M365 × BC-Company als Mandantenbegriff | Mandant ≜ (M365-Tenant-ID × BC-Company); beide in jedem Datensatz; geteilte Compute, isolierte Daten (Search-Index, Blob-Container, Key Vault pro Tenant) | Skaleneffekt + harte Daten-Isolation | Onboarding-Workflow Pflicht; Cross-Check in jedem API-Hop | [01 §8](01-architecture.md), [12 §7](12-security-compliance.md) |
| ADR-25 | „Kein Auto-Send" als technisches Verbot | A | Grundprinzip 1 | Kein Tool für `sendEmail`/`postTeams`/`postBC`; nur lesende und vorschlagende Tools im LLM-Kontext | Technisch erzwungen, nicht nur prozessual | Aktionen via UI-Bestätigung; Aufwand für Compose-Insertion-Flows | [08 §1 L4](08-ai-orchestration.md), [12 §12.1](12-security-compliance.md) |
| ADR-26 | Modell-Routing | V | Kosten-/Qualitäts-Trade-off | Capability-spezifisches Modell-Routing (Mini für C1/C2/C5/C6/C7, GPT-4.1 für C3/C4b–e) zentral konfiguriert | Kostenkontrolle ohne Qualitätsverlust; A/B per Capability | `ModelRouter`-Konfiguration & Monitoring je Capability | [08 §3.1, §2](08-ai-orchestration.md) |
| ADR-27 | Mitarbeiter-Consent verpflichtend | A | Pilot ohne BV erfordert tragfähige Rechtsgrundlage für Beschäftigtendaten | Verpflichtende, freiwillige, dokumentierte **Opt-in-Einwilligung pro Pilot-MA** (DSGVO Art. 6 Abs. 1 lit. a + § 26 Abs. 2 BDSG), jederzeit widerrufbar mit sofortiger Wirkung; Consent-Register als eigene BC-Tabelle (`Communication Consent`, 50014); ohne gültige Einwilligung **technischer Ingestion-Ausschluss** der Mailbox (Stage 0) | Ermöglicht Pilotstart ohne BV, erhält Freiwilligkeitsprinzip, audit-pflichtig | BC-Tabelle + Widerruf-Workflow + Stage-0-Check; Audit aller Consent-Änderungen | [12 §10.3](12-security-compliance.md), [02 Tab. 50014](02-bc-data-model.md), [07 §4 Stage 0](07-ingestion-pipeline.md), [15 A16](15-open-questions-next-steps.md) |
| ADR-28 | Pilot ohne Betriebsvereinbarung unter Schutzbedingungen | A | BV-Verhandlung würde Pilot um Monate verzögern; Risiko R-08 sonst Blocker | Pilot (MVP1–MVP3) läuft **bewusst ohne BV** unter sechs Schutzbedingungen: Opt-in ≤ 50 MA, nur externe Kommunikation, keine Leistungs-/Verhaltenskontrolle, Befristung 6 Monate mit Re-Consent, Information BR/SA/MAV vor Start, kein Telefonie-/Privat-Scope. **BV vor MVP4 zwingend.** | Niedrigstes Risiko bei früher Pilotaufnahme; klare zeitliche Eskalation auf BV | Re-Consent bei Verlängerung; technisches Enforcement der Schutzbedingungen; rechtliche Begleitung | [12 §10.3](12-security-compliance.md), [14 R-08](14-risks-decisions.md), [13 §5.2](13-mvp-roadmap.md), [15 A17](15-open-questions-next-steps.md) |
| ADR-29 | Pflicht-Vollständigkeit der Kommunikationshistorie | A | Lücken in Historie sind fachlich nicht akzeptabel; AI-Briefings sonst unzuverlässig | **Pflicht-Backfill 24 Monate** pro Pilotpostfach (Drosselung gemäß Graph-Limits, Kosten-Cap), permanente Delta-Sync mit Lifecycle-Renewal, **Lücken-Monitor** (erkannte Conversation-IDs ohne erwartete Folgenachrichten), Outlook-/Teams-Plugin ergänzen, ersetzen aber nie die serverseitige Erfassung; Wiederaufnahme-Job nach Ausfällen mit **High-Water-Mark** | Garantiert Vollständigkeit; klare Verantwortlichkeit serverseitig | Höhere Graph-Kosten beim Backfill (Cap erforderlich); zusätzliche Pipeline-Stage „Gap Monitor"; Operations-Runbook für Wiederaufnahme | [07 §4 Stage 16, §11](07-ingestion-pipeline.md), [15 A19](15-open-questions-next-steps.md) |
| ADR-30 | Teams Premium / M365 Copilot Pflicht für Pilot-Teilnehmende | A | Ohne Lizenz keine Meeting-Transkripte ⇒ R-02 | **Lizenz-Vorhandensein als Pflicht-Setup-Check** vor Pilotaufnahme; **Lizenz-Drift-Monitor** via Graph als Alert; Pilot-Teilnehmende ohne aktive Lizenz werden automatisch aus Pilotgruppe entfernt | Entschärft R-02; sichert Meeting-Briefings (MVP3) | Lizenz-Kosten für alle Pilot-Teilnehmenden zu tragen; Monitoring-Aufwand | [13 §4.3](13-mvp-roadmap.md), [14 R-02](14-risks-decisions.md), [15 A18](15-open-questions-next-steps.md) |

**Anzahl ADRs:** 30.

---

## 5. Entscheidungs-Roadmap

Welche offenen Entscheidungen müssen bis wann getroffen sein?

### Vor MVP1 (Mail-Pilot)

| # | Entscheidung | Owner-Vorschlag | Bezug |
|---|---|---|---|
| D-1 | Pilot-Mail-Sicherheitsgruppe definiert + Application Access Policy gesetzt | Exchange-Admin + DPO | R-07, ADR-13 |
| D-2 | DPIA-Auftrag erteilt, erste Iteration abgeschlossen | DSB | R-09 |
| D-3 | Betriebsvereinbarung Pilot (Letter of Intent / Pilotvereinbarung) | HR + Betriebsrat | R-08 |
| D-4 | Mandant↔BC-Company-Mapping verbindlich (1:n / n:m, Setup-Tabelle) | Architektur | R-24, ADR-24 |
| D-5 | Region-Pinning Azure OpenAI + Modell-Deployment-Namen festgelegt | AI-Eng | ADR-06, ADR-10 |
| D-6 | „No Log Retention / Abuse Monitoring opt-out" mit Microsoft beantragt | Procurement | R-20 |
| D-7 | Embedding-Modell + Dimension final (3072 vs. 1536) | AI-Eng | ADR-06, R-19 |
| D-8 | Visibility-Scope-Default + Owner-Team-Konzept in BC | BC-Team | ADR-12 |
| D-9 | Sites.Selected-Allowlist (initial) | M365-Admin | R-30 |
| D-10 | Front-Door / APIM Bedarf (ja/nein) für Webhook-Endpunkt | Platform | [01 §13 #5, #12](01-architecture.md) |

### Vor MVP2 (Teams-Pilot via RSC)

| # | Entscheidung | Owner-Vorschlag | Bezug |
|---|---|---|---|
| D-11 | RSC-Auto-Install-Strategie (Setup-Policy, Pilot-Teams-Liste) | M365-Admin + Betriebsrat | R-03, ADR-14 |
| D-12 | Encrypted-Content-Cert in Key Vault (Premium/HSM) und Rotation-Runbook | Platform/Security | R-04 |
| D-13 | Bot-State-Speicher (Cosmos vs. Storage) | Backend | [01 §13 #11](01-architecture.md) |
| D-14 | Lizenzbestand Teams Premium / M365 Copilot beim Organisator | Lizenzmanagement | R-02 |
| D-15 | Federated-Konzern-Tenants als „intern" konfiguriert | M365-Admin | R-16 |
| D-16 | Budget-Cap Pay-per-use Pilot-Chats (falls aktiviert) | Finance | R-01 |

### Vor MVP3 (Meeting-Transkripte / Briefings)

| # | Entscheidung | Owner-Vorschlag | Bezug |
|---|---|---|---|
| D-17 | Verarbeitung von Transkripten – Hinweis an Teilnehmer-Workflow | DSB | R-15, [12 §10](12-security-compliance.md) |
| D-18 | Recording-Referenz-Strategie (nur webUrl, kein Re-Hosting) | Platform | ADR-20 |
| D-19 | Per-Tenant-Reranker-Modell (ab ≥5.000 Events) ja/nein | AI-Eng | ADR-16 |
| D-20 | DR-Region (aktiv/passiv vs. Re-Index aus SoT) | Platform | R-25, ADR-22 |

### Vor MVP4 (tenant-weiter Rollout)

| # | Entscheidung | Owner-Vorschlag | Bezug |
|---|---|---|---|
| D-21 | Vertragliche Freigabe Teams Export API Pay-per-use + Budget | Procurement + Finance | R-01 |
| D-22 | Vollständige Betriebsvereinbarung (final) | HR + Betriebsrat | R-08 |
| D-23 | Auftragsverarbeitungsvertrag ergänzt (Teams Export API) | Legal | [11 §8 #11](11-graph-feasibility.md) |
| D-24 | Backfill-Tiefe (Monate) und -Budget | Product + Finance | [11 §8 #12](11-graph-feasibility.md) |
| D-25 | Premium-Isolation-Schwelle (dedizierte Compute / Search-Replica) | Architektur | [01 §13 #6](01-architecture.md) |

---

## 6. Restrisiken nach MVP1

Nach Abschluss MVP1 (Mail-Pilot) verbleibende Restrisiken:

| ID | Restrisiko | Bewertung |
|---|---|---|
| R-02 | Transkript-Lücken bei externer Organisatorrolle | Akzeptiert; Capability als optional dokumentiert |
| R-08 | Mitbestimmung – Pilotvereinbarung deckt nur Pilot, nicht Vollbetrieb | Begrenzte Geltungsdauer; vor MVP2 erweitern |
| R-10 | Prompt Injection – LLM-Sicherheitsforschung im Fluss | Mittel; jährliches Red-Teaming, Monitoring-Counter |
| R-11 | AI-Halluzination mit Geschäftsfolge | Mittel – „No Auto-Send" + Source-First UI begrenzen Schaden |
| R-15 | Erfassung privater Inhalte trotz Heuristik | Mittel – Beanstandungs-Workflow + Stichproben |
| R-18 | Modellwechsel – Vertragsbruch trotz Versionierung | Niedrig – Canary + Pinning |
| R-20 | Abuse-Monitoring-Logs Azure OpenAI | Niedrig–Mittel je nach Microsoft-Vereinbarung |
| R-26 | Token-Kosten Wachstum | Niedrig (im Pilot) – Monitoring + Routing |
| R-31 | S/MIME/OME-verschlüsselte Mails ohne Inhalt | Akzeptiert; nur Metadaten |

Risiken R-01, R-03, R-04, R-12 (Cross-Tenant Search), R-13 (Pre-AI-Bypass), R-14 (Restore vs. Löschung), R-24 (Mandant-Mapping) sind **vor MVP1** technisch zu mitigieren, sodass das Restrisiko nach MVP1 niedrig ist.

---

## 7. Risiko-Review-Prozess

| Frequenz | Aktivität | Verantwortlich | Output |
|---|---|---|---|
| **Wöchentlich** | Operative Risiken (DLQ-Rate, Subscription-Renewal-Fehler, Pay-per-use-Counter, Prompt-Injection-Counter) auf Dashboard | Platform-Lead | Tickets bei Schwellwertbruch |
| **Zweiwöchentlich** | Risiko-Stand-up (15 min) – neue Risiken, Status-Updates Top-10 | Risk Owner (Dean-Debug) | Update Risikoregister |
| **Monatlich** | Vollständiger Risiko-Review mit Architektur, Security, AI-Eng, BC-Team, DSB | Architektur-Lead | Re-Scoring, Mitigation-Anpassung, ADR-Aktualisierung |
| **Quartalsweise** | Compliance-Review (DSGVO, Betriebsvereinbarung, AVV, Lizenzbestand) | DSB + HR + Procurement | Bericht an Geschäftsleitung |
| **Vor jedem MVP-Gate** | Re-Bewertung aller Risiken + Entscheidungs-Roadmap-Abgleich + Restrisiko-Bestätigung | Steering Committee | MVP-Gate-Entscheidung |
| **Bei Modell-/Prompt-Wechsel** | AI-Eval-Run inkl. Adversarial-Korpus + Risk-Re-Scoring R-10/R-11/R-18 | AI-Eng + QA | Eval-Report, ggf. Rollback |
| **Bei Subprozessor-Wechsel / neuer Quelle** | Ad-hoc DPIA-Update + Risk-Add | DSB | DPIA-Anhang, Risikoeintrag |
| **Jährlich** | Pen-Test + LLM-Red-Teaming + Risiko-Kompletter-Review | Security-Lead | Pen-Test-Report, Maßnahmenplan |
| **Nach Incident** | Post-Mortem mit Risk-Re-Scoring + neue Risiken | Incident Commander | Post-Mortem-Doc, neue Mitigationen |

**Eskalationspfad:** Risk-Owner → Architektur-Lead → Steering Committee → Geschäftsleitung. Trigger für Eskalation: Score-Anstieg, Schwellwertbruch, neuer regulatorischer Befund, MVP-Gate-Blocker.

---

*Verweise: [00-overview.md](00-overview.md), [11-graph-feasibility.md](11-graph-feasibility.md), [12-security-compliance.md](12-security-compliance.md), [13-mvp-roadmap.md](13-mvp-roadmap.md), [15-open-questions-next-steps.md](15-open-questions-next-steps.md).*
