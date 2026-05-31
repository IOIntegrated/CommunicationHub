# 03 – Business-Central Custom APIs

> Bezug: [`../../instructions.md`](../../instructions.md) Abschnitt 1 „APIs". Datenmodell: [02-bc-data-model.md](02-bc-data-model.md). Architektur: [01-architecture.md](01-architecture.md). Sicherheit: [12-security-compliance.md](12-security-compliance.md). Ingestion-Konsument: [07-ingestion-pipeline.md](07-ingestion-pipeline.md). AI-Output-Schemata: [08-ai-orchestration.md](08-ai-orchestration.md).
>
> Designdokument; keine AL-Implementierung. Beispiel-Payloads sind verkürzt JSON.

---

## 1. API-Strategie

- **Custom API Pages** (BC Page-Object mit `APIPublisher = 'communicationhub'`, `APIGroup = 'copilot'`, `APIVersion = 'v1.0'`, `EntityName`/`EntitySetName`). Generierte URL-Form:

  ```
  https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/communicationhub/copilot/v1.0/companies({companyId})/{entitySet}
  ```

- **Authentifizierung:**
  - **S2S OAuth 2.0 Client Credentials** (Application Permission) für das **Communication Copilot Backend**. Die Microsoft-Entra-App des Backends ist in BC mit dem Permission Set `IOI_COMMHUB_API` per `Page 9860 AAD Application` registriert.
  - **Delegated** Aufrufe der BC-APIs sind nicht für Outlook-Add-in/Teams-App vorgesehen; alle Add-ins gehen über das **Backend als Proxy**. Begründung: einheitliche Berechtigungs-, Quellen- und Audit-Logik im Backend.
  - **Basic Auth ist verboten** (BC SaaS).
- **Vertragsmodell:** OData v4 / JSON; Standardfilter (`$filter`, `$select`, `$top`, `$skip`, `$expand`, `$orderby`); zusätzlich für komplexe Operationen **Bound Actions** (Custom-API-Procedure).
- **Konsumenten:**
  - Communication Copilot Backend (synchron, Outlook/Teams Add-in im Hintergrund).
  - Ingestion Worker (asynchron, persistiert Interactions).
  - Andere BC-Erweiterungen (lesend, optional).
- **Einheitliche Pflicht-Header pro Request:**
  - `Authorization: Bearer <jwt>`
  - `If-Match: <etag>` (bei `PATCH`/`DELETE`)
  - `Idempotency-Key: <opaque>` (bei mutierenden Operationen)
  - `x-correlation-id: <guid>` (propagiert)
  - `x-ccc-tenant: <m365-tenant-guid>` (Cross-Check gegen Datensatz)
  - `x-ccc-bc-company: <bc-company-system-id>` (Cross-Check)

> Sicherheits-Pattern: Visibility-Scope wird **vom BC** durchgesetzt (Codeunit `IOI_CommHub_Security` aus [02 §7](02-bc-data-model.md)), nicht vom Backend. Das Backend leitet nur Identitätssignale weiter (`x-ccc-acting-upn`, `x-ccc-acting-user-oid` als informativ; vertrauenswürdig nur die Token-Claims).

---

## 2. Versionierung & Compatibility

- **API Version** steckt in der URL (`v1.0`). Neue Pflicht-Felder oder Breaking-Changes erzwingen `v1.1` (additiv) bzw. `v2.0` (breaking).
- **Additive Felder** (optional, `Nullable`) sind innerhalb derselben Version erlaubt und führen nicht zum Versions-Bump. Konsumenten ignorieren unbekannte Felder.
- **Deprecation:** `Deprecation`- und `Sunset`-Header (RFC 8594) bei Auslaufen, parallele Verfügbarkeit alter und neuer Version mindestens 12 Monate.
- **Schema-Distribution:** OpenAPI-/CSDL-Metadata wird per `$metadata` ausgeliefert; Backend cached pro Mandant.
- **Enum-Erweiterung:** Enums sind `Extensible`; Konsumenten dürfen nur dokumentierte Werte erwarten und müssen unbekannte Werte tolerant verarbeiten (Pass-through).

