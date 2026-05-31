# 11 – Microsoft Graph Feasibility

> Technische Bewertung, welche der in `instructions.md` (Abschnitte „Microsoft Graph Anforderungen" und „Nacharbeit / Erweiterung") gewünschten Daten über Microsoft Graph in welcher Form, mit welcher Berechtigung und unter welchen Lizenz-/Kostenbedingungen tatsächlich serverseitig zugänglich sind. Grundlage ist der zum Planungszeitpunkt dokumentierte Stand der Microsoft-Graph-API (v1.0 / beta) sowie der Microsoft Teams Export API.

## 1. Ziel

Klar dokumentieren:

- **Was** über Microsoft Graph mit **Application Permissions** (unbeaufsichtigter Server-Dienst) erfassbar ist.
- **In welcher Form** (REST-Pull, Delta, Change Notifications/Webhooks, Encrypted Content).
- **Unter welchen Bedingungen** (Lizenz, Pay-per-use, Resource-Specific Consent, Tenant-Policies).
- **Wo Microsoft Graph die Anforderung nicht oder nur eingeschränkt erfüllt.**
- Daraus abgeleitet: gestaffelte Empfehlung pro MVP.

Diese Bewertung ist Eingabe für [07-ingestion-pipeline.md](07-ingestion-pipeline.md), [12-security-compliance.md](12-security-compliance.md) und [13-mvp-roadmap.md](13-mvp-roadmap.md).

---

## 2. E-Mail (Exchange Online via Graph)

### 2.1 Lesezugriff auf Nachrichten und Konversationen

| Bedarf (aus instructions.md) | Graph-Endpoint | Permission (Application) | Verfügbarkeit |
|---|---|---|---|
| Nachrichten aller berechtigten Postfächer lesen | `GET /users/{id}/messages` | `Mail.Read` | stabil v1.0 |
| Einzelnachricht inkl. Body, Header | `GET /users/{id}/messages/{id}` | `Mail.Read` | stabil |
| Konversation/Thread | `conversationId` Property + Filter `?$filter=conversationId eq '...'` | `Mail.Read` | stabil |
| Internet Message ID (RFC 822) | `internetMessageId` Property + `internetMessageHeaders` (`?$select=internetMessageHeaders`) | `Mail.Read` | stabil |
| Anhänge | `GET /users/{id}/messages/{id}/attachments` (`fileAttachment`, `itemAttachment`, `referenceAttachment`) | `Mail.Read` | stabil |
| MIME des Originals | `GET /users/{id}/messages/{id}/$value` (text/plain MIME) | `Mail.Read` | stabil |
| Mailbox-übergreifend (alle Postfächer) | `/users/{id}/...` mit Application Permission | `Mail.Read` | stabil |

**Wichtig:** `Mail.Read` mit Application Permission gewährt standardmäßig Zugriff auf **alle Postfächer im Tenant**. Eine Einschränkung erfolgt über die **Application Access Policy für Exchange Online** (siehe 2.4).

### 2.2 Change Notifications (Subscriptions / Webhooks)

| Aspekt | Status |
|---|---|
| Resource | `users/{id}/messages` und `users/{id}/mailFolders('Inbox')/messages` werden unterstützt |
| Ressourcen-Wildcard `users/{id}/messages` ohne konkrete User-ID | **wird NICHT unterstützt** – pro Mailbox eine Subscription nötig |
| Maximale Lebensdauer | ca. **3 Tage (4230 min)** für Mail – muss vor Ablauf erneuert werden |
| Renewal | `PATCH /subscriptions/{id}` mit neuem `expirationDateTime` |
| Lifecycle Notifications | Pflicht-Feld `lifecycleNotificationUrl` empfohlen; liefert `subscriptionRemoved`, `reauthorizationRequired`, `missed` |
| Encrypted Content | für Mail **optional** (anders als Teams), aber empfehlenswert für sensible Inhalte – benötigt Zertifikat in Key Vault |
| Validierung | `clientState` und `validationToken`-Handshake bei Subscription-Erstellung |

**Konsequenz für Skalierung:** Bei n Mailboxen → n Subscriptions → n Renewals/3 Tage. Subscription-Manager muss als Cron-Job (siehe [07-ingestion-pipeline.md](07-ingestion-pipeline.md) §9) laufen.

