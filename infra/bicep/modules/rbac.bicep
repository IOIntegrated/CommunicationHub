// =============================================================================
// rbac.bicep — Konsolidierte Role Assignments für Workload-Identitäten
// Plandokument: ../../../docs/plan/12-security-compliance.md §3
// =============================================================================

targetScope = 'resourceGroup'

param keyVaultName string
param serviceBusName string
param searchName string
param aoaiName string

param apiPrincipalId string
param funcPrincipalId string

// Built-in Role IDs
var roles = {
  keyVaultSecretsUser:       '4633458b-17de-408a-b874-0445c86b69e6'
  storageBlobDataContributor:'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  serviceBusDataSender:      '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
  serviceBusDataReceiver:    '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
  searchIndexDataContributor:'8ebe5a00-799e-43f5-93ac-243d3dce84a7'
  aoaiUser:                  '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd' // Cognitive Services OpenAI User
}

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = { name: keyVaultName }
resource sb 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = { name: serviceBusName }
resource srch 'Microsoft.Search/searchServices@2024-06-01-preview' existing = { name: searchName }
resource aoai 'Microsoft.CognitiveServices/accounts@2024-10-01' existing = { name: aoaiName }

// --- API ---
resource apiKvSecrets 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(kv.id, apiPrincipalId, roles.keyVaultSecretsUser)
  scope: kv
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsUser)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAoai 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aoai.id, apiPrincipalId, roles.aoaiUser)
  scope: aoai
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.aoaiUser)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource apiSearch 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(srch.id, apiPrincipalId, roles.searchIndexDataContributor)
  scope: srch
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.searchIndexDataContributor)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource apiSbSender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sb.id, apiPrincipalId, roles.serviceBusDataSender)
  scope: sb
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.serviceBusDataSender)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// --- Functions ---
resource funcKvSecrets 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(kv.id, funcPrincipalId, roles.keyVaultSecretsUser)
  scope: kv
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsUser)
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource funcSbSender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sb.id, funcPrincipalId, roles.serviceBusDataSender)
  scope: sb
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.serviceBusDataSender)
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource funcSbReceiver 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sb.id, funcPrincipalId, roles.serviceBusDataReceiver)
  scope: sb
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.serviceBusDataReceiver)
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource funcSearch 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(srch.id, funcPrincipalId, roles.searchIndexDataContributor)
  scope: srch
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.searchIndexDataContributor)
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource funcAoai 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aoai.id, funcPrincipalId, roles.aoaiUser)
  scope: aoai
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.aoaiUser)
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}
