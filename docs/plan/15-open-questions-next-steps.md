# 15 – Offene Fragen & Empfohlene nächste Schritte

> Konsolidierung aller offenen Fragen aus den Plandokumenten in [docs/plan/](.) sowie Ableitung nächster Schritte, Stakeholder-Zuordnung, Governance, Mitarbeiter-Kommunikation und Go/No-Go-Kriterien für MVP1.
>
> Quelle der Wahrheit für den fachlichen Auftrag: [`../../instructions.md`](../../instructions.md). Verifikation der Abdeckung gegen die Akzeptanzkriterien: [17-traceability.md](17-traceability.md).
>
> Konventionen: **Adressat** = Rolle/Funktion, die antworten/freigeben muss. **Frist** = spätester Klärungszeitpunkt relativ zu den MVP-Stufen aus [13-mvp-roadmap.md](13-mvp-roadmap.md) (MVP1 = Outlook + serverseitige Mail-Erfassung, MVP2 = Teams-Pilot, MVP3 = Meetings/Dokumente, MVP4 = unternehmensweiter Rollout) bzw. „laufend".

---

## 1. Bereits getroffene Annahmen (mit Begründung)

Diese Annahmen wurden im Planungsprozess festgelegt und sind die Basis aller weiteren Entscheidungen. Sie sind explizit revidierbar, müssen aber bei Änderung über den Architektur-Review (siehe §5) gehoben werden.

| # | Annahme | Begründung / Quelle |
|---|---|---|
| A1 | **Business Central SaaS (Online)** als Zielplattform; keine On-Premises-/Container-Variante. | [01 §1](01-architecture.md), [02 §1](02-bc-data-model.md). Custom APIs, S2S-Auth, AppSource-Vorgaben gelten ausschließlich für SaaS. |
| A2 | **Azure-Deployment in EU – Sweden Central (primär), West Europe (sekundär)**. | DSGVO, Azure-OpenAI-EU-Data-Boundary, Verfügbarkeit der GPT-4.1-/Embedding-Modelle ([01 ADR-08](01-architecture.md), [08 §3.2](08-ai-orchestration.md)). |
| A3 | **Multi-Tenant-Service** mit logischer Mandantentrennung: gemeinsame Compute-Ressourcen, mandantenspezifische Daten (Search-Index, Blob-Container, Key Vault) und CMK. | [01 §8](01-architecture.md), [12 §7](12-security-compliance.md). Geringere Betriebskosten, klare Isolations-Schicht. |
| A4 | **Modell-Stack**: GPT-4.1 (Reasoning, Briefings, Antwortvorschläge), GPT-4o-mini (Klassifikation, Extraktion, Reranker, Injection-Check), text-embedding-3-large (3072 dim, optional 1536 quantisiert). | [08 §3.1](08-ai-orchestration.md). Beste Qualität/Latenz/Kosten-Mischung in EU-Region. |
| A5 | **Push-Indexierung** als Default, Pull-Indexer ausschließlich für Backfill. | [09 §5.1](09-data-search.md). Ermöglicht Berechtigungslogik, Idempotenz, Reihenfolge je Konversation und Embedding-Kostenkontrolle. |
| A6 | **RSC-First für Teams** (`ChannelMessage.Read.Group`, `ChatMessage.Read.Chat`); Application-Permission-Pfad nur für unternehmensweite, automatische Erfassung in MVP4. | [05 §2.3](05-teams-app.md), [11 §3.2](11-graph-feasibility.md). Vermeidet Pay-per-use, ermöglicht klare Auditierbarkeit. |
| A7 | **S2S-Auth zu Business Central** (Client Credentials mit Zertifikat im Key Vault); BC-Custom-API als einzige Schnittstelle, Outlook-/Teams-Add-ins gehen ausschließlich über das Backend als Proxy. | [03 §1, §9](03-bc-apis.md). Einheitliche Berechtigungs-, Quellen- und Audit-Logik. |
| A8 | **In BC nur Metadaten, Links, Hashes, Zusammenfassungen, Quellenreferenzen** – Volltexte/Anhänge in Azure AI Search, Azure Blob, SharePoint. | [02 §1](02-bc-data-model.md), [09 P1](09-data-search.md). Datenminimierung, Performance, AppSource-Tauglichkeit. |
| A9 | **Default-Ausschluss interner Kommunikation**, Erfassung nur bei externen Beteiligten (Erfassungslogik 1–8 aus `instructions.md`). | [07 §4 Stage 2](07-ingestion-pipeline.md), [12 §10.1](12-security-compliance.md). DSGVO-Zweckbindung, keine Mitarbeiterüberwachung. |
| A10 | **Grounded AI mit Quellenpflicht und Pre-Inference-Filter**; keine Aussage ohne `sources[]`, keine sendenden Tools, Pre-Inference-Berechtigungsprüfung. | [08 L1–L8](08-ai-orchestration.md), [12 §6](12-security-compliance.md). |
| A11 | **Service Bus Premium mit Sessions** (`sessionId = conversationId`/`chatId`) als Eventing-Backbone; Event Grid nur ergänzend. | [01 §7, ADR-03](01-architecture.md). FIFO je Konversation, DLQ, Duplicate Detection. |
| A12 | **Sprachen**: Deutsch primär, Englisch Fallback; Französisch/Italienisch/Spanisch best effort. | [04 §11](04-outlook-addin.md), [08 §13](08-ai-orchestration.md). |
| A13 | **Object-ID-Range 50000–50099** für Pilotphase; AppSource-Range bei Microsoft beantragt vor MVP4. | [02 §2](02-bc-data-model.md). |
| A14 | **Append-only Audit-Log** in BC (`Communication Audit Log Entry`) plus Application Insights Custom Events; revisionssichere Persistenz in immutable Blob-Tier optional ab MVP3. | [02 §3.13](02-bc-data-model.md), [12 §13](12-security-compliance.md). |
| A15 | **Default Visibility Scope `Owner Team`**, Default Sensitivity `Internal`, Default Retention 10 Jahre bei Belegbezug, sonst 6 Jahre. | [02 §3.11](02-bc-data-model.md), [12 §9.2](12-security-compliance.md). || A16 | **Mitarbeiter-Einwilligung verpflichtend (Opt-in)** – DSGVO Art. 6 Abs. 1 lit. a + § 26 Abs. 2 BDSG; freiwillig, dokumentiert, jederzeit widerrufbar mit sofortiger Wirkung. Consent-Register als eigene BC-Tabelle (Audit-pflichtig). Ohne gültige Einwilligung wird die Mailbox vom Ingestion technisch ausgeschlossen. (Entscheidung E-D1) | [12 §10.3](12-security-compliance.md), [02 Tab. 50014](02-bc-data-model.md), [07 §4 Stage 0](07-ingestion-pipeline.md) |
| A17 | **Pilot ohne Betriebsvereinbarung** unter Schutzbedingungen: (a) Opt-in-Pilotgruppe ≤ 50 MA, (b) nur externe Kundenkommunikation, (c) keine Leistungs-/Verhaltenskontrolle, keine Manager-KPIs auf MA-Ebene, (d) befristet max. 6 Monate (Re-Consent bei Verlängerung), (e) Information BR/SA/MAV vor Start, (f) keine Telefonie-/Privat-Erfassung. **BV vor MVP4 zwingend.** (Entscheidung E-D2) | [12 §10.3](12-security-compliance.md), [13 §5.2](13-mvp-roadmap.md), [14 ADR-28](14-risks-decisions.md) |
| A18 | **Teams Premium / M365 Copilot für alle Pilot-Teilnehmenden vorhanden** – Meeting-Transkripte gelten als verfügbar. Pflicht-Setup-Check + Lizenz-Drift-Monitor (Graph) als Alert. R-02 entschärft. (Entscheidung E-D3) | [13 §4.3](13-mvp-roadmap.md), [14 R-02, ADR-30](14-risks-decisions.md) |
| A19 | **Vollständige Kommunikationshistorie ist Pflichtanforderung** – initialer **Backfill 24 Monate** pro Pilotpostfach (gedrosselt, Kosten-Cap), permanente Delta-Sync mit Lifecycle-Renewal, **Lücken-Monitor**, Add-ins ergänzen serverseitige Erfassung ohne sie zu ersetzen, Wiederaufnahme-Job mit High-Water-Mark. (Entscheidung E-D4) | [07 §4 Stage 16, §11](07-ingestion-pipeline.md), [14 ADR-29](14-risks-decisions.md) |
| A20 | **MVP1 Sprint 0 (Setup-Sprint) gestartet**, Dauer 2–3 Wochen. Ziel: „Definition of Ready für MVP1 Sprint 1". (Entscheidung E-D5) | [18-sprint-0-backlog.md](18-sprint-0-backlog.md), [13 §1.5](13-mvp-roadmap.md) |
---

