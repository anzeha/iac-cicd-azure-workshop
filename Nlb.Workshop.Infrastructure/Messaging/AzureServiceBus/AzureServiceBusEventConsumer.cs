using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nlb.Workshop.Application.Interfaces;
using Nlb.Workshop.Application.Models;
using Nlb.Workshop.Infrastructure.Messaging.Common;
using Nlb.Workshop.Infrastructure.Options;

namespace Nlb.Workshop.Infrastructure.Messaging.AzureServiceBus;

public sealed class AzureServiceBusEventConsumer : IEventConsumer, IAsyncDisposable
{
    private const string DefaultPayloadFormat = "json";

    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<AzureServiceBusEventConsumer> _logger;

    private Func<ConsumedEventContext, CancellationToken, Task>? _handleEventAsync;
    private bool _started;

    public AzureServiceBusEventConsumer(
        IOptions<MessagingOptions> messagingOptions,
        ILogger<AzureServiceBusEventConsumer> logger)
    {
        var options = messagingOptions.Value.AzureServiceBus;

        _client = new ServiceBusClient(
            options.ConnectionString,
            new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp
            });

        _processor = _client.CreateProcessor(
            options.QueueName,
            new ServiceBusProcessorOptions());

        _logger = logger;

        _processor.ProcessMessageAsync += OnProcessMessageAsync;
        _processor.ProcessErrorAsync += OnProcessErrorAsync;
    }

    public async Task StartAsync(
        Func<ConsumedEventContext, CancellationToken, Task> handleEventAsync,
        CancellationToken cancellationToken)
    {
        if (_started)
            return;

        _handleEventAsync = handleEventAsync;
        _started = true;

        await _processor.StartProcessingAsync(cancellationToken);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await _processor.StopProcessingAsync(CancellationToken.None);
            _started = false;
        }
    }

    private async Task OnProcessMessageAsync(ProcessMessageEventArgs args)
    {
        if (_handleEventAsync is null)
            throw new InvalidOperationException("Message handler is not configured.");

        var message = args.Message;

        var eventContext = new ConsumedEventContext(
            message.Body.ToArray(),
            GetStringProperty(message.ApplicationProperties, EventHeaderNames.EventType, message.Subject ?? "unknown"),
            GetIntProperty(message.ApplicationProperties, EventHeaderNames.EventVersion, 1),
            GetStringProperty(message.ApplicationProperties, EventHeaderNames.PartitionKey, string.Empty),
            string.Empty,
            null,
            GetStringProperty(message.ApplicationProperties, EventHeaderNames.PayloadFormat, DefaultPayloadFormat),
            GetNullableStringProperty(message.ApplicationProperties, EventHeaderNames.CorrelationId) ?? message.CorrelationId);

        await _handleEventAsync(eventContext, args.CancellationToken);
        await args.CompleteMessageAsync(message, args.CancellationToken);
    }

    private Task OnProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Azure Service Bus processor error. Entity: {EntityPath}, ErrorSource: {ErrorSource}.",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }

    private static string GetStringProperty(
        IReadOnlyDictionary<string, object> properties,
        string key,
        string fallback)
    {
        if (!properties.TryGetValue(key, out var value))
            return fallback;

        return value?.ToString() ?? fallback;
    }

    private static string? GetNullableStringProperty(
        IReadOnlyDictionary<string, object> properties,
        string key)
    {
        if (!properties.TryGetValue(key, out var value))
            return null;

        return value?.ToString();
    }

    private static int GetIntProperty(
        IReadOnlyDictionary<string, object> properties,
        string key,
        int fallback)
    {
        if (!properties.TryGetValue(key, out var value))
            return fallback;

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            string stringValue when int.TryParse(stringValue, out var parsed) => parsed,
            _ => fallback
        };
    }
}