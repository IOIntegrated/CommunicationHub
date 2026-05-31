// =============================================================================
// functions.bicep – Azure Functions Premium (EP1) für Ingestion – Skelett
// Plandokument: ../../../docs/plan/07-ingestion-pipeline.md
// TODO Sprint 1:
//   - App Service Plan Premium EP1 (Linux, Functions)
//   - Function App .NET 8 isolated, alwaysReady = 1, prewarmedInstanceCount = 1
//   - vnetIntegration, publicNetworkAccess = 'Disabled' (Webhook über APIM/Private LB)
//   - Managed Identity: KV Secrets User, SB Data Sender/Receiver, Storage Blob Data Contrib.
//   - App Insights connection (workspace-based)
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
param env string
param location string = resourceGroup().location
param tags object = {}

// TODO Sprint 1: aspId / storageAccountName / appInsightsConnString als params

output note string = 'functions.bicep – Skelett. Implementierung folgt in Sprint 1.'
