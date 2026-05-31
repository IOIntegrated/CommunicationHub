# Ingestion-Pipeline

> Asynchrone Erfassung externer Kommunikation: Microsoft-Graph-Webhooks → Service Bus → Worker → BC.

**Plandokumente:**
- [docs/plan/07-ingestion-pipeline.md](../../docs/plan/07-ingestion-pipeline.md) – 15-Schritt-Pipeline, Subscription-Manager, Idempotenz
- [docs/plan/11-graph-feasibility.md](../../docs/plan/11-graph-feasibility.md) – Lizenz-/Pay-per-use
- [docs/plan/12-security-compliance.md §3](../../docs/plan/12-security-compliance.md) – Graph-Permissions, Application Access Policy

**Owner:** `@TODO-org/communicationhub-backend`

**Stack:** Azure Functions Premium (EP1), .NET 8 isolated. HTTP-/Timer-/Service-Bus-Trigger. Managed Identity gegen Key Vault / Search / Blob / Cosmos / Service Bus.

## Projektstruktur (geplant)

```
src/
  CommunicationHub.Ingestion.Functions/   Function Host (Webhook-Receiver, Subscription-Manager, Worker)
  CommunicationHub.Ingestion.Core/        Pipeline-Stages, Klassifikation, Matching-Client, BC-Client
tests/
  CommunicationHub.Ingestion.Tests/       xUnit
```

## Sprint 0

- [x] Ordnerstruktur + `host.json`-Skelett
- [ ] CI-Workflow `.github/workflows/ingestion-ci.yml`

## Sprint 1 (MVP1 – Mail)

- **GraphSubscriptionManagerFunction** (Timer): Renewal ≤ 3 Tage, Lifecycle-Notifications, Tenant-Onboarding.
- **MailNotificationReceiverFunction** (HTTP): Validation Token, signierte Payloads, Enqueue `ingest.raw`.
- **MailWorkerFunction** (Service Bus Trigger, Sessions je `conversationId`): Filter externe Beteiligung → Match → Klass./Extr. → Summary → Index/Blob → BC-Write.
- Idempotenz-Keys in Cosmos.
- Application-Access-Policy-fähig (Pilot-Sicherheitsgruppe).

## MVP2

- Teams-Channel-Messages via RSC (`ChannelMessage.Read.Group`).
- Encrypted Content Subscriptions (Zertifikat-Lifecycle aus Key Vault).
