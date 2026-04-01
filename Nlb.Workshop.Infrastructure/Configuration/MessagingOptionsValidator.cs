using Microsoft.Extensions.Options;
using Nlb.Workshop.Infrastructure.Options;

namespace Nlb.Workshop.Infrastructure.Configuration;

public sealed class MessagingOptionsValidator : IValidateOptions<MessagingOptions>
{
  public ValidateOptionsResult Validate(string? name, MessagingOptions options)
  {
    if (!string.Equals(options.Provider, "EventHubs", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(options.Provider, "Kafka", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(options.Provider, "AzureServiceBus", StringComparison.OrdinalIgnoreCase))
    {
      return ValidateOptionsResult.Fail("Messaging:Provider must be either 'EventHubs' or 'Kafka' or 'AzureServiceBus'.");
    }

    if (!string.Equals(options.Serialization.DefaultFormat, "json", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(options.Serialization.DefaultFormat, "avro", StringComparison.OrdinalIgnoreCase))
    {
      return ValidateOptionsResult.Fail("Messaging:Serialization:DefaultFormat must be either 'json' or 'avro'.");
    }

    if (string.Equals(options.Provider, "EventHubs", StringComparison.OrdinalIgnoreCase))
    {
      if (string.IsNullOrWhiteSpace(options.EventHubs.ConnectionString))
        return ValidateOptionsResult.Fail("Messaging:EventHubs:ConnectionString is required when Messaging:Provider is EventHubs.");

      if (string.IsNullOrWhiteSpace(options.EventHubs.EventHubName))
        return ValidateOptionsResult.Fail("Messaging:EventHubs:EventHubName is required when Messaging:Provider is EventHubs.");

      if (string.IsNullOrWhiteSpace(options.EventHubs.ConsumerGroup) ||
          string.IsNullOrWhiteSpace(options.EventHubs.ReplayConsumerGroup))
      {
        return ValidateOptionsResult.Fail("Messaging:EventHubs consumer groups must be configured.");
      }

      if (string.IsNullOrWhiteSpace(options.EventHubs.CheckpointBlobConnectionString) ||
          string.IsNullOrWhiteSpace(options.EventHubs.CheckpointContainerName))
      {
        return ValidateOptionsResult.Fail("Event Hubs checkpoint blob storage settings must be configured.");
      }
    }
    else if (string.Equals(options.Provider, "Kafka", StringComparison.OrdinalIgnoreCase))
    {
      if (string.IsNullOrWhiteSpace(options.Kafka.BootstrapServers))
        return ValidateOptionsResult.Fail("Messaging:Kafka:BootstrapServers is required when Messaging:Provider is Kafka.");

      if (string.IsNullOrWhiteSpace(options.Kafka.Topic))
        return ValidateOptionsResult.Fail("Messaging:Kafka:Topic is required when Messaging:Provider is Kafka.");

      if (string.IsNullOrWhiteSpace(options.Kafka.ConsumerGroup) ||
          string.IsNullOrWhiteSpace(options.Kafka.ReplayConsumerGroup))
      {
        return ValidateOptionsResult.Fail("Messaging:Kafka consumer groups must be configured.");
      }
    }
    else
    {
      if (string.IsNullOrWhiteSpace(options.AzureServiceBus.ConnectionString))
      {
        return ValidateOptionsResult.Fail(
            "Messaging:AzureServiceBus:ConnectionString is required when Messaging:Provider is AzureServiceBus.");
      }

      if (string.IsNullOrWhiteSpace(options.AzureServiceBus.QueueName))
      {
        return ValidateOptionsResult.Fail(
            "Messaging:AzureServiceBus:QueueName is required when Messaging:Provider is AzureServiceBus.");
      }
    }

    return ValidateOptionsResult.Success;
  }
}
