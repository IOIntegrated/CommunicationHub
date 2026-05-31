// =============================================================================
// aisearch.bicep — Azure AI Search (basic für dev/test, standard2 für prod)
// Plandokument: ../../../docs/plan/09-data-search.md
// NOTE: Indizes (interactions, bc-master, bc-documents, summaries, transcripts)
//       werden NICHT in Bicep, sondern aus Backend-Code provisioniert.
// TODO Sprint 1.x: Private Endpoint, publicNetworkAccess = 'disabled'
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
@allowed(['dev', 'test', 'prod'])
param env string
param location string = resourceGroup().location
param tags object = {}

var skuName = env == 'prod' ? 'standard2' : 'basic'
var replicas = env == 'prod' ? 2 : 1
var partitions = env == 'prod' ? 1 : 1
var semantic = env == 'prod' ? 'standard' : 'free'

resource search 'Microsoft.Search/searchServices@2024-06-01-preview' = {
  name: toLower('${namePrefix}-srch-${env}')
  location: location
  tags: tags
  sku: {
    name: skuName
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    replicaCount: replicas
    partitionCount: partitions
    semanticSearch: semantic
    publicNetworkAccess: 'enabled' // TODO Sprint 1.x: 'disabled'
    authOptions: {
      aadOrApiKey: {
        aadAuthFailureMode: 'http401WithBearerChallenge'
      }
    }
    disableLocalAuth: false // TODO Sprint 1.x: true (nach App-Migration auf AAD)
  }
}

output id string = search.id
output name string = search.name
output endpoint string = 'https://${search.name}.search.windows.net'
output principalId string = search.identity.principalId
