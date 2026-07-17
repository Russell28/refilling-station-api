using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    PaidDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_EmployeeId",
                table: "PayrollPayments",
                column: "EmployeeId");

            // Populate with existing data from PayrollEntries table
            migrationBuilder.Sql(@"
                INSERT INTO ""PayrollPayments"" 
                (
                    ""EmployeeId"", 
                    ""PaidDate"", 
                    ""AmountPaid"",
                    ""Notes""
                )
                SELECT 
                    ""EmployeeId"",
                    ""PaidDate"",
                    ""CashPaid"",
                    ""Notes""
                FROM ""PayrollEntries""
                WHERE ""PaidDate"" IS NOT NULL
                AND ""CashPaid"" > 0
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollPayments");
        }
    }
}
