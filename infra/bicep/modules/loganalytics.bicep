// =============================================================================
// loganalytics.bicep – Log Analytics Workspace(s) – Skelett
// Plandokument: ../../../docs/plan/12-security-compliance.md §7 (Audit)
// TODO Sprint 1:
//   - Zwei Workspaces:
//       a) Betrieb/Telemetrie (App Insights workspace-based)
//       b) DEDIZIERTER AUDIT-WORKSPACE (separat, längere Retention,
//          schreibgeschützter Lese-Workspace für SIEM-Anbindung)
//   - Retention dev=30, test=90, prod=365 (Audit: prod=2555 Tage / 7 Jahre prüfen)
//   - Customer-Managed Keys (CMK) für prod
// =============================================================================

targetScope = 'resourceGroup'

param namePrefix string
param env string
param location string = resourceGroup().location
param tags object = {}

// resource lawOps 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
//   name: '${namePrefix}-law-ops-${env}'
//   location: location
//   tags: tags
//   properties: {
//     sku: { name: 'PerGB2018' }
//     retentionInDays: env == 'prod' ? 365 : (env == 'test' ? 90 : 30)
//     features: { enableLogAccessUsingOnlyResourcePermissions: true }
//   }
// }

// resource lawAudit 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
//   name: '${namePrefix}-law-audit-${env}'
//   location: location
//   tags: union(tags, { dataClassification: 'audit' })
//   properties: {
//     sku: { name: 'PerGB2018' }
//     retentionInDays: env == 'prod' ? 2555 : 365 // 7 Jahre für prod prüfen
//     features: { enableLogAccessUsingOnlyResourcePermissions: true }
//   }
// }

output note string = 'loganalytics.bicep – Skelett. Implementierung folgt in Sprint 1.'
