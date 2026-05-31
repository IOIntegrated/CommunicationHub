// =============================================================================
// loganalytics.bicep — Log Analytics (ops + audit) + Application Insights
// Plandokument: ../../../docs/plan/12-security-compliance.md §7
// =============================================================================

targetScope = 'resourceGroup'

@description('Namens-Präfix (z. B. cch).')
param namePrefix string

@description('Umgebung (dev|test|prod).')
@allowed(['dev', 'test', 'prod'])
param env string

@description('Region.')
param location string = resourceGroup().location

@description('Tags.')
param tags object = {}

var opsRetention = env == 'prod' ? 365 : (env == 'test' ? 90 : 30)
// Audit-Retention: prod = 7 Jahre (2555 Tage) per Plan; muss ggf. via Archive tier.
var auditRetention = env == 'prod' ? 730 : 365

resource lawOps 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${namePrefix}-law-ops-${env}'
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: opsRetention
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: env == 'prod' ? -1 : 1
    }
  }
}

resource lawAudit 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${namePrefix}-law-audit-${env}'
  location: location
  tags: union(tags, { dataClassification: 'audit' })
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: auditRetention
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${namePrefix}-appi-${env}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: lawOps.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    DisableLocalAuth: true
  }
}

output lawOpsId string = lawOps.id
output lawAuditId string = lawAudit.id
output appInsightsId string = appInsights.id
output appInsightsConnectionString string = appInsights.properties.ConnectionString
