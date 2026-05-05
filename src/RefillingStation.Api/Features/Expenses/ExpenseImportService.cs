using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using RefillingStation.Api.Common.Import;
using RefillingStation.Api.Common.Utilities;
using RefillingStation.Api.Data;
using RefillingStation.Domain.Entities;
using RefillingStation.Api.Features.Expenses.dtos;
using System.Globalization;

namespace RefillingStation.Api.Features.Expenses
{
    public class ExpenseImportService
    {
        private readonly AppDbContext _db;
        public ExpenseImportService(AppDbContext db)
        {
            _db = db;
        }

        private sealed class ParsedExpenseRow
        {
            public int RowNumber { get; set; }
            public Expense Expense { get; set; } = null!;
        }

        private sealed class ParsedExpense : CreateExpenseRequest 
        {
            public string ExpenseCategory { get; set; } = string.Empty;
        }

        public async Task<ImportResult> ImportAsync(IFormFile file)
        {
            var result = new ImportResult();

            if (file == null || file.Length == 0)
            {
                result.Errors.Add(new ImportError
                {
                    RowNumber = 0,
                    Message = "No file uploaded."
                });

                return result;
            }

            var parsedRows = new List<ParsedExpenseRow>();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            });

            var rows = csv.GetRecords<ExpenseImportRowDto>().ToList();
            result.TotalRows = rows.Count;

            var categories = await _db.ExpenseCategories
                .ToDictionaryAsync(x => x.Name, x => x.Id);

            for (int i = 0; i < rows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var row = rows[i];

                try
                {
                    var parsedExpense = MapRowToExpense(row);
                    var expense = new Expense
                    {
                        Date = parsedExpense.Date,
                        Amount = parsedExpense.Amount,
                        Notes = parsedExpense.Notes,
                        ExpenseCategoryId = categories[parsedExpense.ExpenseCategory]
                    };

                    parsedRows.Add(new ParsedExpenseRow
                    {
                        RowNumber = rowNumber,
                        Expense = expense,
                    });
                }
                catch (Exception ex) 
                {
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = rowNumber,
                        Message = ex.Message,
                    });
                }
            }

            if (result.Errors.Any())
            {
                result.InsertedRows = 0;
                result.FailedRows = result.Errors
                    .Select(x => x.RowNumber)
                    .Distinct()
                    .Count();

                return result;
            }

            _db.Expenses.AddRange(parsedRows.Select(x => x.Expense));
            await _db.SaveChangesAsync();

            result.InsertedRows = parsedRows.Count;
            result.FailedRows = result.Errors.Count;

            return result;
        }

        private ParsedExpense MapRowToExpense(ExpenseImportRowDto row)
        {
            var date = InputParser.ParseRequiredDate(row.Date, "Date");

            return new ParsedExpense
            {
                Date = date.ToDateTime(TimeOnly.MinValue), // Convert dateonly to datetime midnight
                ExpenseCategory = InputParser.ParseRequiredString(row.ExpenseCategory, "Category"),
                Amount = InputParser.ParseNonNegativeDecimal(row.Amount, "Amount"),
                Notes = InputParser.ParseOptionalString(row.Notes)
            };
        }


    }
}
