using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add as nullable first to avoid issues with existing data, then we can make it non-nullable if needed
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "CustomerDebtEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDebtEntries_CustomerId",
                table: "CustomerDebtEntries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerDebtEntries_Customers_CustomerId",
                table: "CustomerDebtEntries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Populate Customers table with distinct customer names from existing CustomerDebtEntries
            migrationBuilder.Sql(@"
                INSERT INTO ""Customers"" (""Name"")
                SELECT DISTINCT ""CustomerName""
                FROM ""CustomerDebtEntries""
                WHERE ""CustomerName"" IS NOT NULL;
            ");

            // Populate CustomerId in CustomerDebtEntries based on the newly created Customers table
            migrationBuilder.Sql(@"
                UPDATE ""CustomerDebtEntries"" cde
                SET ""CustomerId"" = cu.""Id""
                FROM ""Customers"" cu
                WHERE cde.""CustomerName"" = cu.""Name"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDebtEntries_Customers_CustomerId",
                table: "CustomerDebtEntries");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDebtEntries_CustomerId",
                table: "CustomerDebtEntries");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "CustomerDebtEntries");
        }
    }
}
