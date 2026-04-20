using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayrollSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "PayrollEntries",
                newName: "EarnedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                table: "PayrollEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""PayrollEntries""
                SET ""CashPaid"" = ""CashPaid""
                + COALESCE(""AdvanceGiven"", 0) 
                - COALESCE(""AdvanceDeduction"", 0)
            ");

            migrationBuilder.DropColumn(
                name: "AdvanceDeduction",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "AdvanceGiven",
                table: "PayrollEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "PayrollEntries");

            migrationBuilder.RenameColumn(
                name: "EarnedDate",
                table: "PayrollEntries",
                newName: "Date");

            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceDeduction",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceGiven",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
