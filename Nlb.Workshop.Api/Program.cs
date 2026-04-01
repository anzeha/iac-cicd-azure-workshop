using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Nlb.Workshop.Application.Interfaces;
using Nlb.Workshop.Application.UseCases;
using Nlb.Workshop.Contracts.Api;
using Nlb.Workshop.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWorkshopApplication();
builder.Services.AddWorkshopInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();
const string WorkshopVersionHeaderValue = "1";

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
}

using (var scope = app.Services.CreateScope())
{
  var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
  await initializer.EnsureCreatedAsync();
}

app.Use(async (context, next) =>
{
  if (context.Request.Path.StartsWithSegments("/health"))
  {
    context.Response.OnStarting(() =>
    {
      context.Response.Headers["X-Workshop-Version"] = WorkshopVersionHeaderValue;
      return Task.CompletedTask;
    });
  }

  await next();
});

app.MapPost("/orders",
        async ([FromBody] CreateOrderRequest request,
            [FromServices] OrderCommandService commandService,
            CancellationToken cancellationToken) =>
        {
          try
          {
            var response = await commandService.PublishOrderAsync(request, cancellationToken);
            return Results.Created($"/read-model/orders/{response.OrderId}", response);
          }
          catch (ArgumentException ex)
          {
            return Results.BadRequest(new { error = ex.Message });
          }
        })
    .WithName("PublishOrder")
    .WithTags("Orders");

app.MapPost("/orders/bulk",
        async ([FromBody] CreateOrdersBulkRequest request,
            [FromServices] OrderCommandService commandService,
            CancellationToken cancellationToken) =>
        {
          var response = await commandService.PublishBulkAsync(request, cancellationToken);
          return Results.Ok(response);
        })
    .WithName("PublishOrdersBulk")
    .WithTags("Orders");

app.MapGet("/read-model/orders/{orderId}",
        async (string orderId,
            [FromServices] ReadModelQueryService queryService,
            CancellationToken cancellationToken) =>
        {
          var response = await queryService.GetOrderAsync(orderId, cancellationToken);
          return response is null ? Results.NotFound() : Results.Ok(response);
        })
    .WithName("GetOrderReadModel")
    .WithTags("Read Model");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
  Predicate = _ => false
})
    .WithName("Liveness")
    .WithTags("System");

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("ready")
})
    .WithName("Readiness")
    .WithTags("System");

app.MapHealthChecks("/health", new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("ready")
})
    .WithName("Health")
    .WithTags("System");

await app.RunAsync();
