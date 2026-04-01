using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nlb.Workshop.Infrastructure.Data;

#nullable disable

namespace Nlb.Workshop.Infrastructure.Data.Migrations
{
  [DbContext(typeof(WorkshopDbContext))]
  [Migration("20260313150000_InitialReadModelSqlServer")]
  partial class InitialReadModelSqlServer
  {
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
      modelBuilder
        .HasAnnotation("ProductVersion", "10.0.0")
        .HasAnnotation("Relational:MaxIdentifierLength", 128);

      SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

      modelBuilder.Entity("Nlb.Workshop.Domain.Entities.OrderReadModel", b =>
        {
          b.Property<string>("OrderId")
            .HasColumnType("nvarchar(450)");

          b.Property<decimal>("Amount")
            .HasPrecision(18, 2)
            .HasColumnType("decimal(18,2)");

          b.Property<DateTimeOffset>("CreatedAt")
            .HasColumnType("datetimeoffset");

          b.Property<string>("Currency")
            .IsRequired()
            .HasColumnType("nvarchar(max)");

          b.Property<string>("CustomerId")
            .IsRequired()
            .HasColumnType("nvarchar(max)");

          b.Property<Guid>("LastEventId")
            .HasColumnType("uniqueidentifier");

          b.Property<int>("LastEventVersion")
            .HasColumnType("int");

          b.Property<string>("SourceSystem")
            .HasColumnType("nvarchar(max)");

          b.Property<DateTimeOffset>("UpdatedAt")
            .HasColumnType("datetimeoffset");

          b.HasKey("OrderId");

          b.ToTable("Orders");
        });

      modelBuilder.Entity("Nlb.Workshop.Domain.Entities.ProcessedEvent", b =>
        {
          b.Property<Guid>("EventId")
            .ValueGeneratedOnAdd()
            .HasColumnType("uniqueidentifier");

          b.Property<string>("EventType")
            .IsRequired()
            .HasMaxLength(450)
            .HasColumnType("nvarchar(450)");

          b.Property<long?>("Offset")
            .HasColumnType("bigint");

          b.Property<string>("PartitionId")
            .IsRequired()
            .HasMaxLength(450)
            .HasColumnType("nvarchar(450)");

          b.Property<DateTimeOffset>("ProcessedAt")
            .HasColumnType("datetimeoffset");

          b.Property<int>("Version")
            .HasColumnType("int");

          b.HasKey("EventId");

          b.HasIndex("EventType", "PartitionId", "Offset");

          b.ToTable("ProcessedEvents");
        });
#pragma warning restore 612, 618
    }
  }
}
