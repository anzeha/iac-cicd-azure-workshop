using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nlb.Workshop.Application.Interfaces;
using Nlb.Workshop.Contracts.Events;
using Nlb.Workshop.Infrastructure.Messaging.Common;
using Nlb.Workshop.Infrastructure.Options;

namespace Nlb.Workshop.Infrastructure.Messaging.AzureServiceBus;

public sealed class AzureServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
  private const string DefaultPayloadFormat = "json";

  private readonly ServiceBusClient _client;
  private readonly ServiceBusSender _sender;
  private readonly IEventSerializerResolver _eventSerializerResolver;
  private readonly ILogger<AzureServiceBusEventPublisher> _logger;

  public AzureServiceBusEventPublisher(
      IOptions<MessagingOptions> messagingOptions,
      IEventSerializerResolver eventSerializerResolver,
      ILogger<AzureServiceBusEventPublisher> logger)
  {
    var options = messagingOptions.Value.AzureServiceBus;

    _client = new ServiceBusClient(
        options.ConnectionString,
        new ServiceBusClientOptions
        {
          TransportType = ServiceBusTransportType.AmqpTcp
        });

    _sender = _client.CreateSender(options.QueueName);
    _eventSerializerResolver = eventSerializerResolver;
    _logger = logger;
  }

  public async Task PublishAsync<TPayload>(
      EventEnvelope<TPayload> envelope,
      CancellationToken cancellationToken = default)
  {
    var serializer = _eventSerializerResolver.GetSerializer();
    var message = CreateServiceBusMessage(envelope, serializer);

    await _sender.SendMessageAsync(message, cancellationToken);

    _logger.LogInformation(
        "Published Azure Service Bus event {EventId} ({EventType} v{Version}) on key {PartitionKey}.",
        envelope.EventId,
        envelope.EventType,
        envelope.Version,
        envelope.PartitionKey);
  }

  public async Task PublishBatchAsync<TPayload>(
      IReadOnlyCollection<EventEnvelope<TPayload>> envelopes,
      CancellationToken cancellationToken = default)
  {
    foreach (var envelope in envelopes)
      await PublishAsync(envelope, cancellationToken);
  }

  public async ValueTask DisposeAsync()
  {
    await _sender.DisposeAsync();
    await _client.DisposeAsync();
  }

  public static ServiceBusMessage CreateServiceBusMessage<TPayload>(
      EventEnvelope<TPayload> envelope,
      IEventSerializer serializer)
  {
    var payload = serializer.Serialize(envelope);

    var payloadFormat = string.IsNullOrWhiteSpace(serializer.Format)
        ? DefaultPayloadFormat
        : serializer.Format;

    var message = new ServiceBusMessage(payload)
    {
      MessageId = envelope.EventId.ToString(),
      CorrelationId = envelope.CorrelationId,
      Subject = envelope.EventType,
      ContentType = payloadFormat
    };

    message.ApplicationProperties[EventHeaderNames.EventType] = envelope.EventType;
    message.ApplicationProperties[EventHeaderNames.EventVersion] = envelope.Version;
    message.ApplicationProperties[EventHeaderNames.CorrelationId] = envelope.CorrelationId ?? string.Empty;
    message.ApplicationProperties[EventHeaderNames.PartitionKey] = envelope.PartitionKey;
    message.ApplicationProperties[EventHeaderNames.PayloadFormat] = payloadFormat;

    return message;
  }
}