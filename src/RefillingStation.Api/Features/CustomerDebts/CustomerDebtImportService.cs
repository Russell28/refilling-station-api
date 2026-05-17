using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using RefillingStation.Api.Common.Import;
using RefillingStation.Api.Common.Utilities;
using RefillingStation.Api.Data;
using RefillingStation.Domain.Entities;
using System.Globalization;
using RefillingStation.Application.DTOs.Imports;

namespace RefillingStation.Api.Features.CustomerDebts
{
    public class CustomerDebtImportService
    {
        private readonly AppDbContext _db;

        public CustomerDebtImportService(AppDbContext db)
        {
            _db = db;
        }

        private sealed class ParsedDebtRow
        {
            public int RowNumber { get; set; }
            public CustomerDebtEntry Debt { get; set; } = null!;
        }

        public async Task<ImportResult> ImportAsync(IFormFile file)
        {
            var result = new ImportResult();

            if (file == null || file.Length == 0)
            {
                result.Errors.Add(new ImportError
                {
                    RowNumber = 0,
                    Message = "No file uploaded"
                });
            }

            var parsedRows = new List<ParsedDebtRow>();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            });

            var rows = csv.GetRecords<CustomerDebtImportRowRequest>().ToList();
            result.TotalRows = rows.Count;

            var customers = await _db.Customers.ToListAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var row = rows[i];

                try
                {
                    var parsedDebt = MapRowToDebt(row);
                    var debt = new CustomerDebtEntry
                    {
                        Date = parsedDebt.Date,
                        CustomerId = customers.FirstOrDefault(c => c.Name == parsedDebt.CustomerName)?.Id
                            ?? throw new Exception($"Customer '{parsedDebt.CustomerName}' not found"),
                        Amount = parsedDebt.Amount,
                        Notes = parsedDebt.Notes,
                    };

                    parsedRows.Add(new ParsedDebtRow
                    {
                        RowNumber = rowNumber,
                        Debt = debt,
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

            _db.CustomerDebtEntries.AddRange(parsedRows.Select(x => x.Debt));
            await _db.SaveChangesAsync();

            result.InsertedRows = parsedRows.Count;
            result.FailedRows = result.Errors.Count;

            return result;
        }

        private ParsedDebt MapRowToDebt(CustomerDebtImportRowRequest row)
        {
            var date = InputParser.ParseRequiredDate(row.Date, "Date");

            return new ParsedDebt
            {
                Date = date.ToDateTime(TimeOnly.MinValue), // Convert dateonly to datetime midnight
                CustomerName = InputParser.ParseRequiredString(row.CustomerName, "CustomerName"),
                Amount = InputParser.ParseRequiredDecimal(row.Amount, "Amount"),
                Notes = InputParser.ParseOptionalString(row.Notes)
            };
        }

        private sealed class ParsedDebt
        {
            public DateTime Date { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string? Notes { get; set; }

        }
    }
}
