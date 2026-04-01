using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nlb.Workshop.Infrastructure.Data;

namespace Nlb.Workshop.Infrastructure.HealthChecks;

public sealed class ReadModelHealthCheck : IHealthCheck
{
  private readonly IDbContextFactory<WorkshopDbContext> _dbContextFactory;

  public ReadModelHealthCheck(IDbContextFactory<WorkshopDbContext> dbContextFactory)
  {
    _dbContextFactory = dbContextFactory;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
  {
    try
    {
      await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
      var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

      return canConnect
        ? HealthCheckResult.Healthy("Read-model database connection succeeded.")
        : HealthCheckResult.Unhealthy("Read-model database connection failed.");
    }
    catch (Exception exception)
    {
      return HealthCheckResult.Unhealthy("Read-model database connection failed.", exception);
    }
  }
}
