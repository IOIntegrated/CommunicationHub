// =============================================================================
// appservice.bicep – Copilot API App Service (Linux Container) – Skelett
// Plandokument: ../../../docs/plan/01-architecture.md §3
// TODO Sprint 1:
//   - Linux App Service Plan (P1v3+), Container-Deployment
//   - VNet-Integration, publicNetworkAccess = 'Disabled' (über Front Door/APIM)
//   - Managed Identity: KV Secrets User, AOAI User, Search Index Data Contrib.,
//     SB Data Sender, Cosmos DB Data Contributor (selektiv)
//   - HTTPS only, minTlsVersion = '1.2', ftpsState = 'Disabled'
//   - HealthCheckPath = '/healthz', AlwaysOn = true
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
param env string
param location string = resourceGroup().location
param tags object = {}

output note string = 'appservice.bicep – Skelett. Implementierung folgt in Sprint 1.'
