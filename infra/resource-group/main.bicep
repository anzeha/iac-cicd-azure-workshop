targetScope = 'resourceGroup'

param location string = resourceGroup().location
param environmentName string
param workloadName string = 'nlb-workshop'
param tags object = {}
param eventHubName string = 'orders'
param eventHubConsumerGroups array = [
  'projection'
  'replay'
]
param checkpointContainerName string = 'checkpoints'
param storageQueueName string = 'workshop-queue'
param serviceBusQueueName string = 'orders'
param serviceBusTopicName string = 'workshop-events'
param sqlAdminLogin string = 'sqladminuser'
@secure()
param sqlAdminPassword string
param sqlDatabaseName string = 'nlb-workshop'
param aksKubernetesVersion string = '1.34'
param aksSystemNodeCount int = 1
param aksSystemNodeVmSize string = 'Standard_D2s_v6'
param aksEnableAutoScaling bool = true
param aksMinNodeCount int = 1
param aksMaxNodeCount int = 3

var normalizedWorkload = toLower(replace(workloadName, '-', ''))
var uniqueSuffix = take(uniqueString(subscription().subscriptionId, resourceGroup().name), 6)
var namePrefix = '${workloadName}-${environmentName}'
var acrName = toLower(take('${normalizedWorkload}${environmentName}${uniqueSuffix}acr', 50))
var keyVaultName = take('${workloadName}-${uniqueSuffix}3-kv', 24)
var identityName = '${namePrefix}-workload-id'
var storageAccountName = toLower(take('${normalizedWorkload}${environmentName}${uniqueSuffix}st', 24))
var eventHubNamespaceName = take('${namePrefix}-${uniqueSuffix}-evh', 50)
var serviceBusNamespaceName = take('${namePrefix}-${uniqueSuffix}-sbus', 50)
var sqlServerName = toLower(take('${namePrefix}-${uniqueSuffix}-sql', 63))
var aksName = '${namePrefix}-aks'

var workloadNamespace = 'workshop-${environmentName}'
var workloadServiceAccountName = 'workshop-app-${environmentName}-workshop-app'
var workloadIdentitySubject = 'system:serviceaccount:${workloadNamespace}:${workloadServiceAccountName}'

var keyVaultSecretsUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

module existingCluster './existing.bicep' = {
  name: 'existingCluster'
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    aksKubernetesVersion: aksKubernetesVersion
    aksSystemNodeCount: aksSystemNodeCount
    aksSystemNodeVmSize: aksSystemNodeVmSize
    aksEnableAutoScaling: aksEnableAutoScaling
    aksMinNodeCount: aksMinNodeCount
    aksMaxNodeCount: aksMaxNodeCount
    workloadName: workloadName
  }
}

module containerRegistry '../modules/container-registry.bicep' = {
  name: 'containerRegistry'
  params: {
    registryName: acrName
    location: location
    tags: tags
  }
}

module keyVault '../modules/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    vaultName: keyVaultName
    location: location
    tenantId: subscription().tenantId
    tags: tags
  }
}

module storage '../modules/storage-account.bicep' = {
  name: 'storage'
  params: {
    storageAccountName: storageAccountName
    location: location
    blobContainerName: checkpointContainerName
    queueName: storageQueueName
    tags: tags
  }
}

module eventHubs '../modules/event-hubs.bicep' = {
  name: 'eventHubs'
  params: {
    namespaceName: eventHubNamespaceName
    location: location
    eventHubName: eventHubName
    consumerGroups: eventHubConsumerGroups
    tags: tags
  }
}

module serviceBus '../modules/service-bus.bicep' = {
  name: 'serviceBus'
  params: {
    namespaceName: serviceBusNamespaceName
    location: location
    queueName: serviceBusQueueName
    topicName: serviceBusTopicName
    tags: tags
  }
}

module sqlServer '../modules/sql-server.bicep' = {
  name: 'sqlServer'
  params: {
    serverName: sqlServerName
    databaseName: sqlDatabaseName
    location: location
    adminLogin: sqlAdminLogin
    adminPassword: sqlAdminPassword
    tags: tags
  }
}

resource keyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource workloadIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}

resource acrRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

resource keyVaultSecretsUserAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, keyVaultName, identityName, 'KeyVaultSecretsUser')
  scope: keyVaultResource
  properties: {
    principalId: existingCluster.outputs.managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsUserRoleId
  }
}

resource csiDriverKeyVaultSecretsUserAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, keyVaultName, 'csiDriver', 'KeyVaultSecretsUser')
  scope: keyVaultResource
  properties: {
    principalId: existingCluster.outputs.csiDriverObjectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsUserRoleId
  }
}

resource workloadIdentityFederation 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: workloadIdentity
  name: 'aks-${environmentName}-workshop-app'
  properties: {
    audiences: [
      'api://AzureADTokenExchange'
    ]
    issuer: existingCluster.outputs.oidcIssuerUrl
    subject: workloadIdentitySubject
  }
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, acrName, aksName, 'AcrPull')
  scope: acrRegistry
  properties: {
    principalId: existingCluster.outputs.kubeletObjectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPullRoleId
  }
}

var storageConnectionString = storage.outputs.primaryConnectionString
var eventHubsConnectionString = eventHubs.outputs.primaryConnectionString
var serviceBusConnectionString = serviceBus.outputs.primaryConnectionString
var sqlConnectionString = 'Server=tcp:${sqlServer.outputs.fullyQualifiedDomainName},1433;Initial Catalog=${sqlServer.outputs.databaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

resource readModelSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultResource
  name: 'read-model-connection-string'
  properties: {
    value: sqlConnectionString
  }
}

resource eventHubsSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultResource
  name: 'eventhubs-connection-string'
  properties: {
    value: eventHubsConnectionString
  }
}

resource checkpointStorageSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultResource
  name: 'blob-checkpoint-connection-string'
  properties: {
    value: storageConnectionString
  }
}

resource serviceBusSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultResource
  name: 'servicebus-connection-string'
  properties: {
    value: serviceBusConnectionString
  }
}

output aksClusterName string = existingCluster.outputs.aksClusterName
output managedIdentityClientId string = existingCluster.outputs.managedIdentityClientId
output aksClusterResourceGroup string = resourceGroup().name
output acrName string = containerRegistry.outputs.name
output acrLoginServer string = containerRegistry.outputs.loginServer
output keyVaultName string = keyVault.outputs.name
output storageAccountName string = storage.outputs.name
output blobContainerName string = storage.outputs.blobContainerName
output sqlServerName string = sqlServer.outputs.serverName
output sqlDatabaseName string = sqlServer.outputs.databaseName
output eventHubNamespaceName string = eventHubs.outputs.namespaceName
output eventHubName string = eventHubs.outputs.eventHubName
output serviceBusNamespaceName string = serviceBus.outputs.namespaceName
output serviceBusQueueName string = serviceBus.outputs.queueName
output serviceBusTopicName string = serviceBus.outputs.topicName
output workloadIdentitySecretNames object = {
  readModelConnectionString: readModelSecret.name
  eventHubsConnectionString: eventHubsSecret.name
  checkpointStorageConnectionString: checkpointStorageSecret.name
  serviceBusConnectionString: serviceBusSecret.name
}
output workloadIdentityBinding object = {
  namespace: workloadNamespace
  serviceAccountName: workloadServiceAccountName
  subject: workloadIdentitySubject
  federatedCredentialName: workloadIdentityFederation.name
}
