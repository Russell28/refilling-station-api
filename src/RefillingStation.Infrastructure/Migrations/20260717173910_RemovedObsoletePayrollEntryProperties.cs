using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedObsoletePayrollEntryProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashPaid",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "PayrollEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashPaid",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PaidDate",
                table: "PayrollEntries",
                type: "date",
                nullable: true);
        }
    }
}
