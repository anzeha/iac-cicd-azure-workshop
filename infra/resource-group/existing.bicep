targetScope = 'resourceGroup'

param location string = resourceGroup().location
param environmentName string
param workloadName string = 'nlb-workshop'
param tags object = {}
param aksKubernetesVersion string = '1.34'
param aksSystemNodeCount int = 1
param aksSystemNodeVmSize string = 'Standard_D2s_v6'
param aksEnableAutoScaling bool = true
param aksMinNodeCount int = 1
param aksMaxNodeCount int = 3

var normalizedWorkload = toLower(replace(workloadName, '-', ''))
var namePrefix = '${workloadName}-${environmentName}'
var workspaceName = '${namePrefix}-log'
var identityName = '${namePrefix}-workload-id'
var aksName = '${namePrefix}-aks'

module logAnalytics '../modules/log-analytics.bicep' = {
  name: 'logAnalytics'
  params: {
    workspaceName: workspaceName
    location: location
    tags: tags
  }
}

module managedIdentity '../modules/managed-identity.bicep' = {
  name: 'managedIdentity'
  params: {
    identityName: identityName
    location: location
    tags: tags
  }
}

module aks '../modules/aks.bicep' = {
  name: 'aks'
  params: {
    clusterName: aksName
    location: location
    dnsPrefix: '${normalizedWorkload}-${environmentName}'
    kubernetesVersion: aksKubernetesVersion
    logAnalyticsWorkspaceResourceId: logAnalytics.outputs.id
    systemNodeCount: aksSystemNodeCount
    systemNodeVmSize: aksSystemNodeVmSize
    enableAutoScaling: aksEnableAutoScaling
    minNodeCount: aksMinNodeCount
    maxNodeCount: aksMaxNodeCount
    tags: tags
  }
}

output aksClusterName string = aks.outputs.name
output managedIdentityPrincipalId string = managedIdentity.outputs.principalId
output managedIdentityClientId string = managedIdentity.outputs.clientId
output oidcIssuerUrl string = aks.outputs.oidcIssuerUrl
output kubeletObjectId string = aks.outputs.kubeletObjectId
output csiDriverObjectId string = aks.outputs.csiDriverObjectId
output csiDriverClientId string = aks.outputs.csiDriverClientId
