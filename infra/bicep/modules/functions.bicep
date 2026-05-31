// =============================================================================
// functions.bicep — Ingestion Functions (.NET 8 isolated)
// Plandokument: ../../../docs/plan/07-ingestion-pipeline.md
// dev/test: Consumption (Y1) — prod: Premium EP1
// TODO Sprint 1.x: VNet-Integration, publicNetworkAccess = 'Disabled'
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
@allowed(['dev', 'test', 'prod'])
param env string
param location string = resourceGroup().location
param tags object = {}

@description('AI Insights Connection String.')
param appInsightsConnectionString string

@description('Ressourcen-ID des Log Analytics Workspaces (ops).')
param lawOpsId string

var planName = '${namePrefix}-func-plan-${env}'
var funcAppName = '${namePrefix}-func-${env}'
var storageName = toLower(replace('${namePrefix}fn${env}${take(uniqueString(resourceGroup().id), 6)}', '-', ''))

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  tags: tags
  sku: {
    name: env == 'prod' ? 'Standard_ZRS' : 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true // TODO Sprint 1.x: false + MI-only
    publicNetworkAccess: 'Enabled' // TODO Sprint 1.x: 'Disabled'
  }
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  tags: tags
  kind: 'functionapp'
  sku: env == 'prod' ? {
    name: 'EP1'
    tier: 'ElasticPremium'
    family: 'EP'
    capacity: 1
  } : {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true // Linux
  }
}

resource funcApp 'Microsoft.Web/sites@2023-12-01' = {
  name: funcAppName
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      http20Enabled: true
      alwaysOn: env == 'prod'
      appSettings: [
        { name: 'AzureWebJobsStorage__accountName', value: storage.name }
        { name: 'AzureWebJobsStorage__credential', value: 'managedidentity' }
        { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
        { name: 'WEBSITE_RUN_FROM_PACKAGE', value: '1' }
      ]
    }
  }
}

// Storage Blob Data Owner für Function-MI (für AzureWebJobsStorage MI-Auth)
resource storageBlobOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, funcApp.id, 'StorageBlobDataOwner')
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
    principalId: funcApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource diag 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'logs-to-law'
  scope: funcApp
  properties: {
    workspaceId: lawOpsId
    logs: [
      { categoryGroup: 'allLogs', enabled: true }
    ]
    metrics: [
      { category: 'AllMetrics', enabled: true }
    ]
  }
}

output id string = funcApp.id
output name string = funcApp.name
output principalId string = funcApp.identity.principalId
output storageName string = storage.name
output defaultHostName string = funcApp.properties.defaultHostName
