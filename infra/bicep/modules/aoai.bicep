// =============================================================================
// aoai.bicep — Azure OpenAI Account (Sweden Central, EU Data Boundary)
// Plandokument: ../../../docs/plan/08-ai-orchestration.md
// NOTE: Model-Deployments (gpt-4o, text-embedding-3-large) sind quota-pflichtig
//       und werden in einem separaten PR/Step provisioniert, nachdem die
//       Capacity-Requests genehmigt sind. Account selbst hat keine Kosten.
// TODO Sprint 1.x: Private Endpoint, publicNetworkAccess = 'Disabled'
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
@allowed(['dev', 'test', 'prod'])
param env string
param location string = 'swedencentral'
param tags object = {}

@description('Ressourcen-ID des Audit-Log-Analytics-Workspaces.')
param auditWorkspaceId string

var accountName = toLower('${namePrefix}-aoai-${env}')

resource aoai 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: accountName
  location: location
  tags: tags
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: accountName
    publicNetworkAccess: 'Enabled' // TODO Sprint 1.x: 'Disabled'
    disableLocalAuth: true
    networkAcls: {
      defaultAction: 'Allow' // TODO Sprint 1.x: 'Deny'
      virtualNetworkRules: []
      ipRules: []
    }
  }
}

resource diag 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'audit-to-law'
  scope: aoai
  properties: {
    workspaceId: auditWorkspaceId
    logs: [
      {
        categoryGroup: 'audit'
        enabled: true
      }
      {
        categoryGroup: 'allLogs'
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

output id string = aoai.id
output name string = aoai.name
output endpoint string = aoai.properties.endpoint
output principalId string = aoai.identity.principalId
