using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_Name",
                table: "ExpenseCategories",
                column: "Name",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""ExpenseCategories"" (""Name"")
                SELECT DISTINCT TRIM(""ExpenseCategory"")
                FROM ""Expenses""
                WHERE ""ExpenseCategory"" IS NOT NULL
                  AND TRIM(""ExpenseCategory"") <> '';
            ");

            migrationBuilder.Sql(@"
                UPDATE ""ExpenseCategories"" SET ""SortOrder"" = 1 WHERE ""Name"" = 'Gas';
                UPDATE ""ExpenseCategories"" SET ""SortOrder"" = 2 WHERE ""Name"" = 'Snacks';
                UPDATE ""ExpenseCategories"" SET ""SortOrder"" = 3 WHERE ""Name"" = 'Lunch';
                UPDATE ""ExpenseCategories"" SET ""SortOrder"" = 4 WHERE ""Name"" = 'Bike Maintenance';
                UPDATE ""ExpenseCategories"" SET ""SortOrder"" = 5 WHERE ""Name"" = 'Supplies';
                UPDATE ""ExpenseCategories"" SET ""SortOrder"" = 999 WHERE ""Name"" = 'Other';
            ");

            migrationBuilder.Sql(@"
                UPDATE ""ExpenseCategories""
                SET ""SortOrder"" = 50
                WHERE ""SortOrder"" IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ExpenseCategories",
                nullable: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseCategories");
        }
    }
}
