using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidQtyActualToTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TripNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TripType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Employee = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CustomerType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CollectedQty = table.Column<decimal>(type: "TEXT", nullable: false),
                    DeliveredQty = table.Column<decimal>(type: "TEXT", nullable: false),
                    FreeQty = table.Column<decimal>(type: "TEXT", nullable: false),
                    ToBePaidQty = table.Column<decimal>(type: "TEXT", nullable: false),
                    PricePerGallon = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimatedCash = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaidQtyActual = table.Column<decimal>(type: "TEXT", nullable: false),
                    CashCollectedActual = table.Column<decimal>(type: "TEXT", nullable: false),
                    CashUsed = table.Column<decimal>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
