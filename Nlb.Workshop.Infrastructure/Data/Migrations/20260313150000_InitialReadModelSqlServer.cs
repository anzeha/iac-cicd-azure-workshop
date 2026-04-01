using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nlb.Workshop.Infrastructure.Data.Migrations;

public partial class InitialReadModelSqlServer : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "Orders",
      columns: table => new
      {
        OrderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
        CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
        Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
        Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        SourceSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
        LastEventVersion = table.Column<int>(type: "int", nullable: false),
        LastEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_Orders", x => x.OrderId);
      });

    migrationBuilder.CreateTable(
      name: "ProcessedEvents",
      columns: table => new
      {
        EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        EventType = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
        Version = table.Column<int>(type: "int", nullable: false),
        PartitionId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
        Offset = table.Column<long>(type: "bigint", nullable: true),
        ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_ProcessedEvents", x => x.EventId);
      });

    migrationBuilder.CreateIndex(
      name: "IX_ProcessedEvents_EventType_PartitionId_Offset",
      table: "ProcessedEvents",
      columns: ["EventType", "PartitionId", "Offset"]);
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(name: "Orders");
    migrationBuilder.DropTable(name: "ProcessedEvents");
  }
}
