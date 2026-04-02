param clusterName string
param location string
param dnsPrefix string
param kubernetesVersion string = '1.34'
param logAnalyticsWorkspaceResourceId string
param systemNodeCount int = 2
param systemNodeVmSize string = 'Standard_D2s_v6'
param enableAutoScaling bool = true
param minNodeCount int = 1
param maxNodeCount int = 3
param tags object = {}

resource cluster 'Microsoft.ContainerService/managedClusters@2024-02-01' = {
  name: clusterName
  location: location
  tags: tags
  sku: {
    name: 'Base'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: dnsPrefix
    kubernetesVersion: kubernetesVersion

    oidcIssuerProfile: {
      enabled: true
    }

    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }

    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticsWorkspaceResourceId
        }
      }
      azureKeyvaultSecretsProvider: {
        enabled: true
      }
    }

    agentPoolProfiles: [
      {
        name: 'system'
        mode: 'System'
        count: systemNodeCount
        vmSize: systemNodeVmSize
        enableAutoScaling: enableAutoScaling
        minCount: minNodeCount
        maxCount: maxNodeCount
        osType: 'Linux'
        osSKU: 'Ubuntu'
        type: 'VirtualMachineScaleSets'
      }
    ]

    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      loadBalancerSku: 'standard'
    }
  }
}

output name string = cluster.name
output id string = cluster.id
output oidcIssuerUrl string = cluster.properties.oidcIssuerProfile.issuerURL
output kubeletObjectId string = cluster.properties.identityProfile.kubeletidentity.objectId
output csiDriverObjectId string = cluster.properties.addonProfiles.azureKeyvaultSecretsProvider.identity.objectId
output csiDriverClientId string = cluster.properties.addonProfiles.azureKeyvaultSecretsProvider.identity.clientId
