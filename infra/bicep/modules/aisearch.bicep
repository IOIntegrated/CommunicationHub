// =============================================================================
// aisearch.bicep – Azure AI Search (S2, Vector) – Skelett
// Plandokument: ../../../docs/plan/09-data-search.md
// TODO Sprint 1:
//   - SKU 'standard2', semanticSearch 'standard', replicas/partitions je env
//   - Managed Identity (System) für RBAC zu AOAI (Skillset)
//   - Indizes (interactions, bc-master, bc-documents, summaries, transcripts)
//     werden NICHT in Bicep, sondern aus Backend-Code provisioniert.
//   - Private Endpoint, publicNetworkAccess = 'disabled'
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
param env string
param location string = resourceGroup().location
param tags object = {}

// resource search 'Microsoft.Search/searchServices@2024-06-01-preview' = {
//   name: '${namePrefix}-srch-${env}'
//   location: location
//   tags: tags
//   sku: { name: 'standard2' }
//   identity: { type: 'SystemAssigned' }
//   properties: {
//     replicaCount: 1
//     partitionCount: 1
//     semanticSearch: 'standard'
//     publicNetworkAccess: 'disabled'
//     authOptions: { aadOrApiKey: { aadAuthFailureMode: 'http401WithBearerChallenge' } }
//   }
// }

output note string = 'aisearch.bicep – Skelett. Implementierung folgt in Sprint 1.'
