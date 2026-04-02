using './main.bicep'

param location = 'westeurope'
param environmentName = 'prod'
param workloadName = 'nlb-workshop'
param sqlAdminLogin = 'sqladminuser'
param sqlAdminPassword = 'Secure!Lake88Forest'
param sqlDatabaseName = 'nlb-workshop-prod'
param aksKubernetesVersion = '1.34'
param aksSystemNodeCount = 1
param aksSystemNodeVmSize = 'Standard_D2s_v6'
param tags = {
  owner: 'workshop'
  environment: 'prod'
}
