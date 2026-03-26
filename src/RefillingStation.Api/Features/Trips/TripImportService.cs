using CsvHelper;
using CsvHelper.Configuration;
using RefillingStation.Api.Data;
using RefillingStation.Api.Entities;
using RefillingStation.Api.Features.Trips.dtos;
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

            var tripsToInsert = new List<Trip>();

            for (int i =0; i < rows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var row = rows[i];

                try
                {
                    var trip = MapRowToTrip(row);
                    tripsToInsert.Add(trip);
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

            if (tripsToInsert.Count > 0)
            {
                _db.Trips.AddRange(tripsToInsert);
                await _db.SaveChangesAsync();
            }

            result.InsertedRows = tripsToInsert.Count;
            result.FailedRows = result.Errors.Count;

            return result;
        }

        private Trip MapRowToTrip(TripImportRowDto row)
        {
            var date = ParseRequiredDate(row.Date, "Date");
            var tripNumber = ParseRequiredInt(row.TripNo, "Trip No");

            return new Trip
            {
                Date = date,
                TripNumber = tripNumber,

                TimeStarted = ParseNullableDateTime(date, row.TimeStarted),
                TimeEnded = ParseNullableDateTime(date, row.TimeEnded),

                Source = row.Source?.Trim() ?? string.Empty,
                TripType = row.TripType?.Trim() ?? string.Empty,
                EmployeeName = row.Employee?.Trim() ?? string.Empty,
                CustomerCategory = row.CustomerCategory?.Trim() ?? string.Empty,

                CollectedQty = ParseDecimalOrZero(row.CollectedQty),
                LoadedQty = ParseDecimalOrZero(row.LoadedQty),
                DeliveredQty = ParseDecimalOrZero(row.DeliveredQty),
                ActualPaidQty = ParseDecimalOrZero(row.ActualPaidQty),
                ReturnedQty = ParseDecimalOrZero(row.ReturnedQty),
                ReplacementQty = ParseDecimalOrZero(row.ReplacementQty),
                FreeQty = ParseDecimalOrZero(row.FreeQty),

                ActualCashCollected = ParseDecimalOrZero(row.ActualCashCollected),
                IsRemitted = ParseBoolOrFalse(row.IsRemitted),
                RelatedTripId = ParseNullableInt(row.RelatedTripId),
                Notes = row.Notes?.Trim() ?? string.Empty
            };
        }

        private static DateTime ParseRequiredDate(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception($"{fieldName} is required.");

            if (DateTime.TryParse(value, out var parsed))
                return parsed.Date;

            throw new Exception($"{fieldName} is invalid.");
        }

        private static int ParseRequiredInt(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception($"{fieldName} is required.");

            if (int.TryParse(value, out var parsed))
                return parsed;

            throw new Exception($"{fieldName} is invalid.");
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

        private static bool ParseBoolOrFalse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (bool.TryParse(value, out var parsed))
                return parsed;

            throw new Exception($"Invalid boolean value: '{value}'.");
        }

        private static int? ParseNullableInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (int.TryParse(value, out var parsed))
                return parsed;

            throw new Exception($"Invalid integer value: '{value}'.");
        }

        private static DateTime? ParseNullableDateTime(DateTime baseDate, string? timeValue)
        {
            if (string.IsNullOrWhiteSpace(timeValue))
                return null;

            if (TimeSpan.TryParse(timeValue, out var time))
                return baseDate.Date.Add(time);

            throw new Exception($"Invalid time value: '{timeValue}'.");
        }
    }
}
