using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize timestamps to Manila time BEFORE converting to date
            migrationBuilder.Sql("""
                UPDATE "PayrollEntries"
                SET "PaidDate" = ("PaidDate" AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Manila')
                WHERE "PaidDate" IS NOT NULL;
            """);

                    migrationBuilder.Sql("""
                UPDATE "PayrollEntries"
                SET "EarnedDate" = ("EarnedDate" AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Manila');
            """);

                    migrationBuilder.Sql("""
                UPDATE "Expenses"
                SET "Date" = ("Date" AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Manila');
            """);

                    migrationBuilder.Sql("""
                UPDATE "CustomerDebtEntries"
                SET "Date" = ("Date" AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Manila');
            """);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PaidDate",
                table: "PayrollEntries",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EarnedDate",
                table: "PayrollEntries",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "Expenses",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "CustomerDebtEntries",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidDate",
                table: "PayrollEntries",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EarnedDate",
                table: "PayrollEntries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Expenses",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "CustomerDebtEntries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
