// =============================================================================
// aoai.bicep – Azure OpenAI (EU Data Boundary) – Skelett
// Plandokument: ../../../docs/plan/08-ai-orchestration.md
// TODO Sprint 1:
//   - Account in Sweden Central (EU Data Boundary)
//   - Deployments: gpt-4o (chat), text-embedding-3-large (embeddings)
//   - Managed Identity: Cognitive Services OpenAI User für Backend + Worker
//   - Private Endpoint, public network access = Disabled
//   - Diagnostic Settings → Audit-Workspace; Capacity-Tracking
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
param env string
param location string = 'swedencentral'
param tags object = {}

// resource aoai 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
//   name: '${namePrefix}-aoai-${env}'
//   location: location
//   tags: tags
//   kind: 'OpenAI'
//   sku: { name: 'S0' }
//   properties: {
//     customSubDomainName: '${namePrefix}-aoai-${env}'
//     publicNetworkAccess: 'Disabled'
//     disableLocalAuth: true
//   }
// }

output note string = 'aoai.bicep – Skelett. Implementierung folgt in Sprint 1.'
