using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "citext", maxLength: 15, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""Employees"" (""FirstName"", ""LastName"", ""Role"", ""IsActive"")
                SELECT DISTINCT
                    split_part(""EmployeeName"", ' ', 1) AS ""FirstName"",
                    COALESCE(NULLIF(split_part(""EmployeeName"", ' ', 2), ''), '') AS ""LastName"",
                    2 AS ""Role"",
                    true AS ""IsActive""
                FROM ""Trips""
                WHERE ""EmployeeName"" IS NOT NULL AND ""EmployeeName"" <> '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");
        }
    }
}