### 2.3 Delta Queries als Fallback

- `GET /users/{id}/mailFolders/{id}/messages/delta` liefert inkrementelle Änderungen seit letztem `deltaToken`.
- Geeignet als **Backfill** und als **Fallback**, wenn Webhook-Notifications verloren gehen (`missed`-Lifecycle).
- Token-Lebensdauer ist begrenzt (typisch ~30 Tage Inaktivität); dann Full-Sync nötig.
- Throttling-Limits beachten (siehe §6).

### 2.4 Application Access Policy (Exchange) – Beschränkung auf definierte Postfächer

- Mechanismus: Exchange Online PowerShell `New-ApplicationAccessPolicy` schränkt Application Permission `Mail.Read` auf eine **Mail-aktivierte Sicherheitsgruppe** ein.
- Damit lässt sich der „alle-Postfächer"-Default (siehe 2.1) auf Pilot-/Pflichtgruppe reduzieren.
- **Risiko:** Policy ist Exchange-Admin-Setting, nicht in Graph-API sichtbar; Änderungen können den Service still abschneiden – Monitoring nötig.
- **Compliance-Vorteil:** essenziell für DSGVO und Betriebsvereinbarung (siehe [12-security-compliance.md](12-security-compliance.md)).

### 2.5 Was über Mail-Graph **nicht** verfügbar ist

- Keine zuverlässige Klassifikation „privat" durch Graph selbst – muss über Heuristik + Sensitivity Labels (`singleValueExtendedProperties` / Information Protection Labels) erfolgen.
- Senderseitige Verschlüsselung (S/MIME, OME) liefert ggf. nur verschlüsseltes Body – Volltextanalyse dann nicht möglich.

---

## 3. Microsoft Teams – kritische Bewertung (Hauptaugenmerk)

Die Teams-Erfassung ist der **risikoreichste Teil** der Lösung. Microsoft trennt Teams-Nachrichtenzugriff seit 2022 in **lizenzpflichtige (Modell A)** und **nutzungsbasierte (Modell B)** Modelle. Application-Permission-Zugriff auf Chat/Channel-Nachrichten ist **nicht „kostenlos"**.

### 3.1 1:1- und Group-Chats

| Aspekt | Detail |
|---|---|
| Endpoint | `GET /chats`, `GET /chats/{id}/messages`, `GET /users/{id}/chats` |
| Permission (Application) | `Chat.Read.All` |
| Lizenzmodell | Microsoft 365 E5 (Modell A) **oder** Pay-per-use (Modell B) per Teams Export API Billing |
| Modell A | Capacity über E5-Lizenzen pro „seeded user" – seeded user = aufrufender Service-Principal-Kontext nicht direkt; in der Praxis: E5 für **Sender** der erfassten Nachrichten erforderlich, sonst kostenpflichtig |
| Modell B | `model=B` Query-Parameter setzt Pay-per-use, abgerechnet pro abgerufener Nachricht |
| Resource-Specific Consent (RSC) | für Chats **nicht in der gleichen Form** wie für Channels verfügbar; `Chat.Manage.Chat` (RSC) deckt nur installierte App in Chat ab |

**Lizenzkosten pro Nachricht (Pay-per-use, Stand Planungszeitpunkt, listenpreisbasiert, exkl. Rabatte):**
- Indikativ ca. **0,00075 USD pro Nachricht** (Quelle: Microsoft Teams Export API Pricing; Preise volatil – **vor Vertragsabschluss verifizieren**).
- Bei 1 Mio. Nachrichten/Monat ergäben sich rund **750 USD/Monat** – siehe Modellrechnung §6.1.

### 3.2 Channel Messages

| Variante | Permission | Lizenzkosten | RSC | Skalierung |
|---|---|---|---|---|
| Tenant-weit über Application | `ChannelMessage.Read.All` | Pay-per-use (Modell B) **oder** E5-Capacity | nein (Tenant-Scope) | einfach, aber teuer |
| Pro Team installiert | RSC `ChannelMessage.Read.Group` | **kein** Pay-per-use | ja | App muss in **jedem** Team installiert werden |

**Konsequenz:** RSC umgeht die Lizenzkosten, ist aber **nicht** automatisch über alle Teams ausgerollt. Es erfordert Team-Eigentümer-Zustimmung pro Team oder Admin-Skript via Graph (`POST /teams/{id}/installedApps`). Damit ist „alle externen Channelnachrichten erfassen" **organisationspolitisch und technisch nicht trivial skalierbar**.

