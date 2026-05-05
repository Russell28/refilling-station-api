using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using RefillingStation.Api.Data;
using RefillingStation.Domain.Entities;
using RefillingStation.Application.DTOs.Trips;
using System.Globalization;

namespace RefillingStation.Api.Features.Trips
{
    public class TripImportService
    {
        private readonly AppDbContext _db;

        public TripImportService(AppDbContext db)
        {
            _db = db;
        }

        private sealed class ParsedTripRow
        {
            public int RowNumber { get; set; }
            public Trip Trip { get; set; } = null!;
        }

        private sealed class ParsedTrip : Trip 
        {
            public string EmployeeName { get; set; } = string.Empty;
        }

        public async Task<TripImportResult> ImportAsync(IFormFile file)
        {
            var result = new TripImportResult();

            if (file == null || file.Length == 0)
            {
                result.Errors.Add(new TripImportError
                {
                    RowNumber = 0,
                    Message = "No file uploaded."
                });
                result.FailedRows = 1;
                return result;
            }

            var parsedRows = new List<ParsedTripRow>();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            });

            var rows = csv.GetRecords<TripImportRowDto>().ToList();
            result.TotalRows = rows.Count;

            var employees = await _db.Employees.ToListAsync();

            for (int i =0; i < rows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var row = rows[i];

                try
                {
                    var parsedTrip = MapRowToTrip(row);
                    var trip = new Trip
                    {
                        Date = parsedTrip.Date,
                        TripNumber = parsedTrip.TripNumber,
                        EmployeeId = employees.FirstOrDefault(x => x.FullName.Trim() == parsedTrip.EmployeeName)?.Id
                            ?? throw new Exception($"Employee '{parsedTrip.EmployeeName}' not found"),
                        CustomerCategory = parsedTrip.CustomerCategory,
                        CollectedQty = parsedTrip.CollectedQty,
                        LoadedQty = parsedTrip.LoadedQty,
                        DeliveredQty = parsedTrip.DeliveredQty,
                        ReturnedQty = parsedTrip.ReturnedQty,
                        ReplacementQty = parsedTrip.ReplacementQty,
                        FreeQty = parsedTrip.FreeQty,
                        ActualCashCollected = parsedTrip.ActualCashCollected,
                        IsRemitted = parsedTrip.IsRemitted,
                        Notes = parsedTrip.Notes
                    };
                    // Save parsed rows to a list
                    parsedRows.Add(new ParsedTripRow
                    {
                        RowNumber = rowNumber,
                        Trip = trip,
                    });
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new TripImportError()
                    {
                        RowNumber = rowNumber,
                        Message = ex.Message,
                    });
                }
            }

            AddDuplicateFileErrors(parsedRows, result); // Check duplicates in csv file
            await AddDuplicateDatabaseErrorsAsync(parsedRows, result); // Check duplicate records in DB

            if (result.Errors.Any())
            {
                result.InsertedRows = 0;
                result.FailedRows = result.Errors
                    .Select(x => x.RowNumber)
                    .Distinct()
                    .Count();

                return result;
            }

            _db.Trips.AddRange(parsedRows.Select(x => x.Trip));
            await _db.SaveChangesAsync();

            result.InsertedRows = parsedRows.Count;
            result.FailedRows = result.Errors.Count;

            return result;
        }

        private static string BuildTripImportKey(DateOnly date, int tripNumber)
        {
            return $"{date:yyy-MM-dd}|{tripNumber}";
        }

        private static void AddDuplicateFileErrors(
            List<ParsedTripRow> parsedRows,
            TripImportResult result)
        {
            var seen = new Dictionary<string, int>();

            foreach (var row in parsedRows)
            {
                var key = BuildTripImportKey(row.Trip.Date, row.Trip.TripNumber);

                if (seen.TryGetValue(key, out var firstRowNumber))
                {
                    result.Errors.Add(new TripImportError
                    {
                        RowNumber = row.RowNumber,
                        Message = $"Duplicate Date + Trip Number found in file. First occurence is on row {firstRowNumber}."
                    });
                } 
                else
                {
                    seen[key] = row.RowNumber;
                }
            }

        }

        private async Task AddDuplicateDatabaseErrorsAsync(
            List<ParsedTripRow> parsedTripRows,
            TripImportResult result)
        {
            foreach (var row in parsedTripRows)
            {
                var exists = await _db.Trips.AnyAsync(x =>
                    x.Date == row.Trip.Date
                    && x.TripNumber == row.Trip.TripNumber
                );

                if (exists)
                {
                    result.Errors.Add(new TripImportError
                    {
                        RowNumber= row.RowNumber,
                        Message = $"Trip with the same Date ({row.Trip.Date}) and Trip Number ({row.Trip.TripNumber}) already exists in the database."
                    });
                }
            }
        }

        private ParsedTrip MapRowToTrip(TripImportRowDto row)
        {
            var date = ParseRequiredDate(row.Date, "Date");
            var tripNumber = ParseRequiredInt(row.TripNo, "Trip No");
            var timeStarted = ParseOptionalTime(row.TimeStarted, "Time Started", date);
            var timeEnded = ParseOptionalTime(row.TimeEnded, "Time Ended", date);

            if (timeStarted.HasValue && timeEnded.HasValue && timeEnded.Value < timeStarted.Value)
            {
                throw new Exception("Time Ended must be greater than or equal to Time Started.");
            }

            return new ParsedTrip
            {
                Date = date,
                TripNumber = tripNumber,

                TimeStarted = timeStarted,
                TimeEnded = timeEnded,

                EmployeeName = ParseRequiredString(row.EmployeeName, "Employee Name"),
                Source = ParseOptionalString(row.Source),
                TripType = ParseOptionalString(row.TripType),
                CustomerCategory = ParseOptionalString(row.CustomerCategory),

                CollectedQty = ParseNonNegativeDecimal(row.CollectedQty, "Collected Qty"),
                LoadedQty = ParseNonNegativeDecimal(row.LoadedQty, "Loaded Qty"),
                DeliveredQty = ParseNonNegativeDecimal(row.DeliveredQty, "Delivered Qty"),
                ReturnedQty = ParseNonNegativeDecimal(row.ReturnedQty, "Returned Qty"),
                ReplacementQty = ParseNonNegativeDecimal(row.ReplacementQty, "Replacement Qty"),
                FreeQty = ParseNonNegativeDecimal(row.FreeQty, "Free Qty"),

                ActualCashCollected = ParseNonNegativeDecimal(row.ActualCashCollected, "Actual Cash Collected"),
                IsRemitted = ParseRequiredBool(row.IsRemitted, "Is Remitted"),
                Notes = ParseOptionalString(row.Notes)
            };
        }

        private static string ParseRequiredString(string? value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception($"{fieldName} is required.");

            return value.Trim();
        } 

        private static string? ParseOptionalString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static DateOnly ParseRequiredDate(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception($"{fieldName} is required.");

            if (!DateOnly.TryParse(value, out var parsed))
                throw new Exception($"{fieldName} must be a valid date.");

            return parsed;
        }

        private static int ParseRequiredInt(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"{fieldName} is required.");
            }

            if (!int.TryParse(value, out var parsedValue))
            {
                throw new Exception($"{fieldName} must be a valid whole number.");
            }

            if (parsedValue <= 0)
            {
                throw new Exception($"{fieldName} must be greater than 0.");
            }

            return parsedValue;
        }

        private static decimal ParseNonNegativeDecimal(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            if (!decimal.TryParse(value, out var parsedValue))
            {
                throw new Exception($"{fieldName} must be a valid number.");
            }

            if (parsedValue < 0)
            {
                throw new Exception($"{fieldName} cannot be negative.");
            }

            return parsedValue;
        }

        private static decimal ParseDecimalOrZero(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            var cleaned = value.Replace("₱", "").Replace(",", "").Trim();

            if (decimal.TryParse(cleaned, out var parsed))
                return parsed;

            throw new Exception($"Invalid decimal value: '{value}'.");
        }

        private static bool ParseRequiredBool(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"{fieldName} is required.");
            }

            if (!bool.TryParse(value, out var parsedValue))
            {
                throw new Exception($"{fieldName} must be TRUE or FALSE.");
            }

            return parsedValue;
        }

        private static int? ParseNullableInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (int.TryParse(value, out var parsed))
                return parsed;

            throw new Exception($"Invalid integer value: '{value}'.");
        }

        private static DateTime? ParseOptionalTime(string? value, string fieldName, DateOnly date)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!TimeOnly.TryParse(value, out var parsedTime))
            {
                throw new Exception($"{fieldName} must be a valid time.");
            }

            return date.ToDateTime(parsedTime);
        }
    }

    
}