---

## 3. Endpunkt-Katalog

> Resource-Pfade unter `…/api/communicationhub/copilot/v1.0/companies({companyId})/`. Idempotenz-Spalte: `Y` = `Idempotency-Key` Pflicht, `K` = via Schlüssel (z. B. `externalHash`), `–` = nicht erforderlich (read-only).

| # | Operation | HTTP | Resource | Beschreibung | Auth | Idempotenz |
|---|---|---|---|---|---|---|
| 1 | List Interactions | GET | `interactions` | Liste mit `$filter`, `$select`, `$top`, `$orderby`. | S2S | – |
| 2 | Get Interaction | GET | `interactions({id})` | Einzelne [Communication Interaction](02-bc-data-model.md#31-table-50000--communication-interaction). | S2S | – |
| 3 | Create Interaction | POST | `interactions` | Neue Interaction; `externalHash` Pflicht (Dedup). | S2S | Y / K |
| 4 | Update Interaction | PATCH | `interactions({id})` | Teil-Update mit `If-Match`. | S2S | – |
| 5 | List Participants | GET | `interactions({id})/participants` | Bound. | S2S | – |
| 6 | Add Participant | POST | `interactions({id})/participants` | – | S2S | Y |
| 7 | List Attachments | GET | `interactions({id})/attachments` | – | S2S | – |
| 8 | Add Attachment Ref | POST | `interactions({id})/attachments` | nur Referenzen, kein Binär-Upload. | S2S | Y |
| 9 | List Entity Links | GET | `interactions({id})/entityLinks` | – | S2S | – |
| 10 | Add Entity Link | POST | `interactions({id})/entityLinks` | manuell oder AI-bestätigt. | S2S | K (Composite Key) |
| 11 | Remove Entity Link | DELETE | `interactions({id})/entityLinks({linkId})` | mit `If-Match`. | S2S | – |
| 12 | Suggest Customer/Contact Match | POST | `matching/suggestCustomer` | Bound Action (Action API); kein Side-Effect, liefert Kandidatenliste. | S2S | – |
| 13 | Get Customer Context | GET | `context/customer({customerNo})` | Aggregation: Stamm, offene Belege, offene Aufgaben, letzte 5 Interactions. | S2S | – |
| 14 | Get Contact Context | GET | `context/contact({contactNo})` | analog. | S2S | – |
| 15 | Get Project Context | GET | `context/project({jobNo})` | analog. | S2S | – |
| 16 | Open Action Items | GET | `tasks/openActionItems?ownerId=<userSid>` | offene Action Items. | S2S | – |
| 17 | Related Documents | GET | `documents/related?customerNo=...&jobNo=...` | proxied an Backend / Search; BC liefert nur Anhänge zur Customer-Timeline. | S2S | – |
| 18 | Save AI Summary | POST | `summaries` | inkl. Citations. | S2S | Y |
| 19 | Patch AI Summary | PATCH | `summaries({id})` | Edit/Approve/Status. | S2S | – |
| 20 | Save Reply Draft | POST | `replyDrafts` | inkl. Quellen. | S2S | Y |
| 21 | Patch Reply Draft | PATCH | `replyDrafts({id})` | – | S2S | – |
| 22 | Create Action Item | POST | `actionItems` | aus AI-Vorschlag oder manuell. | S2S | Y |
| 23 | Patch Action Item | PATCH | `actionItems({id})` | Status, Approve. | S2S | – |
| 24 | Get Setup – Internal Domains | GET | `setup/internalDomains` | von Ingestion benötigt (Filterregel Stage 2/3). | S2S | – |
| 25 | Get Setup | GET | `setup` | Singleton. | S2S | – |
| 26 | Patch Setup | PATCH | `setup` | Admin-Backend-Konfiguration. | S2S (Admin-Rolle) | – |
| 27 | Submit Audit Event | POST | `audit` | optional, Backend kann Audit-Spuren synchronisieren. | S2S | Y |

Alle „Save AI …"- und „Reply Draft"-Endpunkte aus `instructions.md` sind durch 18–23 abgedeckt.

---

## 4. Beispiele Request/Response

### 4.1 Create Interaction (POST /interactions) – Mail (Ingestion)

Request:

```http
POST /api/communicationhub/copilot/v1.0/companies({companyId})/interactions
Authorization: Bearer eyJ...
Idempotency-Key: 7f6b...-mail-int
x-correlation-id: 9b8a...-c1
x-ccc-tenant: 11111111-2222-3333-4444-555555555555
x-ccc-bc-company: aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee
Content-Type: application/json

{
  "tenantId": "11111111-2222-3333-4444-555555555555",
  "sourceSystem": "M365 Exchange",
  "channel": "Email",
  "direction": "Inbound",
  "captureMethod": "Server Ingestion",
  "captureTimestamp": "2026-05-31T08:14:11Z",
  "sentAt": "2026-05-31T08:11:02Z",
  "receivedAt": "2026-05-31T08:11:48Z",
  "subject": "Lieferstatus Auftrag 4711 / Zeichnung",
  "snippet": "Kommt unsere Lieferung diese Woche? Bitte aktualisierte Zeichnung senden.",
  "mailboxUpn": "vertrieb@contoso.com",
  "sourceUserAadObjectId": "00000000-...-...-...-................",
  "internetMessageId": "<CAEz...@mail.example.com>",
  "conversationId": "AAQk...",
  "sourceMessageId": "AAMk...",
  "externalHash": "9c1f...3ab",
  "isExternalCommunication": true,
  "sensitivityLevel": "Internal",
  "userVisibilityScope": "Owner Team",
  "ownerUserSecurityId": "...",
  "ownerTeamCode": "VK-EU",
  "processingStatus": "Completed",
  "aiSummaryStatus": "Generated",
  "permalinkUrl": "https://outlook.office.com/mail/...",
  "searchDocId": "mail-tenant1-9c1f3ab",
  "blobObjectId": "raw/mail/2026/05/31/9c1f3ab.eml",
  "retentionUntil": "2036-05-31",
  "participants": [
    { "role": "From", "displayName": "Müller, Tim", "emailAddress": "tim.mueller@kunde.de", "emailDomain": "kunde.de", "isExternal": true, "matchSource": "Rule" },
    { "role": "To",   "displayName": "Vertrieb",     "emailAddress": "vertrieb@contoso.com",  "emailDomain": "contoso.com", "isExternal": false }
  ],
  "entityLinks": [
    { "entityTableNo": 18,  "entitySystemId": "...", "entityNo": "10000",      "confidence": 0.92, "source": "AI Suggested" },
    { "entityTableNo": 36,  "entitySystemId": "...", "entityNo": "SO-004711",  "confidence": 0.81, "source": "Rule" }
  ]
}
```

Erfolgs-Response:

```http
HTTP/1.1 201 Created
Location: …/interactions(2025001)
ETag: W/"abc123"
x-correlation-id: 9b8a...-c1

{ "id": 2025001, "systemId": "...", "etag": "W/\"abc123\"", "processingStatus": "Completed" }
```

Validierungsfehler (z. B. fehlender Tenant-Cross-Check):

```http
HTTP/1.1 422 Unprocessable Entity
Content-Type: application/json
x-correlation-id: 9b8a...-c1

{
  "error": {
    "code": "tenantMismatch",
    "message": "Header x-ccc-tenant stimmt nicht mit Token-Claim 'tid' überein.",
    "target": "x-ccc-tenant",
    "details": [ { "code": "auth.tenantClaim", "message": "Erwartet 1111...555, erhalten 2222..." } ]
  }
}
```

### 4.2 Suggest Customer/Contact Match (POST /matching/suggestCustomer)

Request:

```json
{
  "candidates": {
    "fromEmail": "tim.mueller@kunde.de",
    "fromDomain": "kunde.de",
    "subjectHints": ["SO-004711", "Projekt Alpha"],
    "bodyHints": ["Auftrag 4711", "Zeichnung Rev. C"],
    "participants": [
      { "email": "einkauf@kunde.de", "isExternal": true }
    ]
  },
  "topK": 5,
  "minConfidence": 0.5
}
```

Response:

```json
{
  "matches": [
    { "entityTableNo": 18,  "entityNo": "10000",     "entitySystemId": "...", "confidence": 0.92, "reason": "Domain match kunde.de → Customer 10000" },
    { "entityTableNo": 5050,"entityNo": "C00045",    "entitySystemId": "...", "confidence": 0.88, "reason": "Email exact match Contact" },
    { "entityTableNo": 167, "entityNo": "P-ALPHA",   "entitySystemId": "...", "confidence": 0.78, "reason": "Subject token 'Projekt Alpha'" },
    { "entityTableNo": 36,  "entityNo": "SO-004711", "entitySystemId": "...", "confidence": 0.65, "reason": "Subject regex SO-\\d+" }
  ],
  "evaluatedAt": "2026-05-31T08:14:13Z"
}
```

### 4.3 Get Customer Context

```http
GET …/context/customer('10000')?$select=customer,openDocuments,openActionItems,recentInteractions
```

Response (gekürzt):

```json
{
  "customer": { "no": "10000", "name": "Müller GmbH", "blocked": " ", "currencyCode": "EUR" },
  "openDocuments": [
    { "type": "Sales Order", "no": "SO-004711", "amount": 12450.00, "expectedShipment": "2026-06-02" },
    { "type": "Sales Quote", "no": "SQ-009002", "amount":  3200.00 }
  ],
  "openActionItems": [
    { "id": 887, "description": "Aktualisierte Zeichnung senden", "dueDate": "2026-06-02" }
  ],
  "recentInteractions": [
    { "id": 2025001, "channel": "Email", "subject": "Lieferstatus Auftrag 4711", "sentAt": "2026-05-31T08:11:02Z" }
  ],
  "etag": "W/\"v17\""
}
```

### 4.4 Save AI Summary

```http
POST …/summaries
Idempotency-Key: 7f6b...-summary
```

```json
{
  "interactionId": 2025001,
  "summaryType": "Thread",
  "language": "de",
  "summaryShort": "Kunde fragt nach Lieferstatus zu SO-004711 und bittet um aktualisierte Zeichnung. Lieferung laut System für 02.06. avisiert.",
  "longSummaryBlobId": null,
  "citationsJson": [
    { "id": "src://bc/sales-order/SO-004711", "usedFor": ["short"], "quote": "ExpectedShipment 2026-06-02" },
    { "id": "src://search/mail/9c1f3ab",       "usedFor": ["short"] }
  ],
  "confidence": 0.86,
  "status": "Generated",
  "modelDeployment": "gpt-4.1-eu",
  "modelVersion": "2025-04-01",
  "promptTemplateVersion": "1.3.0",
  "schemaVersion": "1.0.0"
}
```

### 4.5 Save Reply Draft

```json
{
  "interactionId": 2025001,
  "language": "de",
  "tone": "neutral",
  "shortReply": "Vielen Dank für Ihre Nachfrage. Die Lieferung zu SO-004711 ist für den 02.06. geplant; die aktualisierte Zeichnung Rev. C senden wir Ihnen heute zu.",
  "longReplyBlobId": null,
  "internalAssessment": "Belegstatus prüfen vor Versand.",
  "confidence": 0.78,
  "containsCommitments": true,
  "containsPricing": false,
  "containsDeliveryDates": true,
  "status": "Draft",
  "sources": [
    { "referenceId": "src://bc/sales-order/SO-004711", "referenceType": "BC Record", "usedFor": "Reply" },
    { "referenceId": "src://search/mail/9c1f3ab",       "referenceType": "Search Doc", "usedFor": "Reply" }
  ]
}
```

### 4.6 Create Action Item from Suggestion

```json
{
  "interactionId": 2025001,
  "description": "Aktualisierte Zeichnung Rev. C an Müller GmbH senden",
  "dueDate": "2026-06-02",
  "assignedToUserSid": "...",
  "linkedEntityTableNo": 36,
  "linkedEntitySystemId": "...",
  "source": "AI Suggested",
  "status": "Open",
  "confidence": 0.82
}
```

### 4.7 Get Setup – Internal Domains (Ingestion)

```http
GET …/setup/internalDomains
```

```json
{
  "value": [
    { "domain": "contoso.com",     "treatAs": "Internal" },
    { "domain": "contoso-eu.com",  "treatAs": "Internal" },
    { "domain": "tochter-ag.com",  "treatAs": "Federated-Internal", "tenantId": "..." }
  ]
}
```

### 4.8 Validierungsfehler-Beispiel (POST /entityLinks ohne Confidence)

```http
HTTP/1.1 400 Bad Request

{
  "error": {
    "code": "validationFailed",
    "message": "Pflichtfeld fehlt.",
    "details": [
      { "code": "required", "target": "confidence", "message": "Feld 'confidence' ist erforderlich (0..1)." }
    ]
  }
}
```

---

## 5. Idempotenz

- **Header `Idempotency-Key`** ist Pflicht für mutierende Endpunkte (3, 6, 8, 10, 18, 20, 22, 27).
- Schlüssel ist opaque (≤ 80 Zeichen, ASCII), wird vom Aufrufer erzeugt; das Backend nutzt typischerweise den deterministischen `externalHash` der Quelle.
- **Speicherung:** BC führt eine Idempotenz-Tabelle (Codeunit-intern als Memory-Cache + persistente Tabelle, TTL 7 Tage) keyed auf (`Tenant Id`, `Endpoint`, `Idempotency-Key`).
- **Verhalten:**
  - Erstanfrage → normale Verarbeitung → Antwort persistiert (`status`, `responseHash`).
  - Wiederholung mit identischem Body-Hash → ursprüngliche Antwort + `Idempotency-Replayed: true` Header.
  - Wiederholung mit abweichendem Body → `409 Conflict` mit `code: idempotencyConflict`.
- **Dedup über External Hash:** zusätzlich zu `Idempotency-Key`. Insert-Versuch mit existierendem `(Tenant Id, External Hash)` (SK10 unique) → `200 OK` mit existierender Resource (semantisches Upsert) statt `409`, sofern `If-None-Match: *` nicht gesetzt ist; mit `If-None-Match: *` strict-`409`.
- **Bound Actions** (z. B. `matching/suggestCustomer`) sind read-only und benötigen kein Idempotency-Key, sind jedoch deterministisch hinsichtlich Eingabe-Hash (Cache-fähig).

---

## 6. Filter- / Suchstrategie

- **Pflichtfilter pro Liste:** Backend setzt grundsätzlich `tenantId` und nutzt die Schlüssel aus [02 §5](02-bc-data-model.md#5-indizes--schlüsselstrategie):
  - Customer-Timeline: `entityLinks/any(l: l/entityTableNo eq 18 and l/entityNo eq '10000')` plus `$orderby=sentAt desc`.
  - Conversation-Aggregation: `$filter=conversationId eq 'AAQk...'`.
  - Worker-Backlog: `$filter=processingStatus eq 'Pending' and isExternalCommunication eq true`.
- **Empfehlung:**
  - immer `$select` setzen, um Payload-Größe zu reduzieren;
  - `$top` ≤ 100 (BC-Standard); für Bulk-Pulls Server-seitiges Paging über `@odata.nextLink`;
  - `$expand=participants,entityLinks` nur, wenn nötig (kostet Joins);
  - `$filter` möglichst auf Schlüsselfeldern – sonst Scan in BC.
- **Server-Driven Search:** für Volltextsuche **nicht** die BC-API, sondern das Backend (Azure AI Search) nutzen. BC liefert nur strukturiert.

---

## 7. Fehlercodes

OData-/Microsoft-Graph-konformes Error-Schema (`error.code`, `error.message`, `error.target`, `error.details`, `error.innererror`).

| HTTP | Code | Bedeutung | Wiederholbar? |
|---|---|---|---|
| 400 | `validationFailed` | Schema-/Pflichtfeldfehler. | nein |
| 400 | `enumValueUnknown` | Unbekannter Enum-Wert. | nein |
| 401 | `unauthorized` | Token fehlt/abgelaufen. | nach Refresh |
| 403 | `permissionDenied` | Permission Set fehlt. | nein |
| 403 | `visibilityScopeDenied` | Datensatz nicht im Scope des Aufrufers. | nein |
| 404 | `notFound` | Ressource existiert nicht. | nein |
| 409 | `idempotencyConflict` | gleiche Idempotency-Key, anderer Body. | nein |
| 409 | `etagMismatch` | `If-Match` falsch (Concurrent Update). | nach Re-Read |
| 422 | `tenantMismatch` | Token-Tenant ≠ Daten-Tenant / Header. | nein |
| 422 | `companyMismatch` | Header `x-ccc-bc-company` ≠ URL. | nein |
| 423 | `legalHoldActive` | Versuch zu löschen trotz Legal Hold. | nein |
| 429 | `tooManyRequests` | BC-Throttling oder API-Quota. | ja, mit `Retry-After` |
| 500 | `internalError` | BC-Codeunit-Fehler. | ja |
| 502 | `upstreamUnavailable` | Backend-Dependency in BC-Pipeline (z. B. Job). | ja |
| 503 | `serviceUnavailable` | BC-Wartung/Update. | ja |

---

## 8. Rate-Limiting / Throttling

- **BC-SaaS-Limits** (dokumentiert von Microsoft): pro Environment harte Quoten auf API-Requests/sec und parallele Hintergrund-Sessions. Werte sind versions-/SKU-abhängig und müssen pro Tenant überwacht werden.
- **Antwortmuster:** BC liefert bei Überschreitung `429 Too Many Requests` mit `Retry-After` (Sekunden) oder `503` bei kurzer Wartung.
- **Backoff im Backend:** Polly-Pipeline (siehe [01 §11](01-architecture.md)):
  - Honor `Retry-After`, sonst exponentiell mit Jitter (1s, 2s, 5s, 15s, 60s).
  - Circuit Breaker pro Tenant ab Fehlerrate > 20 % über 1 min.
- **Eigene Limits im Backend:** Token-Bucket pro Mandant für BC-API-Aufrufe (z. B. 50 req/s Burst, 20 req/s Sustained); Worker-Concurrency-Cap (siehe [07 §7](07-ingestion-pipeline.md)).
- **Bulk Inserts:** lieber sequenziell pro Conversation (Service-Bus-Sessions) als parallel; `$batch`-OData wird **nicht** für Mutationen verwendet (Atomaritäts-/Idempotenz-Kompliziertheit).

---

## 9. S2S-Setup im Tenant

Schritte zur Inbetriebnahme pro Mandant:

1. **Microsoft Entra App-Registrierung** für Backend (Single-Tenant pro Kundenorganisation oder Multi-Tenant je nach Liefermodell). Pflicht-Konfiguration:
   - Application ID URI: `api://<backend-app-id>`.
   - Application Permission `app_access` (eigene API), keine Graph-Application-Permissions auf dieser App (Graph-Permissions liegen auf separater Identität, siehe [12 §2.2](12-security-compliance.md)).
   - Client-Zertifikat (RSA 4096) im Key Vault; kein Secret in App Settings.
2. **Admin Consent** durch Tenant-Admin (`/adminconsent?client_id=<backend-app-id>`).
3. **In BC: AAD-Anwendungs-Zuordnung**
   - Page **`AAD Applications`** (Suche „Microsoft Entra-Anwendungen"): neuen Eintrag mit `Client ID = <backend-app-id>`, Beschreibung „Communication Copilot Backend", State „Enabled".
   - Permission Set zuweisen: `IOI_COMMHUB_API` (+ `IOI_COMMHUB_ADMIN` ausschließlich für Admin-Service-Identität, falls Setup-Mutation gewünscht).
4. **Token-Bezug:**

   ```http
   POST https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token
   grant_type=client_credentials
   client_id=<backend-app-id>
   client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer
   client_assertion=<signed-jwt-with-cert>
   scope=https://api.businesscentral.dynamics.com/.default
   ```

5. **First-Call Health-Check:** Backend ruft `GET /setup/internalDomains` und `GET /companies` (Standard-API), prüft 200; ansonsten Setup-Workflow-Alert.
6. **Trennung der Identitäten:** Backend nutzt **eine** Identität für BC, **eine separate** für Microsoft Graph (App Permissions Mail/Teams) und **eine** für Azure-Ressourcen (Managed Identity). Damit ist Privilege-Separation gegeben.

---

## 10. Sicherheits-Pattern

- **Token-Validation in BC:** BC validiert Tokens durch SaaS-Plattform; zusätzlich prüft jede Custom-API-Codeunit:
  - `tid`-Claim ↔ `x-ccc-tenant` ↔ Datensatz-`Tenant Id`.
  - Company aus URL ↔ `x-ccc-bc-company` ↔ aktuelle Sitzungs-Company.
  - Bei Mismatch: `422 tenantMismatch`/`companyMismatch`, Audit-Eintrag.
- **Visibility Scope wird durchgesetzt im BC**, nicht im Backend (Codeunit `IOI_CommHub_Security`, [02 §7](02-bc-data-model.md#7-berechtigungslogik-in-pages-und-apis)).
  - Backend kann Datensätze nicht „im Namen eines Benutzers" lesen (S2S-App-Identität sieht alles, was ihr Permission Set erlaubt). Daher wird die feinere Visibility nicht im Backend verlangt; Anzeige-/Re-Authorization läuft pro Endbenutzer-Klick im Add-in über Backend → BC mit zusätzlichem Header `x-ccc-acting-user-oid` (informativ; im Backend wird die Berechtigung des Benutzers gegen Graph/BC vorab geprüft – siehe [12 §6](12-security-compliance.md)).
- **Logging-Hygiene:** Custom-API-Codeunits loggen **keine** Klartext-Bodies; Subjects werden in Audit-Events nur als Hash referenziert (Source Hash). Stack-Traces nicht in Response.
- **DoS-Schutz:** Backend rate-limits pro Mandant; BC-eigene Limits agieren als zweite Schutzebene.

---

## 11. API-Beispielflüsse

### 11.1 Outlook „analyze and save"

1. Outlook Add-in → Backend `POST /analyze` (mit Mail-Kontext).
2. Backend → Graph (Mail-Body, OBO).
3. Backend → BC `POST /matching/suggestCustomer`.
4. Backend → Search/AOAI (Klassifikation, Antwortvorschlag).
5. Benutzer bestätigt im Add-in.
6. Backend → BC `POST /interactions` (Idempotency-Key = `externalHash`).
7. Backend → BC `POST /interactions({id})/entityLinks` für jeden bestätigten Match.
8. Backend → BC `POST /summaries`.
9. Backend → BC `POST /replyDrafts` (status `Draft`).
10. Backend → BC `POST /actionItems` für freigegebene Vorschläge.
11. Outlook Add-in zeigt BC-Permalink (Page „Communication Interaction Detail").

### 11.2 Ingestion „persist new external mail"

1. Graph-Webhook → Ingestion Function → Service Bus.
2. Worker → Graph (Detail-Fetch).
3. Worker → BC `POST /matching/suggestCustomer` (für Stage 7).
4. Worker → AOAI (Klassifikation/Extraktion/Summary).
5. Worker → BC `POST /interactions` (Idempotency-Key = `externalHash`).
   - Response 201 → weiter; bei `200` (Dedup) → PATCH falls neue Felder.
6. Worker → BC `POST /interactions({id})/entityLinks` pro Match.
7. Worker → BC `POST /summaries`.
8. Worker → BC `POST /actionItems` für extrahierte Aufgaben (Status `Open`, `Source = AI Suggested`).
9. Worker → Search Index (siehe [01 §5b](01-architecture.md)).
10. Worker → BC `POST /audit` (optional).

### 11.3 Teams Message Extension „save as interaction"

1. Teams ME → Backend `/teams/message/analyze`.
2. Backend → Graph (Chat-Message, Teilnehmer).
3. Backend → BC `POST /matching/suggestCustomer`.
4. Backend → BC `GET /context/customer({no})` für UI-Card.
5. Benutzer bestätigt EntityLinks und „In BC ablegen".
6. Backend → BC `POST /interactions` (channel `Teams Chat`, `permalinkUrl`, `chatId`, `sourceMessageId`, `externalHash`).
7. Backend → BC `POST /interactions({id})/entityLinks`.
8. Backend → BC `POST /summaries`.
9. Backend → Teams ME mit Adaptive Card + BC-Link.

---

## 12. Telemetrie / Korrelation

- **Pflicht-Header `x-correlation-id`** wird vom anrufenden Service gesetzt (oder vom Backend, wenn Aufruf vom Add-in stammt). BC liest Header in `OnBeforeServiceRequest`-Handler und setzt ihn in `Communication Audit Log Entry.Correlation Id`.
- **Response-Header:**
  - `x-correlation-id` (gespiegelt).
  - `x-ratelimit-remaining`, `x-ratelimit-reset` (sofern Backend-Gateway involviert; nicht von BC selbst gesetzt).
  - `Idempotency-Replayed: true|false`.
  - `Server-Timing: bc;dur=<ms>` (optional).
- **Audit-Custom-Events** (siehe [02 §3.13](02-bc-data-model.md#313-table-50012--communication-audit-log-entry)): pro mutierender Operation ein Eintrag mit Korrelation, Service-Principal, Output-Hash.
- **Application Insights:** BC SaaS Telemetry an Workspace des Backends weitergeleitet (BC `appInsightsKey` in Setup), Custom Dimensions `tenantId`, `correlationId`, `entityType`, `endpoint`.

---

## 13. Offene Fragen

1. **Custom API vs. Standard-OData-Pages:** Soll für Customer/Job das BC-Standard-API-Set (`v2.0`) wiederverwendet werden, oder spiegelt der Custom-API-Layer alles? Annahme: lesende Stammdaten via Standard-API, copilot-spezifisch via `communicationhub/copilot/v1.0`.
2. **Bulk-Insert für Backfill:** Akzeptable Strategie (sequenziell pro Conversation) oder benötigen wir `$batch`/Bulk-Endpunkte?
3. **`audit`-Endpunkt:** Synchronisieren wir Backend-Audit-Trail in BC oder reicht App Insights + dedizierter Audit-Store außerhalb BC?
4. **Permission Set für Setup-Mutation:** wer darf `PATCH /setup` aufrufen – nur eine zweite, separat geschützte Service-Identität?
5. **`x-ccc-acting-user-oid`-Header:** verbindlich oder optional? Bei verbindlich → wie verifizieren (Cross-Check gegen vorgelagerte OBO-Token im Backend, BC vertraut Backend).
6. **Webhooks aus BC:** Soll BC bei Korrekturen (`entityLink confirmed`) das Backend benachrichtigen (Re-Index)? Empfehlung: ja, via BC-Job-Queue + Outbound-Call; offen, ob synchron oder per Storage-Queue.
7. **OpenAPI-Generierung:** Wir generieren `openapi.json` aus den AL-Page-Annotationen plus manuelle Ergänzungen für Bound Actions (`matching/suggestCustomer`).
8. **Long-Reply / Long-Summary Upload:** Soll BC eine vor-signierte URL für Blob-Upload zurückliefern, oder lädt das Backend direkt in Blob und übergibt nur die `Long Blob Id`? Empfehlung: zweite Variante (Backend → Blob via Managed Identity).
9. **Cross-Company-Reads:** Wie behandeln wir Anfragen, die Daten aus mehreren Companies erfordern (Konzern)? Aktuell: pro Company ein Aufruf; Aggregation im Backend.
10. **Versionierungspfad:** `v1.0` → `v1.1` semver-Schwelle definieren (welche Änderungen gelten als breaking).