### 3.3 Meeting Chats

- Meeting-Chats sind technisch **Chats mit `chatType = meeting`** (`GET /chats?$filter=chatType eq 'meeting'`).
- Lizenz-/Pay-per-use-Logik wie 1:1- und Group-Chats (3.1).
- Externe Teilnehmer erkennbar über `chat.members` → `tenantId` ≠ Home-Tenant oder fehlend.

### 3.4 Meeting-Transkripte

| Aspekt | Detail |
|---|---|
| Endpoint | `GET /users/{id}/onlineMeetings/{id}/transcripts`, `…/transcripts/{id}/content` (VTT) |
| Permission (Application) | `OnlineMeetingTranscript.Read.All` |
| Voraussetzung Erstellung | **Teams Premium** oder **Microsoft 365 Copilot** Lizenz für den Organisator (für Transkripterzeugung); ohne diese Lizenz gibt es **kein Transkript zum Lesen** |
| Verfügbarkeit pro Meeting | nicht garantiert: hängt von Organisator-Lizenz, Meeting-Policy und manueller Aktivierung „Transkript starten" ab |
| Sprachverfügbarkeit | begrenzt (siehe Microsoft-Doku der Sprach-Matrix) – externe Teilnehmer-Inhalte können fehlen |
| Identifikation des Meetings | Mapping `joinWebUrl` → `onlineMeeting`; pro Postfach des Organisators abrufbar |

**Risiko:** Selbst mit korrekten Permissions liefert Graph nur das, was Teams aufgezeichnet hat. Anforderung „Meeting-Transkripte verarbeiten" ist daher **lizenz- und policy-abhängig**, nicht rein API-abhängig.

### 3.5 Recordings / Artifacts

- `OnlineMeetingArtifact.Read.All` (Application) für Meeting-Recordings und zugehörige Artefakte.
- Recordings landen physisch in OneDrive (privat) oder SharePoint (Channel-Meetings) – Zugriff erfordert zusätzlich `Files.Read.All` / `Sites.Read.All` für Volltext/Download.
- **Aufbewahrung** unterliegt der Tenant-Recording-Policy; älter als Policy ⇒ nicht mehr verfügbar.

### 3.6 Externe Teilnehmer erkennen

- Via `chat.members[].tenantId` und `onlineMeeting.participants[].upn` / `…tenantId`:
  - `tenantId` ≠ Home-Tenant ⇒ Gast oder Federated User
  - fehlender `tenantId` / Anonymous-Join ⇒ extern
- Bei E-Mail: Domain-Vergleich gegen interne Domains-Liste (siehe [10-matching.md](10-matching.md) Erfassungslogik 1–8).
- **Limitierung:** „Federated" Mitarbeiter eines Tochterunternehmens haben anderen Tenant – müssen explizit als „intern" konfigurierbar sein.

### 3.7 Permalinks zu Nachrichten