## 2. Konsolidierte Liste offener Fragen

Quellen-Abkürzungen: `01` = `01-architecture.md`, `02` = `02-bc-data-model.md`, …, `16` = `16-testing-acceptance.md`.

### 2.1 Strategie & Scope

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-S-01 | Liefermodell: rein interne Kundenlösung oder ISV-/AppSource-Auslieferung an Dritte? | 12 §18.6, 02 §12.1 | Geschäftsführung, Produktmanagement | Datenmodell-IDs, Multi-Tenant-Strategie und AVV-Vorlagen werden unterschiedlich geschnitten. | Interne Lösung in MVP1–3, ISV-Pfad ab MVP4 prüfen. | vor MVP1 |
| OF-S-02 | Konzern- vs. Einzelunternehmens-Konstellation: mehrere BC-Companies in einem M365-Tenant – wie wird die Visibility-Trennung intern geregelt? | 01 §13.3, 02 §12.8, 12 §18.10 | Geschäftsführung, BC-Verantwortliche, DSB | Permission-Modell und Cross-Company-Reads müssen evtl. erweitert werden. | Pilot mit 1 BC-Company; Konzern-Sicht als MVP4-Erweiterung. | vor MVP1 |
| OF-S-03 | Pilot-Bereich (Vertrieb / Service / PM) – welcher Geschäftsbereich startet, mit wie vielen Postfächern? | 04 §14, 16 §14.9, 11 §8.4 | Geschäftsführung, Bereichsleitung | Pilot kann nicht gestartet werden; Erfassungs-Scope unklar. | **entschieden → siehe A17/A18** (Opt-in-Pilotgruppe ≤ 50 MA, Vertrieb/Service). | ~~sofort~~ erledigt |
| OF-S-04 | Erweiterung Telefonie/Call-Records: in Roadmap oder out of scope? | 02 §12.10, 07 §14.11, 11 §6.7 | Produktmanagement | Datenmodell-Erweiterungspunkte und Channel-Enum müssen ggf. reserviert werden. | Out of Scope für MVP1–4; Erweiterungspunkt im Schema offen halten. | laufend |
| OF-S-05 | Proaktive Kundenbriefings (Push-Benachrichtigung an Vertrieb) – im Scope oder erst nach Pilot? | 05 §14.5, 07 §4 Stage 14 | Bereichsleitung, DSB | UI- und Notifikations-Funktionen sowie DSGVO-Bewertung verschieben sich. | Erst MVP4 nach Pilotauswertung; vorher nur reaktiv. | vor MVP4 |

### 2.2 Lizenzen & Verträge

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-L-01 | M365-E5-Lizenzbestand: ausreichend für Modell A oder Pay-per-use günstiger? | 11 §8.1 | IT-Einkauf, M365-Admin | Teams-Erfassungs-Modell und Kostenrechnung nicht entscheidbar. | Lizenz-Inventur in den ersten 2 Wochen; Default RSC-First, Pay-per-use als Fallback mit Budget-Cap. | vor MVP2 |
| OF-L-02 | Teams Premium / M365 Copilot beim Meeting-Organisator – Verbreitung im Tenant? | 11 §8.3, 16 §14.3 | M365-Admin, Bereichsleitung | MVP3 Meeting-Transkripte ggf. weitgehend leer. | **entschieden → siehe A18** (alle Pilot-Teilnehmenden lizenziert; Lizenz-Drift-Monitor). | ~~vor MVP3~~ erledigt |
| OF-L-03 | DSGVO-Auftragsverarbeitungsvertrag (AVV) für Teams Export API / Pay-per-use – liegt vor? | 11 §8.11, 12 §8.6 | Recht/DSB, Einkauf | Kein produktiver Tenant-weiter Teams-Zugriff möglich. | AVV-Anforderung an Microsoft prüfen, ggf. Standard-DPA bestätigen. | vor MVP2 |
| OF-L-04 | Azure OpenAI „Abuse Monitoring / Human Review opt-out" – vertraglich beantragt? | 08 §3.2, 08 O2, 12 §18.5 | Recht/DSB, Microsoft Account Manager | Inhalte könnten kurzfristig durch Microsoft-Mitarbeiter eingesehen werden. | Antrag stellen; bis Bestätigung sensible Inhalte (Sensitivity ≥ Confidential) nicht durch AOAI verarbeiten. | vor MVP1 |
| OF-L-05 | Customer-Lockbox / Subprozessor-Liste – für ISV-Lieferung erforderlich? | 12 §18.6 | Recht, Compliance | Liefermodell ISV blockiert. | Klärung erst bei OF-S-01 = ISV. | vor MVP4 |
| OF-L-06 | Azure-Subscription-Modell: separate Subs pro Stage, EA/CSP-Vertrag mit ausreichendem Commitment für AOAI-PTU/PAYG? | 01 §6, 16 §2 | IT-Einkauf, Cloud-Architekt | Provisionierung & Kostenkontrolle nicht möglich. | EA-Sub mit Cost-Mgmt-Budget pro Stage; PAYG für AOAI im Pilot, PTU-Option ab MVP4. | sofort |

