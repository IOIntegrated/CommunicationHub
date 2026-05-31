// Dev-Umgebung – Parameter-Skelett. Siehe ../main.bicep.
using '../main.bicep'

param env = 'dev'
param location = 'swedencentral'
param namePrefix = 'cch'

// TODO Sprint 1: tenantId override (falls dev in eigenem Tenant)
// param tenantId = '00000000-0000-0000-0000-000000000000'

param tags = {
  env: 'dev'
  workload: 'communication-copilot'
  managedBy: 'bicep'
  costCenter: 'TODO'
  dataClassification: 'internal'
}
