// =============================================================================
// main.bicep – Customer Communication Copilot, Top-Level-Deployment (Skelett)
// Plandokument: ../../docs/plan/01-architecture.md
// Status: MVP1 Sprint 0 – nur Skelett. Module sind angelegt, aber ohne Inhalt.
// =============================================================================

targetScope = 'resourceGroup'

@description('Umgebungs-Slug, z. B. dev | test | prod.')
@allowed([
  'dev'
  'test'
  'prod'
])
param env string

@description('Azure-Region. Default Sweden Central (EU Data Boundary, AOAI verfügbar).')
param location string = 'swedencentral'

@description('Entra-ID-Tenant für die zu deployende Umgebung.')
param tenantId string = subscription().tenantId

@description('Globales Namens-Präfix für alle Ressourcen, z. B. "cch".')
@minLength(2)
@maxLength(6)
param namePrefix string = 'cch'

@description('Gemeinsame Tags für alle Ressourcen.')
param tags object = {
  env: env
  workload: 'communication-copilot'
  managedBy: 'bicep'
}

// -----------------------------------------------------------------------------
// TODO Sprint 1: Module-Aufrufe aktivieren, sobald die einzelnen Module
// implementiert sind. Reihenfolge / Abhängigkeiten:
//
//   1. Log Analytics (inkl. dediziertem Audit-Workspace) – wird von allen
//      anderen Modulen für Diagnostic Settings referenziert.
//   2. Key Vault – Secrets/Certs für AOAI, SB-Cert, BC-OAuth, Graph-Cert.
//   3. Service Bus – Topics ingest.raw / ingest.normalized / ingest.failed.
//   4. AI Search – Indizes interactions, bc-master, bc-documents, summaries,
//      transcripts (Schema kommt aus Backend-Repo).
//   5. Azure OpenAI – Account + Deployments (chat, embeddings).
//   6. Functions (Premium EP1) – Ingestion Receiver + Worker.
//   7. App Service (Linux Container) – Copilot API.
//
// Beispiel (auskommentiert):
//
// module law 'modules/loganalytics.bicep' = {
//   name: '${namePrefix}-law-${env}'
//   params: {
//     location: location
//     namePrefix: namePrefix
//     env: env
//     tags: tags
//   }
// }
// -----------------------------------------------------------------------------

output deploymentNote string = 'Skelett-Deployment – keine Ressourcen erzeugt. Tenant=${tenantId}, Env=${env}, Region=${location}.'
