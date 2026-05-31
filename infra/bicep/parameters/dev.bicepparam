// Dev-Parameter. Siehe ../main.bicep.
using '../main.bicep'

param env = 'dev'
param location = 'swedencentral'
param namePrefix = 'cch'

param tags = {
  env: 'dev'
  workload: 'communication-copilot'
  managedBy: 'bicep'
  costCenter: 'rd-internal'
  dataClassification: 'internal'
}
