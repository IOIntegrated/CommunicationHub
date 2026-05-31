// =============================================================================
// main.bicep — Customer Communication Copilot, RG-Deployment
// Plandokument: ../../docs/plan/01-architecture.md
// =============================================================================

targetScope = 'resourceGroup'

@description('Umgebungs-Slug.')
@allowed(['dev', 'test', 'prod'])
param env string

@description('Azure-Region. Default Sweden Central (EU Data Boundary, AOAI verfügbar).')
param location string = 'swedencentral'

@description('Globales Namens-Präfix für alle Ressourcen.')
@minLength(2)
@maxLength(6)
param namePrefix string = 'cch'

@description('Gemeinsame Tags.')
param tags object = {
  env: env
  workload: 'communication-copilot'
  managedBy: 'bicep'
}

// -----------------------------------------------------------------------------
// 1. Log Analytics + App Insights (vorab — alle anderen Module hängen am LAW)
// -----------------------------------------------------------------------------
module law 'modules/loganalytics.bicep' = {
  name: 'law'
  params: {
    namePrefix: namePrefix
    env: env
    location: location
    tags: tags
  }
}

// -----------------------------------------------------------------------------
// 2. Key Vault
// -----------------------------------------------------------------------------
module kv 'modules/keyvault.bicep' = {
  name: 'kv'
  params: {
    namePrefix: namePrefix
    env: env
    location: location
    tags: tags
    auditWorkspaceId: law.outputs.lawAuditId
  }
}

// -----------------------------------------------------------------------------
// 3. Service Bus + Topics
// -----------------------------------------------------------------------------
module sb 'modules/servicebus.bicep' = {
  name: 'sb'
  params: {
    namePrefix: namePrefix
    env: env
    location: location
    tags: tags
  }
}

// -----------------------------------------------------------------------------
// 4. AI Search
// -----------------------------------------------------------------------------
module srch 'modules/aisearch.bicep' = {
  name: 'srch'
  params: {
    namePrefix: namePrefix
    env: env
    location: location
    tags: tags
  }
}

// -----------------------------------------------------------------------------
// 5. Azure OpenAI (Account ohne Deployments — Quota separat anfragen)
// -----------------------------------------------------------------------------
module aoai 'modules/aoai.bicep' = {
  name: 'aoai'
  params: {
    namePrefix: namePrefix
    env: env
    location: location
    tags: tags
    auditWorkspaceId: law.outputs.lawAuditId
  }
}

// -----------------------------------------------------------------------------
// 6. Ingestion Functions
// -----------------------------------------------------------------------------
module func 'modules/functions.bicep' = {
  name: 'func'
  params: {
    namePrefix: namePrefix
    env: env
    location: location
    tags: tags
    appInsightsConnectionString: law.outputs.appInsightsConnectionString
    lawOpsId: law.outputs.lawOpsId
  }
}

// -----------------------------------------------------------------------------
// 7. Copilot API (App Service)
// -----------------------------------------------------------------------------
module api 'modules/appservice.bicep' = {
  name: 'api'
  params: {
    namePrefix: namePrefix
    env: env
    location: location
    tags: tags
    appInsightsConnectionString: law.outputs.appInsightsConnectionString
    lawOpsId: law.outputs.lawOpsId
  }
}

// -----------------------------------------------------------------------------
// 8. RBAC zwischen MIs und Datenservices
// -----------------------------------------------------------------------------
module rbac 'modules/rbac.bicep' = {
  name: 'rbac'
  params: {
    keyVaultName: kv.outputs.name
    serviceBusName: sb.outputs.namespace
    searchName: srch.outputs.name
    aoaiName: aoai.outputs.name
    apiPrincipalId: api.outputs.principalId
    funcPrincipalId: func.outputs.principalId
  }
}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------
output appInsightsConnectionString string = law.outputs.appInsightsConnectionString
output keyVaultName string = kv.outputs.name
output keyVaultUri string = kv.outputs.uri
output serviceBusNamespace string = sb.outputs.namespace
output searchEndpoint string = srch.outputs.endpoint
output aoaiEndpoint string = aoai.outputs.endpoint
output apiHostName string = api.outputs.defaultHostName
output functionAppName string = func.outputs.name
