using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseCategoryIdToExpenseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpenseCategoryId",
                table: "Expenses",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Expenses"" e
                SET ""ExpenseCategoryId"" = ec.""Id""
                FROM ""ExpenseCategories"" ec
                WHERE TRIM(e.""ExpenseCategory"") = ec.""Name"";
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Expenses""
                SET ""ExpenseCategoryId"" = (SELECT ""Id"" FROM ""ExpenseCategories"" WHERE ""Name"" = 'Other')
                WHERE ""ExpenseCategoryId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ExpenseCategoryId",
                table: "Expenses",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpenseCategoryId",
                table: "Expenses");
        }
    }
}
