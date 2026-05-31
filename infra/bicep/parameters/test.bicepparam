// Test-Parameter. Siehe ../main.bicep.
using '../main.bicep'

param env = 'test'
param location = 'swedencentral'
param namePrefix = 'cch'

param tags = {
  env: 'test'
  workload: 'communication-copilot'
  managedBy: 'bicep'
  costCenter: 'rd-internal'
  dataClassification: 'internal'
}
