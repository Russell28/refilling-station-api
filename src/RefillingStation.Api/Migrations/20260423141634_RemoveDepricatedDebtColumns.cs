using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDepricatedDebtColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "CustomerDebtEntries");

            migrationBuilder.DropColumn(
                name: "EntryType",
                table: "CustomerDebtEntries");

            migrationBuilder.DropColumn(
                name: "RelatedTripId",
                table: "CustomerDebtEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "CustomerDebtEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryType",
                table: "CustomerDebtEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedTripId",
                table: "CustomerDebtEntries",
                type: "integer",
                nullable: true);
        }
    }
}
