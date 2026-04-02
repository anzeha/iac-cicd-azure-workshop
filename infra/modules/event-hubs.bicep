param namespaceName string
param location string
param eventHubName string
param consumerGroups array = [
  'projection'
  'replay'
]
param authorizationRuleName string = 'workshop-app'
param tags object = {}

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2024-01-01' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    publicNetworkAccess: 'Enabled'
    minimumTlsVersion: '1.2'
    zoneRedundant: false
    kafkaEnabled: true
    isAutoInflateEnabled: false
  }
}

resource namespaceAuthorizationRule 'Microsoft.EventHub/namespaces/AuthorizationRules@2024-01-01' = {
  parent: eventHubNamespace
  name: authorizationRuleName
  properties: {
    rights: [
      'Listen'
      'Send'
      'Manage'
    ]
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = {
  parent: eventHubNamespace
  name: eventHubName
  properties: {
    messageRetentionInDays: 1
    partitionCount: 4
    status: 'Active'
  }
}

resource eventHubConsumerGroups 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = [for consumerGroupName in consumerGroups: {
  parent: eventHub
  name: consumerGroupName
  properties: {}
}]

output namespaceName string = eventHubNamespace.name
output namespaceId string = eventHubNamespace.id
output eventHubName string = eventHub.name
output authorizationRuleId string = namespaceAuthorizationRule.id
output authorizationRuleName string = namespaceAuthorizationRule.name
output primaryConnectionString string = listKeys(namespaceAuthorizationRule.id, '2024-01-01').primaryConnectionString
