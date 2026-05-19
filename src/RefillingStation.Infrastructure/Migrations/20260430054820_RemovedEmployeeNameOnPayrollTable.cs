using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedEmployeeNameOnPayrollTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "PayrollEntries");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "PayrollEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "PayrollEntries",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "PayrollEntries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                table: "PayrollEntries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
