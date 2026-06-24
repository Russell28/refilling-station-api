using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add columns
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);

            // nullable
            migrationBuilder.AddColumn<int>(
                name: "EmploymentType",
                table: "Employees",
                type: "integer",
                nullable: true);

            // Step 2: Backfill set DeactivatedAt for inactive employees
            migrationBuilder.Sql(@"
                UPDATE ""Employees""
                SET ""DeactivatedAt"" = NOW()
                WHERE ""IsActive"" = FALSE
                  AND ""DeactivatedAt"" IS NULL;
            ");

            // Step 3: Backfill existing rows with default value (Permanent = 1)
            migrationBuilder.Sql(@"
                UPDATE ""Employees""
                SET ""EmploymentType"" = 1
                WHERE ""EmploymentType"" IS NULL;
            ");

            // Step 4: Alter column to be non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "EmploymentType",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 3,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Employees");
        }
    }
}
