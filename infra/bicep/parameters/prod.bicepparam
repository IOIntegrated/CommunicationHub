// Prod-Umgebung – Parameter-Skelett. Siehe ../main.bicep.
// TODO Sprint 1: prod-spezifische Hardening-Parameter (CMK, PE, höhere SKUs).
using '../main.bicep'

param env = 'prod'
param location = 'swedencentral'
param namePrefix = 'cch'

param tags = {
  env: 'prod'
  workload: 'communication-copilot'
  managedBy: 'bicep'
  costCenter: 'TODO'
  dataClassification: 'confidential'
}
