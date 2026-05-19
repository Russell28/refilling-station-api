using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeIdOnPayrollTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "PayrollEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""PayrollEntries"" p
                SET ""EmployeeId"" = e.""Id""
                FROM ""Employees"" e
                WHERE p.""EmployeeName"" = CONCAT(e.""FirstName"", ' ', e.""LastName"");
            ");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "PayrollEntries",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_EmployeeId",
                table: "PayrollEntries",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                table: "PayrollEntries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                table: "PayrollEntries");

            migrationBuilder.DropIndex(
                name: "IX_PayrollEntries_EmployeeId",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "PayrollEntries");
        }
    }
}
