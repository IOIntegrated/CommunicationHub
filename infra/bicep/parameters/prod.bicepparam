// Prod-Parameter. Siehe ../main.bicep.
using '../main.bicep'

param env = 'prod'
param location = 'swedencentral'
param namePrefix = 'cch'

param tags = {
  env: 'prod'
  workload: 'communication-copilot'
  managedBy: 'bicep'
  costCenter: 'production'
  dataClassification: 'confidential'
}
