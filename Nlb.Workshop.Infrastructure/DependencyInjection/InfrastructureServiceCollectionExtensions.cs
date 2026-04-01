using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nlb.Workshop.Application.Interfaces;
using Nlb.Workshop.Infrastructure.Configuration;
using Nlb.Workshop.Infrastructure.Data;
using Nlb.Workshop.Infrastructure.HealthChecks;
using Nlb.Workshop.Infrastructure.Messaging.AzureServiceBus;
using Nlb.Workshop.Infrastructure.Messaging.EventHubs;
using Nlb.Workshop.Infrastructure.Messaging.Kafka;
using Nlb.Workshop.Infrastructure.Messaging.Partitioning;
using Nlb.Workshop.Infrastructure.Messaging.Serialization;
using Nlb.Workshop.Infrastructure.Options;
using Nlb.Workshop.Infrastructure.Repositories;
using Nlb.Workshop.Infrastructure.Replay;

namespace Nlb.Workshop.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
  public static IServiceCollection AddWorkshopInfrastructure(this IServiceCollection services,
      IConfiguration configuration)
  {
    var readModelConnectionString = configuration.GetConnectionString("ReadModel");
    if (string.IsNullOrWhiteSpace(readModelConnectionString))
      throw new InvalidOperationException("ConnectionStrings:ReadModel is required.");

    services
      .AddOptions<MessagingOptions>()
      .Bind(configuration.GetSection(MessagingOptions.SectionName))
      .ValidateOnStart();
    services.AddSingleton<IValidateOptions<MessagingOptions>, MessagingOptionsValidator>();

    services.AddPooledDbContextFactory<WorkshopDbContext>(options =>
      options.UseSqlServer(readModelConnectionString, sqlServerOptions =>
      {
        sqlServerOptions.MigrationsAssembly(typeof(WorkshopDbContext).Assembly.FullName);
        sqlServerOptions.EnableRetryOnFailure();
      }));

    services.AddSingleton<IProjectionRepository, EfProjectionRepository>();
    services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
    services.AddSingleton<IPartitionKeyResolver, OrderPartitionKeyResolver>();
    services.AddHealthChecks()
      .AddCheck<ReadModelHealthCheck>("read-model", tags: ["ready"]);

    services.AddSingleton<IEventSerializer, JsonEventSerializer>();
    services.AddSingleton<IEventSerializer, AvroEventSerializer>();
    services.AddSingleton<IEventSerializerResolver, EventSerializerResolver>();

    services.AddSingleton<IEventPublisher>(serviceProvider =>
    {
      var messagingOptions = serviceProvider.GetRequiredService<IOptions<MessagingOptions>>().Value;

      return messagingOptions.Provider switch
      {
        var p when p.Equals("Kafka", StringComparison.OrdinalIgnoreCase)
          => ActivatorUtilities.CreateInstance<KafkaEventPublisher>(serviceProvider),

        var p when p.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase)
          => ActivatorUtilities.CreateInstance<AzureServiceBusEventPublisher>(serviceProvider),

        _ => ActivatorUtilities.CreateInstance<EventHubsEventPublisher>(serviceProvider)
      };
    });

    services.AddSingleton<IEventConsumer>(serviceProvider =>
    {
      var messagingOptions = serviceProvider.GetRequiredService<IOptions<MessagingOptions>>().Value;

      return messagingOptions.Provider switch
      {
        var p when p.Equals("Kafka", StringComparison.OrdinalIgnoreCase)
          => ActivatorUtilities.CreateInstance<KafkaEventConsumer>(serviceProvider),

        var p when p.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase)
          => ActivatorUtilities.CreateInstance<AzureServiceBusEventConsumer>(serviceProvider),

        _ => ActivatorUtilities.CreateInstance<EventHubsEventConsumer>(serviceProvider)
      };
    });

    services.AddSingleton<IReplayCoordinator>(serviceProvider =>
    {
      var messagingOptions = serviceProvider.GetRequiredService<IOptions<MessagingOptions>>().Value;

      return messagingOptions.Provider switch
      {
        var p when p.Equals("Kafka", StringComparison.OrdinalIgnoreCase)
          => ActivatorUtilities.CreateInstance<KafkaReplayCoordinator>(serviceProvider),

        var p when p.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase)
          => throw new InvalidOperationException("Replay is not supported when Messaging:Provider is AzureServiceBus."),

        _ => ActivatorUtilities.CreateInstance<EventHubsReplayCoordinator>(serviceProvider)
      };
    });

    return services;
  }
}
