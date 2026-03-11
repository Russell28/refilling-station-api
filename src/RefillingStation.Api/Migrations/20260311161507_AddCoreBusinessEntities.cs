using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreBusinessEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaidQtyActual",
                table: "Trips",
                newName: "Segment");

            migrationBuilder.RenameColumn(
                name: "EstimatedCash",
                table: "Trips",
                newName: "ReturnedQty");

            migrationBuilder.RenameColumn(
                name: "Employee",
                table: "Trips",
                newName: "EmployeeName");

            migrationBuilder.RenameColumn(
                name: "CustomerType",
                table: "Trips",
                newName: "CustomerCategory");

            migrationBuilder.RenameColumn(
                name: "CashUsed",
                table: "Trips",
                newName: "ReplacementQty");

            migrationBuilder.RenameColumn(
                name: "CashCollectedActual",
                table: "Trips",
                newName: "LoadedQty");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCashCollected",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualPaidQty",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AdjustmentReason",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RelatedTripId",
                table: "Trips",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeEnded",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStarted",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerDebtEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    RelatedTripId = table.Column<int>(type: "INTEGER", nullable: true),
                    EntryType = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDebtEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpenseCategory = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EmployeeName = table.Column<string>(type: "TEXT", nullable: false),
                    SalaryAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AdvanceGiven = table.Column<decimal>(type: "TEXT", nullable: false),
                    AdvanceDeduction = table.Column<decimal>(type: "TEXT", nullable: false),
                    CashPaid = table.Column<decimal>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDebtEntries");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "ActualCashCollected",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ActualPaidQty",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "AdjustmentReason",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "RelatedTripId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TimeEnded",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TimeStarted",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "Segment",
                table: "Trips",
                newName: "PaidQtyActual");

            migrationBuilder.RenameColumn(
                name: "ReturnedQty",
                table: "Trips",
                newName: "EstimatedCash");

            migrationBuilder.RenameColumn(
                name: "ReplacementQty",
                table: "Trips",
                newName: "CashUsed");

            migrationBuilder.RenameColumn(
                name: "LoadedQty",
                table: "Trips",
                newName: "CashCollectedActual");

            migrationBuilder.RenameColumn(
                name: "EmployeeName",
                table: "Trips",
                newName: "Employee");

            migrationBuilder.RenameColumn(
                name: "CustomerCategory",
                table: "Trips",
                newName: "CustomerType");
        }
    }
}
