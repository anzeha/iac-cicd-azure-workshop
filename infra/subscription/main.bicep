targetScope = 'subscription'

@description('Azure region for the workshop environment.')
param location string

@description('Deployment environment name.')
param environmentName string

@description('Workload name used as the naming prefix.')
param workloadName string = 'nlb-workshop'

@description('Optional tags applied to the resource group.')
param tags object = {}

var resourceGroupName = '${workloadName}-${environmentName}-rg'

resource workshopResourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: union(tags, {
    workload: workloadName
    environment: environmentName
    managedBy: 'bicep'
  })
}

output resourceGroupName string = workshopResourceGroup.name
output location string = workshopResourceGroup.location
