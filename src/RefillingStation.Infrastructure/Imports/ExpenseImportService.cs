using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Imports;
using RefillingStation.Application.Interfaces.Services.Helpers;
using RefillingStation.Application.Interfaces.Services.Imports;
using RefillingStation.Domain.Entities;
using RefillingStation.Infrastructure.Imports.Shared;
using RefillingStation.Infrastructure.Imports.Shared.Mappings;
using RefillingStation.Infrastructure.Persistence;


namespace RefillingStation.Infrastructure.Imports
{
    public class ExpenseImportService : IExpenseImportService
    {
        private readonly AppDbContext _context;

        public ExpenseImportService(AppDbContext context)
        {
            _context = context;
        }

        // STEP 1: Parsed row (validated + typed, but still contains CategoryName)
        private sealed class ParsedExpenseRow
        {
            public DateOnly Date { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string? Notes { get; set; }
        }

        // STEP 2: Normalized row (FK resolved, ready for domain)
        private sealed class NormalizedExpenseRow
        {
            public DateOnly Date { get; set; }
            public int CategoryId { get; set; }
            public decimal Amount { get; set; }
            public string? Notes { get; set; }
        }

        public async Task<ImportResult> ImportAsync(Stream stream)
        {
            var result = new ImportResult();

            // Raw CSV → ExpenseImportRowRequest
            var csvRows = CsvParser.Parse(stream, new ExpenseImportRowRequestMap());
            result.TotalRows = csvRows.Count;

            var normalizedRows = new List<(int RowNumber, NormalizedExpenseRow Row)>();

            // Load FK lookup once
            var categoryLookup = await _context.ExpenseCategories
                .ToDictionaryAsync(
                    x => x.Name.Trim().ToLower(),
                    x => x.Id
                );

            for (int i = 0; i < csvRows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var csvRow = csvRows[i];

                try
                {
                    // STEP 1: Parse CSV → ParsedExpenseRow
                    var parsed = ParseCsvRow(csvRow);

                    // STEP 2: Normalize → NormalizedExpenseRow
                    var normalized = NormalizeParsedRow(parsed, categoryLookup);

                    normalizedRows.Add((rowNumber, normalized));
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = rowNumber,
                        Message = ex.Message
                    });
                }
            }

            // If errors exist, stop before DB insert
            if (result.Errors.Any())
            {
                result.FailedRows = result.Errors.Count;
                return result;
            }

            // STEP 3: Convert normalized rows → domain entities
            var expenses = normalizedRows.Select(x => new Expense
            {
                Date = x.Row.Date,
                Amount = x.Row.Amount,
                Notes = x.Row.Notes,
                ExpenseCategoryId = x.Row.CategoryId
            });

            _context.Expenses.AddRange(expenses);
            await _context.SaveChangesAsync();

            result.InsertedRows = normalizedRows.Count;
            return result;
        }

        // STEP 1: Parse CSV row → ParsedExpenseRow
        private ParsedExpenseRow ParseCsvRow(ExpenseImportRowRequest row)
        {
            var date = ImportParsingHelpers.ParseRequiredDate(row.Date, "Date");

            return new ParsedExpenseRow
            {
                Date = date,
                CategoryName = ImportParsingHelpers.ParseRequiredString(row.ExpenseCategory, "Category"),
                Amount = ImportParsingHelpers.ParseNonNegativeDecimal(row.Amount, "Amount"),
                Notes = ImportParsingHelpers.ParseOptionalString(row.Notes)
            };
        }

        // STEP 2: Normalize parsed row → NormalizedExpenseRow
        private NormalizedExpenseRow NormalizeParsedRow(
            ParsedExpenseRow parsed,
            Dictionary<string, int> categoryLookup)
        {
            var key = parsed.CategoryName.Trim().ToLower();

            if (!categoryLookup.TryGetValue(key, out var categoryId))
                throw new Exception($"Category '{parsed.CategoryName}' does not exist.");

            return new NormalizedExpenseRow
            {
                Date = parsed.Date,
                Amount = parsed.Amount,
                Notes = parsed.Notes,
                CategoryId = categoryId
            };
        }
    }
}
