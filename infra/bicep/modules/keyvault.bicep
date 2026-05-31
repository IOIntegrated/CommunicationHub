// =============================================================================
// keyvault.bicep — Key Vault (RBAC, soft-delete, purge-protection)
// Plandokument: ../../../docs/plan/12-security-compliance.md §2 + §10
// TODO Sprint 1.x: Private Endpoint + publicNetworkAccess = 'Disabled' (VNet-PR)
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
@allowed(['dev', 'test', 'prod'])
param env string
param location string = resourceGroup().location
param tags object = {}

@description('Ressourcen-ID des Log Analytics Workspaces (Audit-Workspace).')
param auditWorkspaceId string

// KV-Name max 24 chars, global eindeutig
var vaultName = toLower('${namePrefix}-kv-${env}-${take(uniqueString(resourceGroup().id), 6)}')
var skuName = env == 'prod' ? 'premium' : 'standard'

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: vaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: skuName
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enablePurgeProtection: true
    enableSoftDelete: true
    softDeleteRetentionInDays: env == 'prod' ? 90 : 30
    publicNetworkAccess: 'Enabled' // TODO Sprint 1.x: 'Disabled' + Private Endpoint
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow' // TODO Sprint 1.x: 'Deny'
    }
  }
}

resource diag 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'audit-to-law'
  scope: kv
  properties: {
    workspaceId: auditWorkspaceId
    logs: [
      {
        categoryGroup: 'audit'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

output id string = kv.id
output name string = kv.name
output uri string = kv.properties.vaultUri
