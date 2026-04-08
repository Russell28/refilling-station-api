using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class MonthlyClosing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonthlyClosings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Month = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    TotalCashCollected = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPayrollEarned = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManagerShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OwnerShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyClosings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyClosings_Month",
                table: "MonthlyClosings",
                column: "Month",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyClosings");
        }
    }
}
