using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefillingStation.Api.Migrations
{
    public partial class PopulateCustomers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert trimmed distinct non-empty customer names and update CustomerId on debt entries
            migrationBuilder.Sql(@"WITH distinct_names AS (
  SELECT DISTINCT TRIM(""CustomerName"") AS name
  FROM ""CustomerDebtEntries""
  WHERE ""CustomerName"" IS NOT NULL AND TRIM(""CustomerName"") <> ''
)
INSERT INTO ""Customers"" (""Name"")
SELECT name FROM distinct_names
ON CONFLICT (""Name"") DO NOTHING;

UPDATE ""CustomerDebtEntries"" cde
SET ""CustomerId"" = cu.""Id""
FROM ""Customers"" cu
WHERE TRIM(cde.""CustomerName"") = cu.""Name"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert CustomerId assignments and remove any customers not referenced by debt entries
            migrationBuilder.Sql(@"UPDATE ""CustomerDebtEntries"" SET ""CustomerId"" = NULL;

DELETE FROM ""Customers""
WHERE NOT EXISTS (
  SELECT 1 FROM ""CustomerDebtEntries"" cde WHERE cde.""CustomerId"" = ""Customers"".""Id""
);");
        }
    }
}
