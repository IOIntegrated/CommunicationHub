// =============================================================================
// servicebus.bicep – Service Bus Premium – Skelett
// Plandokument: ../../../docs/plan/07-ingestion-pipeline.md
// TODO Sprint 1:
//   - SKU Premium (für Private Endpoint, Sessions, Geo-DR)
//   - Topics: ingest.raw, ingest.normalized, ingest.failed
//   - Subscriptions je Worker mit Sessions auf conversationId
//   - DLQ aktiv, deadLetteringOnMessageExpiration = true
//   - RBAC: Managed Identity der Functions = Service Bus Data Sender/Receiver
//   - disableLocalAuth = true
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
param env string
param location string = resourceGroup().location
param tags object = {}

// resource sb 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
//   name: '${namePrefix}-sb-${env}'
//   location: location
//   tags: tags
//   sku: { name: 'Premium', tier: 'Premium', capacity: 1 }
//   properties: {
//     disableLocalAuth: true
//     publicNetworkAccess: 'Disabled'
//   }
// }

output note string = 'servicebus.bicep – Skelett. Implementierung folgt in Sprint 1.'
