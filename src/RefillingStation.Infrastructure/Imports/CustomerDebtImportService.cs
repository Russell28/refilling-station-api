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
    public class CustomerDebtImportService : ICustomerDebtImportService
    {
        private readonly AppDbContext _context;

        public CustomerDebtImportService(AppDbContext context)
        {
            _context = context;
        }

        // STEP 1: Parsed row (validated + typed, but still contains CustomerName)
        private sealed class ParsedCustomerDebtRow
        {
            public DateOnly Date { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string? Notes { get; set; }
        }

        // STEP 2: Normalized row (FK resolved, ready for domain)
        private sealed class NormalizedCustomerDebtRow
        {
            public DateOnly Date { get; set; }
            public int CustomerId { get; set; }
            public decimal Amount { get; set; }
            public string? Notes { get; set; }
        }

        public async Task<ImportResult> ImportAsync(Stream stream)
        {
            var result = new ImportResult();

            // Raw CSV → CustomerDebtImportRowRequest
            var csvRows = CsvParser.Parse(stream, new CustomerDebtImportRowRequestMap());
            result.TotalRows = csvRows.Count;

            var normalizedRows = new List<(int RowNumber, NormalizedCustomerDebtRow Row)>();

            // Load FK lookup once
            var customerLookup = await _context.Customers
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
                    // STEP 1: Parse CSV → ParsedCustomerDebtRow
                    var parsed = ParseCsvRow(csvRow);

                    // STEP 2: Normalize → NormalizedCustomerDebtRow
                    var normalized = NormalizeParsedRow(parsed, customerLookup);

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
            var customerDebts = normalizedRows.Select(x => new CustomerDebtEntry
            {
                Date = x.Row.Date,
                Amount = x.Row.Amount,
                Notes = x.Row.Notes,
                CustomerId = x.Row.CustomerId
            });

            _context.CustomerDebtEntries.AddRange(customerDebts);
            await _context.SaveChangesAsync();

            result.InsertedRows = normalizedRows.Count;
            return result;
        }

        // STEP 1: Parse CSV row → ParsedCustomerDebtRow
        private ParsedCustomerDebtRow ParseCsvRow(CustomerDebtImportRowRequest row)
        {
            var date = ImportParsingHelpers.ParseRequiredDate(row.Date, "Date");

            return new ParsedCustomerDebtRow
            {
                Date = date,
                CustomerName = ImportParsingHelpers.ParseRequiredString(row.CustomerName, "Customer"),
                Amount = ImportParsingHelpers.ParseRequiredDecimal(row.Amount, "Amount"),
                Notes = ImportParsingHelpers.ParseOptionalString(row.Notes)
            };
        }

        // STEP 2: Normalize parsed row → NormalizedCustomerDebtRow
        private NormalizedCustomerDebtRow NormalizeParsedRow(
            ParsedCustomerDebtRow parsed,
            Dictionary<string, int> customerLookup)
        {
            var key = parsed.CustomerName.Trim().ToLower();

            if (!customerLookup.TryGetValue(key, out var customerId))
                throw new Exception($"Customer '{parsed.CustomerName}' does not exist.");

            return new NormalizedCustomerDebtRow
            {
                Date = parsed.Date,
                Amount = parsed.Amount,
                Notes = parsed.Notes,
                CustomerId = customerId
            };
        }
    }
}
