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
    public class TripImportService : ITripImportService
    {
        private readonly AppDbContext _context;

        public TripImportService(AppDbContext context)
        {
            _context = context;
        }

        // STEP 1: Parsed row (validated + typed, but still contains EmployeeName)
        private sealed class ParsedTripRow
        {
            public DateOnly Date { get; set; }
            public int TripNumber { get; set; }
            public DateTime? TimeStarted { get; set; }
            public DateTime? TimeEnded { get; set; }

            // ===== Source info =====
            public string EmployeeName { get; set; } = string.Empty;
            public string? Source { get; set; }
            public string? TripType { get; set; }
            public string? CustomerCategory { get; set; }

            // ===== Quantities =====
            public decimal CollectedQty { get; set; }
            public decimal LoadedQty { get; set; }
            public decimal DeliveredQty { get; set; }
            public decimal FreeQty { get; set; }
            public decimal ReturnedQty { get; set; }
            public decimal ReplacementQty { get; set; }

            // ===== Cash =====
            public decimal ActualCashCollected { get; set; }
            public bool IsRemitted { get; set; } = false;

            // ===== Notes =====
            public string? Notes { get; set; }
        }

        // STEP 2: Normalized row (FK resolved, ready for domain)
        private sealed class NormalizedTripRow
        {
            public DateOnly Date { get; set; }
            public int TripNumber { get; set; }
            public DateTime? TimeStarted { get; set; }
            public DateTime? TimeEnded { get; set; }

            // ===== Source info =====
            public int EmployeeId { get; set; }
            public string? Source { get; set; }
            public string? TripType { get; set; }
            public string? CustomerCategory { get; set; }

            // ===== Quantities =====
            public decimal CollectedQty { get; set; }
            public decimal LoadedQty { get; set; }
            public decimal DeliveredQty { get; set; }
            public decimal FreeQty { get; set; }
            public decimal ReturnedQty { get; set; }
            public decimal ReplacementQty { get; set; }

            // ===== Cash =====
            public decimal ActualCashCollected { get; set; }
            public bool IsRemitted { get; set; } = false;

            // ===== Notes =====
            public string? Notes { get; set; }
        }

        private sealed class NormalizedTripRowWithNumber
        {
            public int RowNumber { get; set; }
            public NormalizedTripRow Row { get; set; } = default!;
        }

        public async Task<ImportResult> ImportAsync(Stream stream)
        {
            var result = new ImportResult();

            // Raw CSV → TripImportRowRequest
            var csvRows = CsvParser.Parse(stream, new TripImportRowRequestMap());
            result.TotalRows = csvRows.Count;

            var normalizedRows = new List<NormalizedTripRowWithNumber>();

            // Load FK lookup once
            var employeeLookup = await _context.Employees
                .ToDictionaryAsync(
                    x => x.FullName.Trim().ToLower(),
                    x => x.Id
                );

            for (int i = 0; i < csvRows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var csvRow = csvRows[i];

                try
                {
                    // STEP 1: Parse CSV → ParsedTripRow
                    var parsed = ParseCsvRow(csvRow);

                    // STEP 2: Normalize → NormalizedTripRow
                    var normalized = NormalizeParsedRow(parsed, employeeLookup);

                    normalizedRows.Add(new NormalizedTripRowWithNumber
                    {
                        RowNumber = rowNumber,
                        Row = normalized
                    });
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

            // STEP 2.5: Duplicate check on CSV (Date + TripNumber)
            DuplicateRowChecker.AddDuplicateErrors(
                rows: normalizedRows,
                keySelector: x => $"{x.Row.Date:yyyy-MM-dd}|{x.Row.TripNumber}",
                rowNumberSelector: x => x.RowNumber,
                errors: result.Errors
            );

            // STEP 2.6: DB Duplicate check (Date + TripNumber)
            await AddDuplicateDatabaseErrorsAsync(normalizedRows, result);

            // If errors exist, stop before DB insert
            if (result.Errors.Any())
            {
                result.FailedRows = result.Errors.Count;
                return result;
            }

            // STEP 3: Convert normalized rows → domain entities
            var trips = normalizedRows.Select(x => new Trip
            {
                Date = x.Row.Date,
                TripNumber = x.Row.TripNumber,

                TimeStarted = x.Row.TimeStarted,
                TimeEnded = x.Row.TimeEnded,

                EmployeeId = x.Row.EmployeeId,
                Source = x.Row.Source,
                TripType = x.Row.TripType,
                CustomerCategory = x.Row.CustomerCategory,

                CollectedQty = x.Row.CollectedQty,
                LoadedQty = x.Row.LoadedQty,
                DeliveredQty = x.Row.DeliveredQty,
                ReturnedQty = x.Row.ReturnedQty,
                ReplacementQty = x.Row.ReplacementQty,
                FreeQty = x.Row.FreeQty,

                ActualCashCollected = x.Row.ActualCashCollected,
                IsRemitted = x.Row.IsRemitted,
                Notes = x.Row.Notes
            });

            _context.Trips.AddRange(trips);
            await _context.SaveChangesAsync();

            result.InsertedRows = normalizedRows.Count;
            return result;
        }

        private async Task AddDuplicateDatabaseErrorsAsync(
            IEnumerable<NormalizedTripRowWithNumber> rows,
            ImportResult result)
        {
            // 1. Distinct keys from DB
            var dbKeys = await _context.Trips
                .Select(t => new { t.Date, t.TripNumber })
                .Distinct()
                .ToListAsync();

            var dbSet = dbKeys
                .Select(k => (k.Date, k.TripNumber))
                .ToHashSet();

            // 2. Compare each CSV row to DB keys
            foreach (var row in rows)
            {
                var key = (row.Row.Date, row.Row.TripNumber);

                if (dbSet.Contains(key))
                {
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = row.RowNumber,
                        Message = $"Trip with the same Date ({row.Row.Date}) and Trip Number ({row.Row.TripNumber}) already exists in the database."
                    });
                }
            }
        }



        // STEP 1: Parse CSV row → ParsedTripRow
        private ParsedTripRow ParseCsvRow(TripImportRowRequest row)
        {
            // Parse all fields first
            var date = ImportParsingHelpers.ParseRequiredDate(row.Date, "Date");
            var tripNumber = ImportParsingHelpers.ParseRequiredInt(row.TripNo, "Trip No");

            var timeStarted = ImportParsingHelpers.ParseOptionalTime(row.TimeStarted, "Time Started", date);
            var timeEnded = ImportParsingHelpers.ParseOptionalTime(row.TimeEnded, "Time Ended", date);

            var employeeName = ImportParsingHelpers.ParseRequiredString(row.EmployeeName, "Employee Name");
            var source = ImportParsingHelpers.ParseOptionalString(row.Source);
            var tripType = ImportParsingHelpers.ParseOptionalString(row.TripType);
            var customerCategory = ImportParsingHelpers.ParseOptionalString(row.CustomerCategory);

            var collectedQty = ImportParsingHelpers.ParseNonNegativeDecimal(row.CollectedQty, "Collected Qty");
            var loadedQty = ImportParsingHelpers.ParseNonNegativeDecimal(row.LoadedQty, "Loaded Qty");
            var deliveredQty = ImportParsingHelpers.ParseNonNegativeDecimal(row.DeliveredQty, "Delivered Qty");
            var returnedQty = ImportParsingHelpers.ParseNonNegativeDecimal(row.ReturnedQty, "Returned Qty");
            var replacementQty = ImportParsingHelpers.ParseNonNegativeDecimal(row.ReplacementQty, "Replacement Qty");
            var freeQty = ImportParsingHelpers.ParseNonNegativeDecimal(row.FreeQty, "Free Qty");

            var actualCashCollected = ImportParsingHelpers.ParseNonNegativeDecimal(row.ActualCashCollected, "Actual Cash Collected");
            var isRemitted = ImportParsingHelpers.ParseRequiredBool(row.IsRemitted, "Is Remitted");
            var notes = ImportParsingHelpers.ParseOptionalString(row.Notes);

            // Return parsed row
            return new ParsedTripRow
            {
                Date = date,
                TripNumber = tripNumber,

                TimeStarted = timeStarted,
                TimeEnded = timeEnded,

                EmployeeName = employeeName,
                Source = source,
                TripType = tripType,
                CustomerCategory = customerCategory,

                CollectedQty = collectedQty,
                LoadedQty = loadedQty,
                DeliveredQty = deliveredQty,
                ReturnedQty = returnedQty,
                ReplacementQty = replacementQty,
                FreeQty = freeQty,

                ActualCashCollected = actualCashCollected,
                IsRemitted = isRemitted,
                Notes = notes
            };
        }


        // STEP 2: Normalize parsed row → NormalizedTripRow
        private NormalizedTripRow NormalizeParsedRow(
            ParsedTripRow parsed,
            Dictionary<string, int> employeeLookup)
        {
            var employeeKey = parsed.EmployeeName.Trim().ToLower();

            if (!employeeLookup.TryGetValue(employeeKey, out var employeeId))
                throw new Exception($"Employee '{parsed.EmployeeName}' does not exist.");

            return new NormalizedTripRow
            {
                Date = parsed.Date,
                TripNumber = parsed.TripNumber,

                TimeStarted = parsed.TimeStarted,
                TimeEnded = parsed.TimeEnded,

                EmployeeId = employeeId,
                Source = parsed.Source,
                TripType = parsed.TripType,
                CustomerCategory = parsed.CustomerCategory,

                CollectedQty = parsed.CollectedQty,
                LoadedQty = parsed.LoadedQty,
                DeliveredQty = parsed.DeliveredQty,
                ReturnedQty = parsed.ReturnedQty,
                ReplacementQty = parsed.ReplacementQty,
                FreeQty = parsed.FreeQty,

                ActualCashCollected = parsed.ActualCashCollected,
                IsRemitted = parsed.IsRemitted,
                Notes = parsed.Notes
            };
        }

    }
}
