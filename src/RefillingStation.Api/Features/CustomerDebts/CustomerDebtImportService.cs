using CsvHelper;
using CsvHelper.Configuration;
using RefillingStation.Api.Common.Import;
using RefillingStation.Api.Common.Utilities;
using RefillingStation.Api.Data;
using RefillingStation.Api.Entities;
using RefillingStation.Api.Features.CustomerDebts.dtos;
using System.Globalization;

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

            var rows = csv.GetRecords<CustomerDebtImportRowDto>().ToList();
            result.TotalRows = rows.Count;

            for (int i = 0; i < rows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var row = rows[i];

                try
                {
                    var Debt = MapRowToDebt(row);
                    parsedRows.Add(new ParsedDebtRow
                    {
                        RowNumber = rowNumber,
                        Debt = Debt,
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

        private CustomerDebtEntry MapRowToDebt(CustomerDebtImportRowDto row)
        {
            var date = InputParser.ParseRequiredDate(row.Date, "Date");

            return new CustomerDebtEntry
            {
                Date = date.ToDateTime(TimeOnly.MinValue), // Convert dateonly to datetime midnight
                CustomerName = InputParser.ParseOptionalString(row.CustomerName),
                Amount = InputParser.ParseRequiredDecimal(row.Amount, "Amount"),
                Notes = InputParser.ParseOptionalString(row.Notes)
            };
        }
    }
}
