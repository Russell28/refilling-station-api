using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTripModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustmentReason",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PricePerGallon",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ToBePaidQty",
                table: "Trips");

            migrationBuilder.AddColumn<bool>(
                name: "IsRemitted",
                table: "Trips",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemitted",
                table: "Trips");

            migrationBuilder.AddColumn<string>(
                name: "AdjustmentReason",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerGallon",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToBePaidQty",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
