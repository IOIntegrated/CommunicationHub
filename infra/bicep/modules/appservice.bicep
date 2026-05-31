// =============================================================================
// appservice.bicep — Copilot API (Linux App Service)
// Plandokument: ../../../docs/plan/01-architecture.md §3
// dev/test: B1 — prod: P1v3
// TODO Sprint 1.x: VNet, Private Endpoint, publicNetworkAccess = 'Disabled'
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
@allowed(['dev', 'test', 'prod'])
param env string
param location string = resourceGroup().location
param tags object = {}

@description('Application Insights Connection String.')
param appInsightsConnectionString string

@description('Ressourcen-ID des Log Analytics Workspaces (ops).')
param lawOpsId string

var planName = '${namePrefix}-api-plan-${env}'
var apiName = '${namePrefix}-api-${env}'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  tags: tags
  kind: 'linux'
  sku: env == 'prod' ? {
    name: 'P1v3'
    tier: 'PremiumV3'
    family: 'Pv3'
    capacity: 1
  } : (env == 'test' ? {
    name: 'B2'
    tier: 'Basic'
  } : {
    name: 'B1'
    tier: 'Basic'
  })
  properties: {
    reserved: true // Linux
  }
}

resource api 'Microsoft.Web/sites@2023-12-01' = {
  name: apiName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      http20Enabled: true
      alwaysOn: env != 'dev' // B1 unterstützt AlwaysOn nicht
      healthCheckPath: '/healthz'
      appSettings: [
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
        { name: 'ASPNETCORE_ENVIRONMENT', value: env == 'prod' ? 'Production' : 'Development' }
        { name: 'WEBSITE_RUN_FROM_PACKAGE', value: '1' }
      ]
    }
  }
}

resource diag 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'logs-to-law'
  scope: api
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

output id string = api.id
output name string = api.name
output principalId string = api.identity.principalId
output defaultHostName string = api.properties.defaultHostName
