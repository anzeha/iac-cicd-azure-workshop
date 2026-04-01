using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nlb.Workshop.Infrastructure.Data;

public sealed class WorkshopDbContextFactory : IDesignTimeDbContextFactory<WorkshopDbContext>
{
  public WorkshopDbContext CreateDbContext(string[] args)
  {
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ReadModel");
    var optionsBuilder = new DbContextOptionsBuilder<WorkshopDbContext>();

    optionsBuilder.UseSqlServer(
      connectionString ??
      "Server=tcp:localhost,1433;Initial Catalog=nlb-workshop;User ID=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False;");

    return new WorkshopDbContext(optionsBuilder.Options);
  }
}
