// =============================================================================
// servicebus.bicep — Service Bus Namespace + Topics (ingest.raw/normalized/failed)
// Plandokument: ../../../docs/plan/07-ingestion-pipeline.md
// dev/test: Standard SKU (kein PE, keine Geo-DR) — prod: Premium
// TODO Sprint 1.x: Private Endpoint, Geo-DR-Pairing (prod)
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
@allowed(['dev', 'test', 'prod'])
param env string
param location string = resourceGroup().location
param tags object = {}

var skuName = env == 'prod' ? 'Premium' : 'Standard'
var skuCapacity = env == 'prod' ? 1 : null

resource sb 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${namePrefix}-sb-${env}'
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuName
    capacity: skuCapacity
  }
  properties: {
    disableLocalAuth: true
    publicNetworkAccess: 'Enabled' // TODO Sprint 1.x: 'Disabled' für prod + PE
    minimumTlsVersion: '1.2'
  }
}

var topicNames = [
  'ingest-raw'
  'ingest-normalized'
  'ingest-failed'
]

resource topics 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = [for name in topicNames: {
  name: name
  parent: sb
  properties: {
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enablePartitioning: false
    supportOrdering: true
    defaultMessageTimeToLive: 'P14D'
  }
}]

resource workerSubs 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = [for (name, i) in topicNames: {
  name: 'worker'
  parent: topics[i]
  properties: {
    requiresSession: env == 'prod' // Sessions nur auf Premium
    deadLetteringOnMessageExpiration: true
    deadLetteringOnFilterEvaluationExceptions: true
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}]

output id string = sb.id
output namespace string = sb.name
output endpoint string = sb.properties.serviceBusEndpoint
