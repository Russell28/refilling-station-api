using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRoleToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a temporary int column
            migrationBuilder.AddColumn<int>(
                name: "RoleInt",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Map string values to int
            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""RoleInt"" = 1
                WHERE ""Role"" = 'Admin';
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""RoleInt"" = 2
                WHERE ""Role"" = 'Employee';
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""RoleInt"" = 3
                WHERE ""Role"" = 'ReadOnly';
            ");

            // Drop old column
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            // Rename new column
            migrationBuilder.RenameColumn(
                name: "RoleInt",
                table: "Users",
                newName: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