- `chatMessage.webUrl` liefert direkten Permalink in Teams-Client.
- Für `channelMessage` ebenfalls `webUrl` verfügbar.
- Für BC-Timeline als Quellen-Link nutzbar (Anforderung „Quellen sichtbar").

### 3.8 Change Notifications für Chats / Channels

| Aspekt | Status |
|---|---|
| Resources | `/chats/{id}/messages`, `/teams/{id}/channels/{id}/messages`, `/chats/getAllMessages`, `/users/{id}/chats/getAllMessages` |
| Tenant-weit „getAllMessages" | nur mit Pay-per-use (Modell B) bzw. E5-Capacity |
| Encrypted Content | für `getAllMessages`-Resources **verpflichtend** – Public Key des Subscribers im Subscription-Request, Decryption mit Private Key in Key Vault |
| RSC | für team-installierte Channel-Subscriptions Pflicht-Pfad |
| Max. Lebensdauer | ca. **60 min** für Teams Resources – sehr kurz, Renewer muss aggressiv arbeiten |
| Lifecycle Notifications | Pflicht – `reauthorizationRequired` häufig |

**Konsequenz:** Teams-Subscriptions sind **operativ deutlich aufwendiger** als Mail-Subscriptions (kürzere Lebensdauer, Pflicht-Encryption, Cert-Rotation).

---

## 4. SharePoint / OneDrive

| Bedarf | Endpoint | Permission (Application) | Hinweis |
|---|---|---|---|
| Sites auflisten | `GET /sites?search=` | `Sites.Read.All` | Tenant-weit |
| Drives | `GET /sites/{id}/drives`, `GET /users/{id}/drive` | `Files.Read.All` / `Sites.Read.All` | OneDrive = `users/{id}/drive` |
| Geteilte Items (extern) | `GET /sites/{id}/drives/{id}/items/{id}/permissions` | `Sites.Read.All` | externe Sharing-Links erkennbar an `link.scope = anonymous` / `users` mit externer UPN |
| Sharing Links erkennen | `permissions[].link` | `Sites.Read.All` | extern via `grantedToIdentitiesV2` |
| Webhooks auf Drives | `POST /subscriptions` mit Resource `/drives/{id}/root` | `Files.Read.All` | nur Root-Level, kein Item-Level |
| Volltext Dokumente | Download via `/drives/{id}/items/{id}/content` | `Files.Read.All` | Indexierung in Azure AI Search empfohlen, **nicht** Speicherung in BC |

**Empfehlung:** Anhänge nur als **Referenz** (`webUrl` + `driveItemId`) in BC speichern, Volltext in Azure AI Search. Anforderung „keine unnötige Volltextspeicherung in BC" wird so eingehalten.

---

## 5. Capability Matrix

| Datentyp | Permission(s) | App vs. Delegated | Pay-per-use? | Lizenzanforderung | RSC möglich? | Change Notifications? | Risiko/Limitierung |
|---|---|---|---|---|---|---|---|
| Mail – Lesen | `Mail.Read` | App | nein | – | nein | ja (≤3 d) | Application Access Policy zur Einschränkung erforderlich |
| Mail – MIME / Header | `Mail.Read` | App | nein | – | nein | ja | – |
| Mail – Anhänge | `Mail.Read` | App | nein | – | nein | ja | große Anhänge ⇒ Streaming, Throttling |
| 1:1- / Group-Chats | `Chat.Read.All` | App | **ja (Modell B)** oder E5 (Modell A) | M365 E5 oder Pay-per-use | eingeschränkt (`Chat.Manage.Chat` RSC nur für installierte Apps) | ja, kurze TTL, encrypted Pflicht | Kostenrisiko, kurze Subscription-Lebensdauer |
| Channel-Messages tenant-weit | `ChannelMessage.Read.All` | App | **ja (Modell B)** oder E5 | M365 E5 oder Pay-per-use | nein | ja, kurze TTL, encrypted Pflicht | Kostenrisiko |
| Channel-Messages pro Team | RSC `ChannelMessage.Read.Group` | App (RSC) | **nein** | – | **ja** | ja, kurze TTL | App-Installation pro Team nötig, organisatorisch aufwendig |
| Meeting-Chats | `Chat.Read.All` (chatType=meeting) | App | wie Chats | wie Chats | – | ja | wie Chats |
| Meeting-Transkripte | `OnlineMeetingTranscript.Read.All` | App | nein | **Teams Premium / Copilot beim Organisator** | – | nein (Pull) | nicht jedes Meeting hat Transkript |
| Meeting-Recordings | `OnlineMeetingArtifact.Read.All` + `Files.Read.All` | App | nein | – | – | nein | Tenant-Recording-Policy bestimmt Aufbewahrung |
| SharePoint-Dokumente | `Sites.Read.All`, `Files.Read.All` | App | nein | – | site-level RSC `Sites.Selected` möglich | ja (Drive-Root, ≤3 d) | nur Root-Webhook |
| OneDrive | `Files.Read.All` | App | nein | – | nein | ja (Drive-Root) | je User ein Drive ⇒ viele Subscriptions |
| Externe Teilnehmer (Identifikation) | – (in Daten enthalten) | – | – | – | – | – | Federated Mitarbeiter ggf. fälschlich „extern" |

---

## 6. Risiken & Limits

### 6.1 Pay-per-use-Kosten – Modellrechnung

**Annahmen (sind in §13 Open Questions zu validieren):**
- 1.000.000 Teams-Nachrichten / Monat unternehmensweit (1:1, Group, Channel kombiniert).
- Listenpreis indikativ ~0,00075 USD/Nachricht für Modell B (Microsoft Teams Export API).
- Subscription-Notifications zählen pro abgerufener Nachricht, nicht pro Notification.

**Rechnung (indikativ):**
- 1.000.000 × 0,00075 USD = **~750 USD / Monat** Pay-per-use-Anteil.
- Bei 10× Volumen (10 Mio. Nachrichten): **~7.500 USD / Monat**.
- Plus: Backfill-Kosten beim Initial-Load der Historie (z. B. 12 Monate Historie × 1 Mio. ≈ **~9.000 USD einmalig**).
- Plus: Storage- und Compute-Kosten Azure (separat).

**Konsequenz:** Tenant-weiter Application-Zugriff auf Teams-Nachrichten ist **kostenrelevant** und muss vor MVP2 vertraglich/finanziell freigegeben sein. Alternative über RSC pro Team senkt Kosten, erhöht Rollout-Aufwand.

### 6.2 Teams-Premium / Copilot-Lizenz für Transkripte

- Ohne Premium/Copilot beim Organisator: kein Transkript ⇒ Anforderung „Meeting-Transkripte und Zusammenfassungen verarbeiten" technisch **nicht erfüllbar** für betroffene Meetings.
- Risiko: Meetings mit **externer Organisatorrolle** (Kunde lädt ein) haben **nie** ein Transkript bei uns – nur lokal manuelle Aufzeichnung.

### 6.3 RSC-Skalierung

- RSC erfordert eine **Teams-App-Installation pro Team**. Bei 500 Teams ⇒ 500 Installationen.
- Automatisierbar via Graph (`installedApps`), aber benötigt einmaligen Tenant-Admin-Konsens und laufendes Monitoring (neu erstellte Teams müssen automatisch ausgestattet werden – Subscription auf `groups`-Erstellung).
- Eigentümer-/Mitgliederwiderspruch möglich (Compliance/Betriebsrat).

### 6.4 Encrypted Content Subscriptions

- Pflicht für Teams `getAllMessages`-Resources.
- Public Key (X.509) im Subscription-Request, Private Key zur Decryption in **Key Vault HSM**.
- Operative Folge: Cert-Rotation, Cert-Backup, Key-Vault-Permissions, Monitoring auf Decryption-Fehler.
- Bei verlorenem Private Key sind verschlüsselte Notifications **nicht wiederherstellbar**.

### 6.5 Throttling

- Graph-Throttling pro App + pro Mailbox/Resource. Typische Limits:
  - Mail: ca. 10.000 Requests / 10 min / Mailbox.
  - Teams: deutlich engere Limits, insbesondere `chats/getAllMessages`.
- Pipeline muss `Retry-After`-Header respektieren, exponentielles Backoff anwenden.
- Backfill-Strategie ([07-ingestion-pipeline.md](07-ingestion-pipeline.md) §11) muss drosseln.

### 6.6 Subscription-Renewal

- Cron-basierter Renewer pro Subscription nötig.
- Mail: Renewal alle ~2 Tage (Puffer vor 3-Tage-Ablauf).
- Teams: Renewal alle ~45 min (Puffer vor 60-min-Ablauf) ⇒ hohe Frequenz, eigener Worker.
- `reauthorizationRequired`-Lifecycle muss App-seitig behandelt werden (Token erneuern, Subscription neu validieren).

### 6.7 Datenmodell-Lücken

- **Telefonie / Teams Phone**: Call-Records unter `/communications/callRecords` (Permission `CallRecords.Read.All`) sind nur **Metadaten** (Teilnehmer, Dauer, Qualität), **keine** Inhalte / Transkripte – Anforderung „Telefonanlage als Quelle" ist über Graph nicht erfüllbar.
- **Externe Mailinglisten / Newsletter-Markierung**: keine zuverlässige Graph-Property; Heuristik nötig (List-Unsubscribe-Header etc.).
- **Sensitivity Labels**: über `singleValueExtendedProperties`/`assignedLabels` lesbar, aber nicht für jede Mail gesetzt.

---

## 7. Empfehlung – gestaffelter Ansatz

Die Empfehlung folgt der MVP-Schichtung in [13-mvp-roadmap.md](13-mvp-roadmap.md) und reduziert Lizenz-/Kosten-/Compliance-Risiken zu Beginn.

### MVP 1 – nur E-Mail (geringes Risiko, hoher Nutzen)
- `Mail.Read` Application + **Application Access Policy** auf Pilot-Mail-Sicherheitsgruppe.
- Change Notifications pro Pilot-Mailbox + Delta-Query-Fallback.
- Keine Teams-Erfassung serverseitig (Teams nur via Add-in delegiert, falls Pilot-Bedarf).
- **Begründung:** keine Pay-per-use-Kosten, etablierte API, geringe Compliance-Hürde, deckt den größten Teil externer Geschäftskommunikation ab.

### MVP 2 – Teams-Pilot über RSC für definierte Teams (kontrollierte Erweiterung)
- App-Installation in **definierten Pilot-Teams** mit RSC `ChannelMessage.Read.Group`.
- Optional Pay-per-use für 1:1-/Group-Chats nur in **Pilotgruppe**, mit Budget-Cap und Monitoring.
- Encrypted Content Subscriptions implementieren, Cert-Rotation produktionsreif.
- **Begründung:** Lizenzkosten kontrollierbar, organisatorische RSC-Hürde nur einmalig pro Pilot-Team, technisches Encryption-/Renewal-Setup wird produktionserprobt, **bevor** Tenant-weit ausgerollt wird.

### MVP 3 – Transkripte / Meetings, abhängig von Lizenz
- `OnlineMeetingTranscript.Read.All` + `OnlineMeetingArtifact.Read.All`.
- **Voraussetzung:** Klärung mit Microsoft-Lizenz-Partner, ob Teams Premium / Copilot bei Pilotorganisatoren verfügbar.
- Recordings nur referenziert (SharePoint/OneDrive-Link), nicht kopiert.
- **Begründung:** Mehrwert nur dort, wo Transkript faktisch existiert; ohne Lizenzklärung droht „leere" Funktion.

### MVP 4 – Tenant-weiter Rollout Teams (nur nach Vertrags- und Compliance-Freigabe)
- Tenant-weite Subscriptions mit Pay-per-use **oder** RSC-Vollausrollung über Auto-Install-Skripte.
- Mit Betriebsvereinbarung, Data-Protection-Impact-Assessment und Budgetfreigabe.

---

## 8. Offene Fragen

1. **Lizenzbestand**: Wie viele M365-E5-Lizenzen sind im Tenant vorhanden? Reichen sie für Modell A, oder ist Modell B (Pay-per-use) wirtschaftlich günstiger?
2. **Teams-Nachrichtenvolumen**: Belastbare Schätzung externer Teams-Nachrichten/Monat ist Voraussetzung für Modellrechnung §6.1.
3. **Teams-Premium / Copilot-Verbreitung**: Wie viele Organisatoren externer Meetings haben Premium/Copilot? Ohne Daten ist MVP3-Scope nicht planbar.
4. **Application Access Policy**: Ist Exchange-Admin-seitig die Einrichtung einer Pilot-Mail-Sicherheitsgruppe sofort umsetzbar?
5. **RSC-Auto-Install**: Politisch tragfähig, dass die App in **alle** Teams installiert wird, oder nur opt-in?
6. **Federated Tenants**: Welche externen Tenants sind „intern" (Konzernschwestern) und sollen aus „externer Kommunikation" ausgenommen werden?
7. **Encrypted Content**: Ist Key Vault Premium (HSM) im Tenant verfügbar? Wer betreibt Cert-Rotation?
8. **Telefonie**: Soll Call-Records-Metadatenerfassung trotz fehlender Inhalte erfolgen, oder Telefonie aus Scope nehmen?
9. **Sensitivity Labels**: Wird im Tenant aktiv gelabelt? Wenn ja: Mapping-Tabelle Label → Behandlung in Pipeline (Erfassung / Ausschluss / nur Metadaten) erforderlich.
10. **Newsletter-/Bulk-Mail-Erkennung**: Welche Heuristik gilt als „ausreichend" (List-Unsubscribe, Sender-Reputation, Domain-Whitelist)?
11. **DSGVO-Auftragsverarbeitung**: Liegt Auftragsverarbeitungsvertrag mit Microsoft für Teams Export API / Pay-per-use vor?
12. **Backfill-Tiefe**: Wie weit zurück soll initial geladen werden? (Kostenrelevant – siehe §6.1.)
