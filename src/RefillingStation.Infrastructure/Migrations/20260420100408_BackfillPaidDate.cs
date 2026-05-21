using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPaidDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""PayrollEntries""
                SET ""PaidDate"" = ""EarnedDate""
                WHERE ""CashPaid"" > 0
                AND ""PaidDate"" IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""PayrollEntries""
                SET ""PaidDate"" = NULL
            ");
        }
    }
}
