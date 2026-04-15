using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerDebtEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    RelatedTripId = table.Column<int>(type: "integer", nullable: true),
                    EntryType = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDebtEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpenseCategory = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyClosings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Month = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    TotalCashCollected = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPayrollEarned = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetProfit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ManagerShare = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OwnerShare = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyClosings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmployeeName = table.Column<string>(type: "text", nullable: false),
                    SalaryAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AdvanceGiven = table.Column<decimal>(type: "numeric", nullable: false),
                    AdvanceDeduction = table.Column<decimal>(type: "numeric", nullable: false),
                    CashPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    TripNumber = table.Column<int>(type: "integer", nullable: false),
                    TimeStarted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeEnded = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmployeeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TripType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CollectedQty = table.Column<decimal>(type: "numeric", nullable: false),
                    LoadedQty = table.Column<decimal>(type: "numeric", nullable: false),
                    DeliveredQty = table.Column<decimal>(type: "numeric", nullable: false),
                    FreeQty = table.Column<decimal>(type: "numeric", nullable: false),
                    ReturnedQty = table.Column<decimal>(type: "numeric", nullable: false),
                    ReplacementQty = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualCashCollected = table.Column<decimal>(type: "numeric", nullable: false),
                    IsRemitted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyClosings_Month",
                table: "MonthlyClosings",
                column: "Month",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_Date_TripNumber",
                table: "Trips",
                columns: new[] { "Date", "TripNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDebtEntries");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "MonthlyClosings");

            migrationBuilder.DropTable(
                name: "PayrollEntries");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