### 2.3 Datenschutz / Compliance / Mitbestimmung

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-D-01 | Datenschutz-Folgenabschätzung (DSFA/DPIA) – wer (DSB intern vs. extern) und bis wann? | 12 §8.4, 12 §18.2, 16 §14.4 | DSB, Geschäftsführung | Produktivnahme rechtswidrig. | DSFA durch internen DSB starten, externen Berater bei Engpass; vorab Bausteine aus [12](12-security-compliance.md) liefern. | sofort, abgeschlossen vor MVP1 |
| OF-D-02 | Betriebsvereinbarung (BV) gemäß § 87 Abs. 1 Nr. 6 BetrVG – Inhalte, Zeitplan, Eskalation bei Missbrauch? | 12 §10.1, 12 §18.1, 16 §14.5 | Personalvertretung, HR, DSB | Keine Pilot-/Produktivnahme in DE/AT zulässig. | **entschieden für Pilot → siehe A16/A17** (Pilot ohne BV unter Schutzbedingungen + Opt-in-Consent; **BV vor MVP4 zwingend**). | ~~vor MVP1~~ Pilot erledigt, BV vor MVP4 |
| OF-D-03 | Information externer Personen (Art. 13/14 DSGVO) – Standard-Disclaimer in Mail-Signaturen oder separate Information? | 12 §8.3, 12 §10.1 | DSB, Marketing/Kommunikation | Auskunfts- und Informationspflichten verletzt. | Standard-Disclaimer in geschäftlichen Signaturen + Datenschutzhinweis auf Webseite. | vor MVP1 |
| OF-D-04 | Privatmarkierungs-Erkennung – wie zuverlässig (Sensitivity Labels vs. Heuristik)? | 11 §8.9, 12 §10.1, 12 §18.8, 07 §14.6 | DSB, M365-Admin | Risiko der Erfassung privater Inhalte ⇒ DSGVO-Verstoß. | MIP-Labels primär; ergänzende Heuristik (Outlook-Kategorie „Privat", Subject-Patterns); regelmäßige Stichproben. | vor MVP1 |
| OF-D-05 | Crypto-Shredding / Restore-Replay – Backup-Lösch-Strategie endgültig festlegen? | 12 §9.4, 12 §18.7, 09 §8.4 | DSB, Cloud-Architekt | Recht auf Löschung kann bei Backup-Restore unterlaufen werden. | Replay-Job mit Lösch-Tickets; CMK pro Mandant für Crypto-Shredding-Option. | vor MVP3 |
| OF-D-06 | Sites.Selected (SharePoint) – pro Tenant aktiv konfigurierbar, wer pflegt Allowlist? | 12 §18.9 | M365-Admin, Bereichsleitung | Dokumentenintegration MVP3 verzögert. | Allowlist-Pflege durch IT-Admin; Site pro Customer/Project. | vor MVP3 |
| OF-D-07 | Edit-Distance- und Akzeptanz-Telemetrie – Mitarbeiterüberwachung? | 12 §18.11, 16 R10 | DSB, Personalvertretung | Pilot-Stop. | Opt-in-Telemetrie, ausschließlich aggregiert je Tenant, keine Einzelpersonen-Ausweisung. | vor MVP1 |
| OF-D-08 | Shared Mailboxes / Delegate Access – Owner-Zuordnung im Audit eindeutig? | 04 §15.6, 12 §18.12 | M365-Admin, DSB | Audit-Log nicht eindeutig. | Shared-Mailbox-Mitgliedschaft via Graph auflösen; Owner als Liste persistieren. | vor MVP1 |

### 2.4 Architektur & Cloud

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-A-01 | Front Door / APIM – für externe Konsumenten und Graph-Webhooks erforderlich? | 01 §13.5, 01 §13.12 | Cloud-Architekt, Security | Netz-Topologie und Kosten unklar. | APIM als Standard-Gateway, Front Door optional ab Multi-Region. | vor MVP1 |
| OF-A-02 | DR-Region: aktiver Active/Passive-Failover Sweden ↔ West Europe oder reine Backup/Restore-Strategie? | 01 §13.9 | Cloud-Architekt, IT-Leitung | RTO/RPO unklar; Kostenkalkulation differenziert. | Backup/Restore (RPO ≤ 15 min, RTO ≤ 4 h) in MVP1–3; Active/Passive ab MVP4 evaluieren. | vor MVP4 |
| OF-A-03 | Search-SKU & Sizing: S2 oder S3, Replica-/Partition-Plan pro Mandant; gemeinsamer vs. dedizierter Service? | 01 §13.4, 09 §7 | Cloud-Architekt | Performance- und Kostenrahmen offen. | S1 Pilot, S2 Prod (≥ 2 Replicas), dedizierter Service ab regulatorischer Anforderung. | vor MVP1 |
| OF-A-04 | Premium-Isolation: ab welcher Größe/Compliance-Stufe dedizierte Compute / Key Vault Premium HSM? | 01 §13.6 | Cloud-Architekt, Security | Mandantenspezifische SLA nicht erfüllbar. | Default Standard-Tier; dedizierte Ressourcen nur auf Antrag mit Aufpreis. | laufend |
| OF-A-05 | Bot-State-Speicher: Cosmos DB vs. Azure Storage – PII-Implikationen? | 01 §13.11 | Cloud-Architekt, DSB | Datenklassifikation unklar. | Cosmos DB SQL API, keine PII im State, nur unsensible Konversationskontext-Felder. | vor MVP2 |
| OF-A-06 | Audit-Persistenz: Log Analytics 90–730 Tage ausreichend oder immutable Blob mit Legal Hold? | 01 §13.10, 12 §13 | DSB, Security | Revisionssicherheit nicht gewährleistet. | Log Analytics 7 Jahre für Audit-Custom-Events + Spiegelung in immutable Blob für gesetzlich relevante Events. | vor MVP3 |
| OF-A-07 | Anhänge-Strategie: Originale persistieren vs. nur SharePoint-/OneDrive-Permalink? | 01 §13.7, 02 §3.3 | DSB, Cloud-Architekt | Lösch-/Auskunftsprozesse unterschiedlich. | Default: nur Referenz; Originale nur bei expliziter Belegverknüpfung in SharePoint-Bibliothek. | vor MVP3 |
| OF-A-08 | OpenAPI-Generierung der Custom APIs – aus AL-Annotationen + manuelle Bound-Action-Ergänzung? | 03 §13.7 | Backend-Lead, BC-Lead | API-Vertrag nicht maschinenlesbar. | Generation aus AL-Pages + Hand-Patch für Bound Actions, SemVer im URL. | vor MVP1 |
| OF-A-09 | BC-Outbound-Webhooks bei Korrekturen (`entityLink confirmed`) – synchron oder Storage-Queue? | 03 §13.6 | BC-Lead, Backend-Lead | Re-Indexierung verzögert. | Storage-Queue-basierter Outbound-Call aus BC-Job-Queue. | vor MVP1 |
| OF-A-10 | Cross-Company-Reads (Konzern): pro Company ein Aufruf vs. Backend-Aggregation? | 03 §13.9 | Backend-Lead, BC-Lead | Konzern-UX schlecht. | Backend-Aggregation, BC liefert pro Company. | vor MVP4 |

### 2.5 Microsoft Graph & Teams

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-G-01 | Application Access Policy (Exchange) – Pflege durch wen, Pilot-Sicherheitsgruppe sofort umsetzbar? | 07 §14.1, 11 §8.4, 12 §18.x | M365-Admin (Exchange) | MVP1 Mail-Erfassung blockiert. | Pflege durch zentrales M365-Admin-Team; Pilot-Gruppe in Woche 1 erstellt. | sofort |
| OF-G-02 | Federated Tenants als „intern" – welche Partner-/Konzern-Tenants gelten als intern, wo gepflegt? | 05 §14.4, 07 §14.2, 11 §8.6 | M365-Admin, Bereichsleitung | Mitarbeiter von Tochterfirmen werden fälschlich als „extern" geführt. | BC-Setup-Tabelle `Communication Internal Domain` mit Treat-As-Federated, gepflegt von ADMIN-Rolle. | vor MVP1 |
| OF-G-03 | Encrypted Content Subscriptions (Teams) – Key Vault Premium / HSM verfügbar, Cert-Rotation? | 07 §14.3, 11 §8.7 | Cloud-Architekt, Security | Teams-Subscriptions nicht aufsetzbar. | Key Vault Premium provisionieren; jährliche Cert-Rotation mit 7-Tage-Übergangsfenster. | vor MVP2 |
| OF-G-04 | RSC-Auto-Install in „alle" Pilot-Teams – politisch tragfähig oder opt-in? | 05 §14.6, 11 §8.5 | Bereichsleitung, BR | MVP2-Rollout-Aufwand unkalkulierbar. | Opt-in pro Team durch Team-Owner; Auto-Install nur in dedizierter Pilot-Gruppe. | vor MVP2 |
| OF-G-05 | Streaming-API für Bot-Antworten – Tenant-Verfügbarkeit, Fallback-UX? | 05 §14.2 | Backend-Lead, UX | Bot-UX inkonsistent über Tenants. | Feature-Flag, Fallback auf zwei-stufige Antwort. | vor MVP2 |
| OF-G-06 | Meeting-Side-Panel (in-Meeting App) – im Scope? | 05 §14.3 | Produktmanagement | UX in Meetings limitiert. | Out of Scope MVP1–3; ggf. MVP4. | vor MVP4 |
| OF-G-07 | Sensitivity-Labels-Mapping (MIP → Pipeline-Behandlung) – verbindliches Mapping? | 02 §12.5, 07 §14.6, 11 §8.9 | DSB, M365-Admin | Inkonsistente Behandlung sensibler Inhalte. | Mapping-Tabelle in Setup-Page; Default `Highly Confidential` ⇒ kein AOAI-Aufruf. | vor MVP1 |
| OF-G-08 | Newsletter-/Bulk-Mail-Heuristik – akzeptable False-Negative-Rate, Whitelist? | 07 §14.5, 11 §8.10 | Bereichsleitung, DSB | Spam überflutet Timeline. | Standardheuristik (List-Unsubscribe, Precedence: bulk) + tenant-spezifische Allowlist. | vor MVP1 |
| OF-G-09 | Backfill-Tiefe und -Kosten – welcher Initial-Zeitraum, Pay-per-use-Freigabe? | 07 §14.4, 11 §8.12 | Bereichsleitung, IT-Einkauf | Kostenexplosion, falsche Pilot-Erwartung. | **entschieden → siehe A19** (24 Monate Pflicht-Backfill pro Pilotpostfach, gedrosselt, Kosten-Cap, Lücken-Monitor). | ~~vor MVP1~~ erledigt |
| OF-G-10 | Outlook „Neues Outlook" (Monarch) – Add-in-Kompatibilität validiert? | 04 §15.7 | Backend-Lead, Pilot-IT | Pilot-User mit neuem Outlook ausgeschlossen. | Test-Matrix Web/Desktop/Monarch in Sprint 1; Smoke-Tests Pflicht. | vor MVP1 |

### 2.6 Business-Central-Konfiguration

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-B-01 | AppSource-Object-ID-Range bei Microsoft beantragt? | 02 §12.1 | BC-Lead, Produktmanagement | AppSource-Veröffentlichung blockiert. | Pilot-Range 50000–50099; AppSource-Range vor MVP4 beantragen. | vor MVP4 |
| OF-B-02 | Owner Team Code – Bindung an BC-User-Groups oder Entra-Groups (per Sync)? | 02 §12.2 | BC-Lead, M365-Admin | Visibility-Filter nicht implementierbar. | BC-User-Groups primär; Entra-Group-Sync optional als Erweiterung. | vor MVP1 |
| OF-B-03 | BC Task Integration: Standard-Tabelle 5080 „Interaction" als Ziel oder eigenständig? | 02 §12.3 | BC-Lead, Bereichsleitung | Action-Item-Workflow inkonsistent. | Eigenständige Tabelle 50007; Verlinkung optional auf Standard. | vor MVP1 |
| OF-B-04 | Right-to-be-forgotten-Kaskade BC → Search → Blob → SharePoint – orchestriertes Lösch-Skript? | 02 §12.7, 09 §8.4, 12 §18.7 | BC-Lead, Cloud-Architekt, DSB | DSGVO-Lösch-SLA verfehlbar. | Backend-Lösch-Workflow mit Status-Tracking und Audit. | vor MVP3 |
| OF-B-05 | Permission Set für Setup-Mutation – wer darf `PATCH /setup`? | 03 §13.4 | BC-Lead, Security | Setup unsicher konfigurierbar. | Separate Service-Identität `IOI_COMMHUB_ADMIN` mit Just-in-Time-Aktivierung. | vor MVP1 |
| OF-B-06 | `x-ccc-acting-user-oid`-Header – verbindlich oder optional, wie verifiziert? | 03 §13.5, 12 §6.2 | BC-Lead, Security | Audit-Eindeutigkeit der Endbenutzer-Aktion fraglich. | Verbindlich; Backend führt OBO-Vorab-Check, BC vertraut Backend (S2S-Identität). | vor MVP1 |
| OF-B-07 | Long-Reply / Long-Summary Upload – Signed URL aus BC oder Backend → Blob? | 03 §13.8 | BC-Lead, Backend-Lead | Speicherpfad unklar. | Backend lädt direkt in Blob via Managed Identity, BC bekommt nur `Long Blob Id`. | vor MVP1 |
| OF-B-08 | Versionierungspfad: was gilt als Breaking Change in `v1.0` → `v1.1`? | 03 §13.10 | Backend-Lead, BC-Lead | Konsumenten brechen unvermittelt. | SemVer-Konvention dokumentiert: Pflichtfeld-Hinzufügung = Major. | vor MVP1 |

### 2.7 AI / Modelle / Kosten

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-AI-01 | Modellverfügbarkeit GPT-4.1 in Sweden Central zum Implementierungszeitpunkt – GA und Kontextlänge? | 08 O1 | Cloud-Architekt, Backend-Lead | Antwortvorschläge / Briefings in Qualität gefährdet. | Verfügbarkeit Anfang Sprint 1 prüfen; Fallback GPT-4o. | sofort |
| OF-AI-02 | Kostenexplosion bei Voll-Rollout – Briefing-Capabilities skalieren teuer; Caching/Tiering verbindlich? | 08 O4 | Bereichsleitung, IT-Einkauf | Budget-Überschreitung. | ModelRouter (4o-mini default), Briefing-Cache TTL 24 h, Tenant-Kosten-Alerts mit Hard-Throttle bei 120 %. | vor MVP1 |
| OF-AI-03 | Mehrsprachige Qualität (FR/IT/ES) – Goldset-Erweiterung erforderlich? | 08 O5 | Produktmanagement, QA | Pilot-Erwartungen verfehlt. | DE/EN als Default, FR/IT/ES best effort, Goldset-Erweiterung erst bei konkreter Pilot-Anforderung. | vor MVP4 |
| OF-AI-04 | Datenresidenz Embeddings-Cache – tenantgetrennte Search-Indizes erforderlich? | 08 O6 | DSB, Cloud-Architekt | Cross-Tenant-Cache-Verletzung. | Cache pro Tenant verbindlich; eigener Search-Index `cache:<tenant>`. | vor MVP1 |
| OF-AI-05 | Feedback-Speicherung (Edit-Diffs) – DSGVO-Bewertung pro Tenant, opt-in? | 08 O7 | DSB, Bereichsleitung | Lerndaten nicht nutzbar. | Opt-in pro Tenant, anonymisiert, kein Klartext-Diff. | vor MVP2 |
| OF-AI-06 | Umgang mit verschlüsselten / IRM-Mails – nur Metadaten-Fallback? | 08 O9 | DSB, Bereichsleitung | Anwender erwarten Inhalts-Verarbeitung. | Fallback auf Metadaten + Hinweis im UI; keine Decryption. | laufend |
| OF-AI-07 | Latenz Briefings (C4c/C4e) – nahe SLO-Grenze, asynchrones UI-Pattern Pflicht? | 08 O10 | UX-Lead, Backend-Lead | UX hakt bei langen Briefings. | Async-Pattern (202 Accepted + Polling/Webhook) ab > 8 s. | vor MVP3 |
| OF-AI-08 | Restrisiko Prompt Injection trotz Tool-Allowlist und Sanitizer | 08 O3, 12 §18.11 | Security, DSB | Reputationsrisiko bei Falschvorschlag. | Tiefenverteidigung gem. [08 §6](08-ai-orchestration.md), HITL Pflicht, UI-Hinweis. | laufend |
| OF-AI-09 | Halluzinationen mit Geschäftsfolgen – Schwellen, UI-Hervorhebung? | 08 O8, 12 §18.11 | Bereichsleitung, UX | Falscher Vorschlag wird übernommen. | Confidence < 0,6 visuell hervorheben; Pflicht-Quellen-Chip; Schulung. | vor MVP1 |
| OF-AI-10 | Eval-Tooling – Promptfoo vs. Azure AI Evaluation SDK vs. Eigenbau? | 16 §14.7 | QA-Lead, Backend-Lead | Eval-Pipeline nicht reproduzierbar. | Default eigener .NET-Runner + Promptfoo für Adversarial-Suite. | vor MVP1 |
| OF-AI-11 | LLM-as-Judge-Modell – anderes Provider-Modell zur Reduktion korrelierter Fehler? | 16 §14.8 | QA-Lead, DSB | Faithfulness-Eval verzerrt. | Sekundäres OpenAI-Modell anderer Familie (z. B. GPT-4o für Generation, GPT-4.1 für Judge); externe Provider nur bei EU-Konformität. | vor MVP2 |

### 2.8 Pilotbetrieb

| ID | Frage | Quelle | Adressat | Auswirkung wenn unbeantwortet | Empfohlene Antwort/Annahme | Frist |
|----|-------|--------|----------|-------------------------------|----------------------------|-------|
| OF-P-01 | Goldlabel-Erstellung – wer erstellt und pflegt (intern vs. extern, ca. 80–120 h)? | 16 §14.1 | QA-Lead, Bereichsleitung | Eval-Set fehlt, Release-Gate offen. | Interne Fachuser, 1 Annotation-Tag pro Sprint; externer Dienstleister bei Engpass. | vor MVP1 |
| OF-P-02 | Pay-per-use-Budget Teams Export API in Test-Stage – Obergrenze? | 16 §14.2 | IT-Einkauf, QA | Test-Sub kann unkontrolliert kosten. | 200–500 EUR/Monat mit Hard-Cap und Alarm. | vor MVP2 |
| OF-P-03 | Pilotgruppen-Größe – 8 oder 15 Personen, Bereitschaft zur Telemetrie-Teilnahme? | 16 §14.9 | Bereichsleitung, BR | Statistische Aussagekraft Pilot fragwürdig. | **entschieden → siehe A17** (Opt-in-Pilotgruppe ≤ 50 MA, Telemetrie opt-in). | ~~sofort~~ erledigt |
| OF-P-04 | Anonymisierungstiefe Pilot-BC-Sandbox – k-Anonymität k=5 oder strenger? | 16 §14.10 | DSB | Echtdaten-Pilot blockiert. | k=5, Streuung Beträge ±20 %, Datums-Δ ≤ 30 Tage. | vor MVP1 |
| OF-P-05 | Externer Pen-Test-Provider und Zeitfenster vor MVP4-GA? | 16 §14.6 | Security, IT-Einkauf | Sicherheitsfreigabe verspätet. | Pen-Test-Slot 4 Wochen vor MVP4-Release buchen. | vor MVP4 |
| OF-P-06 | DLQ-Operationsmodell – wer betreibt Re-Drive, SLA für DLQ-Sichtung? | 07 §14.8 | Operations-Team | Datenlücken in BC. | Operations-Team mit Tool für Re-Drive, SLA 1 Werktag. | vor MVP1 |
| OF-P-07 | Notifications: Standard-Kanal pro Benutzer (BC My Notifications, Teams, E-Mail)? | 07 §14.7 | Bereichsleitung, UX | Pilot-Kommunikation unklar. | BC My Notifications als Default, Teams sekundär. | vor MVP1 |
| OF-P-08 | BC-API-Last – kann Custom API 50 POST/s im Burst tragen oder Outbox-Drosselung? | 07 §14.10 | BC-Lead, Operations | Throttling beim Burst-Backfill. | Outbox-Drosselung im Backend, max. 20 POST/s pro Tenant. | vor MVP1 |
| OF-P-09 | Reaktion auf gelöschte Teams-Nachrichten – wie verhalten sich BC-Interactions? | 05 §14.10 | UX, BC-Lead | UX-Inkonsistenz. | Hinweis-Banner im Tab; Löschen weiterhin nur durch Benutzer in BC. | vor MVP2 |
| OF-P-10 | Manifest-Lokalisierung Outlook/Teams – Anzeigename mehrsprachig? | 04 §15.10 | UX, Marketing | UX inkonsistent. | DE primär, EN als `<Override>`. | vor MVP1 |

**Zusammenfassung:** 51 offene Fragen über 8 Kategorien:

| Kategorie | Anzahl |
|-----------|-------:|
| Strategie & Scope | 5 |
| Lizenzen & Verträge | 6 |
| Datenschutz / Compliance / Mitbestimmung | 8 |
| Architektur & Cloud | 10 |
| Microsoft Graph & Teams | 10 |
| BC-Konfiguration | 8 |
| AI / Modelle / Kosten | 11 |
| Pilotbetrieb | 10 |
| **Summe** | **51** |

(Hinweis: Einzelne Fragen tauchen in mehreren Quellen auf; oben jeweils der primäre Adressat.)

---

## 3. Empfohlene nächste Schritte

### 3.1 Sofort (≤ 2 Wochen)

| # | Aktion | Verantwortlich | Liefergegenstand |
|---|--------|----------------|------------------|
| N-01 | **DSFA-Start** mit Bausteinen aus [12 §8.4](12-security-compliance.md). | DSB | DSFA-Entwurf v0.1 |
| N-01a | **Consent-Formular & Consent-Register** (DE/EN) gemäß A16/ADR-27 – Formularvorlage, BC-Tabelle `Communication Consent` (50014), Widerruf-Prozess. | DSB + HR + BC-Lead | Formular v1.0, Register-Schema, Widerruf-Workflow |
| N-01b | **Information Mitbestimmungsgremien (BR / Sprecherausschuss / MAV)** vor Pilotstart gemäß A17/ADR-28 – Vorstellung Pilot-Scope, Schutzbedingungen, Re-Consent-Verfahren. | HR + DSB + Sponsor | Informationsschreiben, Sitzungsprotokoll |
| N-02 | **Betriebsvereinbarung-Entwurf** ([12 §10.1](12-security-compliance.md)). | HR + Personalvertretung + DSB | BV-Entwurf v0.1 |
| N-03 | **Pilotgruppe definieren** (Bereich, 8–15 Postfächer, Mail-Sicherheitsgruppe in Exchange). | Bereichsleitung + M365-Admin | Pilot-Charta + AAD-Group `pilot-comm-copilot` |
| N-04 | **Lizenz-Inventur** M365 E5, Teams Premium, M365 Copilot pro Pilotorganisator. | IT-Einkauf + M365-Admin | Lizenz-Report |
| N-05 | **Azure-Subscription-Provisioning** für `dev`, `test`, `pilot` (Sweden Central). | Cloud-Architekt | IaC-Repo (Bicep/Terraform) mit RGs/KV/AppI je Stage |
| N-06 | **Service Principals & App-Registrierungen** für Backend, Ingestion, BC-S2S. | M365-Admin + Cloud-Architekt | App-Registrierungen mit Zertifikaten in Key Vault |
| N-07 | **Test-Mailbox/Tenant** einrichten (separater M365-Test-Tenant + 5 Test-Postfächer + 2 Test-Teams). | M365-Admin | Test-Tenant-Setup |
| N-08 | Klärung **OF-AI-01** (GPT-4.1-Verfügbarkeit Sweden Central). | Cloud-Architekt | Bestätigung oder Fallback-Plan |
| N-09 | **Application Access Policy** (Exchange) auf Pilotgruppe einrichten. | Exchange-Admin | Policy aktiv + Health-Check-Skript |

### 3.2 Kurzfristig (≤ 4 Wochen)

| # | Aktion | Verantwortlich | Liefergegenstand |
|---|--------|----------------|------------------|
| N-10 | **Architektur-Review** mit Stakeholdern gegen [01](01-architecture.md), [12](12-security-compliance.md). | Cloud-Architekt | Review-Protokoll, ADR-Aktualisierungen |
| N-11 | **Backend-Skeleton** (Copilot API + Ingestion Functions + Service Bus + Cosmos). | Backend-Lead | Repo `comm-copilot-backend` mit CI/CD |
| N-12 | **BC Setup-Page** und Permission Sets als AL-Skelett ([02 §3.11, §6](02-bc-data-model.md)). | BC-Lead | AL-App `IOI_CommHub` v0.1 |
| N-13 | **Ingestion-PoC** mit einer Pilot-Mailbox (Mail-Subscription, Webhook, Stage 1–4). | Backend-Lead | PoC-Demo |
| N-14 | **Eval-Datensatz aufbauen** (50 synthetische Mails mit Goldlabels, [16 §3](16-testing-acceptance.md)). | QA-Lead | Repo `comm-copilot-eval` v0.1 |
| N-15 | Klärung **OF-G-01, OF-G-09, OF-G-07** (App Access Policy, Backfill-Tiefe, Sensitivity-Labels). | M365-Admin + DSB | Setup-Doku + Mapping-Tabelle |
| N-16 | **DSGVO-Hinweis** (Mail-Signatur, Webseite). | DSB + Marketing | Disclaimer-Texte |

### 3.3 Mittelfristig (4–10 Wochen) – MVP1-Implementierung

Umsetzung gemäß `13-mvp-roadmap.md` (sofern vorhanden) bzw. der MVP-1-Definition aus `instructions.md`:

- Outlook Add-in Side Panel ([04](04-outlook-addin.md)).
- Serverseitige Mail-Ingestion ([07](07-ingestion-pipeline.md), Stages 1–15).
- Match-Logik (deterministisch + LR-Reranker, [10](10-matching.md)).
- BC-Datenmodell + Custom APIs + Pages ([02](02-bc-data-model.md), [03](03-bc-apis.md)).
- AI-Capabilities C1, C2, C3, C4a, C5, C6, C7 ([08](08-ai-orchestration.md)).
- Audit-Log, Telemetrie ([12 §13](12-security-compliance.md)).
- Test-Suite gemäß [16](16-testing-acceptance.md) (Unit, Integration, AI-Eval, Security).

Wöchentliches Steering, Eval-Reviews je Sprint (siehe §5).

### 3.4 Anschließend – MVP2 Teams-Pilot

Nach erfolgreichem MVP1 und mit erfüllten Vorbedingungen (OF-L-01, OF-L-03, OF-G-03, OF-G-04, OF-AI-05):

- Teams App (Bot, ME, Tab) mit RSC-First ([05](05-teams-app.md)).
- Teams-Ingestion via Encrypted Content Subscriptions ([07 §3, §13](07-ingestion-pipeline.md)).
- Erweiterung Eval-Set um Teams-Beispiele.

---

## 4. Stakeholder-Map

| Rolle | Verantwortung | Erwartete Inputs | Erwartete Freigaben |
|-------|---------------|------------------|---------------------|
| **Geschäftsführung / Sponsor** | Strategische Entscheidungen, Budgetfreigabe, Pilot-Sponsorship | Lieferziele, Roadmap, Risiken, KPIs | Go/No-Go MVP1–4, Budgetfreigabe, ISV-Entscheidung |
| **Bereichsleitung Vertrieb / Service / PM** | Pilot-Bereich, Pilot-Anwender, Akzeptanz-Definition | Pilot-Personen, Use Cases, Akzeptanzkriterien | Pilot-Charta, Akzeptanz-Bericht |
| **Datenschutzbeauftragter (DSB)** | DSFA, Verarbeitungsverzeichnis, Betroffenenrechte | Verarbeitungszwecke, Datenkategorien, Empfänger | DSFA-Freigabe, Lösch-Workflow, AOAI-Freigabe |
| **Personalvertretung / Betriebsrat** | Mitbestimmung gem. § 87 BetrVG | Pilot-Scope, Telemetrie-Konzept, Schulungsplan | Betriebsvereinbarung |
| **Recht / Compliance** | AVV, DPA, ISV-Verträge | Microsoft-DPA, Subprozessor-Liste | AVV-Freigaben |
| **IT-Einkauf** | Lizenzbeschaffung, Azure-EA | Lizenz-Inventur, Kostenrahmen | Lizenz-Bestellungen, EA-Commitment |
| **M365-Admin (Exchange + Teams)** | App-Registrierungen, Application Access Policy, RSC | Pilot-Sicherheitsgruppe, Setup-Policies | Admin Consent, Policies aktiv |
| **Cloud-Architekt** | Azure-Topologie, Netzwerk, IaC | Architektur gem. [01](01-architecture.md) | Architektur-Review-Freigabe |
| **Security / SOC** | Bedrohungsmodell, Pen-Test, SIEM | STRIDE-Analyse [12 §1.2](12-security-compliance.md), Audit-Konzept | Sicherheits-Freigabe vor MVP-GA |
| **Backend-Lead** | Copilot API, Ingestion, AI-Orchestrator | Specs aus [01](01-architecture.md), [07](07-ingestion-pipeline.md), [08](08-ai-orchestration.md) | Release-Tag pro Sprint |
| **BC-Lead (AL)** | BC-Extension, Custom APIs, Permission Sets | Datenmodell [02](02-bc-data-model.md), API-Spec [03](03-bc-apis.md) | AL-App-Release |
| **UX-Lead** | Outlook-/Teams-/BC-UX, Lokalisierung, A11y | Personas, UX-Reviews | UX-Approval pro Surface |
| **QA-Lead / Quinn-Tester** | Test-Strategie, Eval-Pipeline, Goldlabels | [16](16-testing-acceptance.md) | Release-Gate-Freigaben |
| **Operations-Team** | Run, Monitoring, DLQ-Handling, Subscription-Renewal | KPI-Dashboards [01 §10](01-architecture.md) | Betriebshandbuch, On-Call |
| **Pilot-Anwender** | Realnutzung, Feedback, Korrekturen | UX-Feedback, Akzeptanzrate | Pilot-Akzeptanz-Bericht |

---

## 5. Governance & Review-Rhythmus

| Forum | Frequenz | Teilnehmer | Inhalt |
|-------|---------:|------------|--------|
| **Steering Committee** | wöchentlich (45 min) | Sponsor, Bereichsleitung, DSB, Backend-Lead, BC-Lead, QA-Lead, Cloud-Architekt | Fortschritt vs. Plan, Risiken, Entscheidungen, Budget, offene Fragen aus §2 |
| **Architektur-Review** | alle 2 Wochen oder bei ADR-Änderung | Cloud-Architekt, Backend-/BC-Lead, Security | ADR-Updates, Architektur-Drift, [01](01-architecture.md) |
| **Eval-Review (AI-Qualität)** | wöchentlich nach jedem Eval-Run | QA-Lead, Backend-Lead, Bereichsleitung | Faithfulness, Citation Coverage, Halluzinationen, Drift gem. [08 §10](08-ai-orchestration.md), [16 §6](16-testing-acceptance.md) |
| **Risiko-Review** | alle 2 Wochen | Sponsor, DSB, Security, Operations | Risiko-Register aus [14](14-risks-decisions.md) (sofern vorhanden), Pen-Test-Findings, Cost-Alerts |
| **DSGVO-/BR-Sync** | monatlich | DSB, BR, HR, Bereichsleitung | DSFA-Status, BV-Status, Pilot-Erfahrungen |
| **Pilot-Retrospektive** | alle 2 Wochen | Pilot-Anwender, UX-Lead, Backend-Lead | UX-Feedback, Akzeptanz-Metriken, Korrektur-Backlog |

Entscheidungs-Tracking: alle wesentlichen Entscheidungen werden als ADR in `docs/plan/` ergänzt (Format ADR-light gem. [01 §12](01-architecture.md)) und im Risiko-/Entscheidungs-Dokument [14](14-risks-decisions.md) referenziert.

---

## 6. Kommunikations- / Informationskonzept für betroffene Mitarbeiter

> Ziel: Transparenz über Zweck, Umfang, Rechte, Grenzen. **Keine Mitarbeiterüberwachung**, klare Zweckbindung gem. [12 §10.1](12-security-compliance.md).

### 6.1 Information vor Pilotstart (Pflicht)

- **All-Hands-Information** (Townhall + Intranet-Artikel + E-Mail): Zweck, Funktionsumfang, Was wird erfasst (extern), Was nicht (intern, privat), Rechte, Eskalationsweg.
- **Datenschutzhinweis** (gem. Art. 13 DSGVO) – schriftlich an alle Mitarbeiter mit Pilot-Beteiligung; auch öffentlich auf Intranet.
- **FAQ-Dokument** (PDF + Wiki) – z. B.:
  - „Werden meine privaten Mails verarbeitet?"
  - „Sieht mein Vorgesetzter, wie schnell ich antworte?"
  - „Was passiert bei einer falschen Zuordnung?"
  - „Wie kann ich Inhalte löschen lassen?"
- **Sprechstunde DSB** (z. B. wöchentlich 30 min) während des Pilots.

### 6.2 Onboarding und Schulung Pilot-Anwender

- **Schulung 60–90 min** pro Pilot-Anwender: Funktionen Outlook Add-in, Korrektur-Flows, Quellen-Chips, Grenzen (kein Auto-Send), Datenschutz, Eskalation.
- **Reference Card** (1 Seite) als Schnellreferenz.
- **Hands-On-Sandbox-Zugang** im Pilot-BC mit synthetischen Daten vor Echtnutzung.
- **Buddy-System**: ein erfahrener Pilot-Anwender pro 3 neue Anwender.

### 6.3 Laufende Information

- **Monatlicher Newsletter** „Copilot-Update" mit Funktionsänderungen, Eval-Ergebnissen (aggregiert), Roadmap, FAQ-Erweiterungen.
- **Feedback-Kanal**: dediziertes Postfach `comm-copilot-feedback@<unternehmen>` und Teams-Kanal; jedes Feedback wird im Pilot-Backlog gespiegelt.
- **Transparenz-Bericht** je MVP-Stufe: Anzahl erfasster Mails (aggregiert), Akzeptanzrate, Korrekturrate, DSGVO-Vorfälle (Anzahl, Art, Reaktion).

### 6.4 Information externer Personen

- **Standard-Disclaimer** in Mail-Signatur und Webseite (Datenschutzhinweis gem. Art. 14 DSGVO): „Ihre Kommunikation wird zur Bearbeitung in unserem Kunden-Kommunikationssystem gespeichert. Details und Ihre Rechte: [Link]."
- **Datenschutzhinweis** auf der Unternehmens-Webseite mit Auskunfts-/Lösch-Antragsformular.

---

## 7. Go/No-Go-Kriterien vor MVP1

**Go ist nur möglich, wenn alle folgenden Kriterien erfüllt sind.** No-Go bei jedem Verstoß.

### 7.1 Rechtlich / Compliance

| # | Kriterium | Quelle | Status-Quelle |
|---|-----------|--------|---------------|
| GG-01 | DSFA abgeschlossen, Risiken auf Restrisiko-Niveau gemäß DSB. | [12 §8.4](12-security-compliance.md), OF-D-01 | DSFA-Dokument vom DSB unterzeichnet |
| GG-02 | Betriebsvereinbarung mit Personalvertretung unterzeichnet (in DE/AT). | [12 §10.1](12-security-compliance.md), OF-D-02 | BV-Dokument |
| GG-03 | Pilot-Anwender informiert (Datenschutzhinweis), externes Disclaimer aktiv. | OF-D-03 | Verteiler-Bestätigung, Webseite-Live |
| GG-04 | Auftragsverarbeitungsvertrag (Microsoft, Subprozessoren) bestätigt. | [12 §8.6](12-security-compliance.md), OF-L-03 | AVV-Bestätigungen |
| GG-05 | Azure-OpenAI „No Log Retention" / „No Training" bestätigt. | [08 §3.2](08-ai-orchestration.md), OF-L-04 | Microsoft-Bestätigung |

### 7.2 Technisch

| # | Kriterium | Quelle | Status-Quelle |
|---|-----------|--------|---------------|
| GG-06 | Application Access Policy (Exchange) auf Pilotgruppe aktiv und durch Health-Check verifiziert. | [11 §2.4](11-graph-feasibility.md), OF-G-01 | Health-Check-Report |
| GG-07 | E2E-Pfad „Mail → BC-Timeline" grün auf Pilot-Stage (siehe [16 §5.1](16-testing-acceptance.md)). | [16 §5.1](16-testing-acceptance.md) | CI-Report |
| GG-08 | AI-Eval-Gate erfüllt: Faithfulness ≥ 0,95, Citation Coverage ≥ 0,95, Hallucination Rate ≤ 0,02 auf Goldset. | [16 §6.1](16-testing-acceptance.md), [08 §10.2](08-ai-orchestration.md) | Eval-Report |
| GG-09 | Security-Tests: Prompt-Injection-Suite 100 % bestanden, Berechtigungs-Negativtests 100 % grün. | [16 §7.1, §7.2](16-testing-acceptance.md) | Test-Report |
| GG-10 | Audit-Log liefert revisionssichere Einträge (immutable, Hash-Kette validiert). | [12 §13](12-security-compliance.md), [16 §7.5](16-testing-acceptance.md) | Audit-Test-Report |
| GG-11 | Performance-Budget eingehalten: TTFI Outlook ≤ 3 s p95, Ingestion E2E ≤ 60 s p95. | [04 §7.1](04-outlook-addin.md), [01 §10](01-architecture.md) | Last-Test-Report |
| GG-12 | Right-to-be-forgotten-Workflow funktional (Test gem. [16 §7.3](16-testing-acceptance.md)). | OF-D-05, OF-B-04 | DSGVO-Test-Report |

### 7.3 Operativ

| # | Kriterium | Quelle | Status-Quelle |
|---|-----------|--------|---------------|
| GG-13 | Pilotgruppe definiert, geschult, Buddy-System steht. | §6.2, OF-S-03, OF-P-03 | Schulungs-Bestätigung |
| GG-14 | Operations-Team einsatzfähig (DLQ-Tooling, On-Call, Subscription-Renewal-Monitor). | OF-P-06 | Betriebshandbuch v1.0 |
| GG-15 | Cost-Alerts (Tenant-Budget, AOAI-Quota) aktiv mit Hard-Throttle bei 120 %. | OF-AI-02, [08 §15.3](08-ai-orchestration.md) | Alerting-Konfiguration |
| GG-16 | Rollback-Plan dokumentiert und getestet (Modell-/Template-Rollback per Config-Flag). | [08 §15.1](08-ai-orchestration.md) | Rollback-Drill-Report |

**No-Go-Konsequenz:** Pilot wird verschoben, betroffene Kriterien werden im nächsten Steering priorisiert. Kein „weicher" Go.

---

## 8. Abschluss

Dieses Dokument ist **lebend**: Offene Fragen werden in §2 fortgeschrieben; Antworten werden mit Datum und Entscheidungsträger ergänzt und in [14-risks-decisions.md](14-risks-decisions.md) als ADR übernommen, sofern strukturell. Die finale Verifikation der Plan-Abdeckung gegen die Akzeptanzkriterien aus `instructions.md` erfolgt in [17-traceability.md](17-traceability.md).
