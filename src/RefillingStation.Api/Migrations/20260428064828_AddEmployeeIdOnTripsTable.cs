using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeIdOnTripsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Trips",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Trips"" t
                SET ""EmployeeId"" = e.""Id""
                FROM ""Employees"" e
                WHERE t.""EmployeeName"" = CONCAT(e.""FirstName"", ' ', e.""LastName"");
            ");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Trips",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_EmployeeId",
                table: "Trips",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Employees_EmployeeId",
                table: "Trips",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Employees_EmployeeId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_EmployeeId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Trips");
        }
    }
}
