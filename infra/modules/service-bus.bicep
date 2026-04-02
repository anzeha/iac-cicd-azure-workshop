param namespaceName string
param location string
param queueName string
param topicName string
param authorizationRuleName string = 'workshop-app'
param tags object = {}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource namespaceAuthorizationRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2024-01-01' = {
  parent: serviceBusNamespace
  name: authorizationRuleName
  properties: {
    rights: [
      'Listen'
      'Send'
      'Manage'
    ]
  }
}

resource queue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    lockDuration: 'PT1M'
    maxDeliveryCount: 10
    requiresSession: false
  }
}

resource topic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: serviceBusNamespace
  name: topicName
  properties: {
    defaultMessageTimeToLive: 'P7D'
    enablePartitioning: true
  }
}

output namespaceName string = serviceBusNamespace.name
output namespaceId string = serviceBusNamespace.id
output queueName string = queue.name
output topicName string = topic.name
output authorizationRuleId string = namespaceAuthorizationRule.id
output primaryConnectionString string = listKeys(namespaceAuthorizationRule.id, '2024-01-01').primaryConnectionString
