// =============================================================================
// keyvault.bicep – Skelett (MVP1 Sprint 0)
// Plandokument: ../../../docs/plan/12-security-compliance.md §2 + §10
// TODO Sprint 1:
//   - Premium SKU (HSM-optional), purgeProtection, soft-delete 90 Tage
//   - RBAC (RBAC-authorization model), keine Access Policies
//   - Private Endpoint, public network access = Disabled
//   - Diagnostic Settings → Audit-Workspace
// =============================================================================

targetScope = 'resourceGroup'

@description('Namens-Präfix.')
param namePrefix string

@description('Umgebung (dev|test|prod).')
param env string

@description('Region.')
param location string = resourceGroup().location

@description('Tags.')
param tags object = {}

// TODO Sprint 1: var vaultName = '${namePrefix}-kv-${env}-${take(uniqueString(resourceGroup().id), 4)}'

// resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
//   name: vaultName
//   location: location
//   tags: tags
//   properties: {
//     sku: { family: 'A', name: 'premium' }
//     tenantId: subscription().tenantId
//     enableRbacAuthorization: true
//     enablePurgeProtection: true
//     enableSoftDelete: true
//     softDeleteRetentionInDays: 90
//     publicNetworkAccess: 'Disabled'
//   }
// }

output note string = 'keyvault.bicep – Skelett. Implementierung folgt in Sprint 1.'
