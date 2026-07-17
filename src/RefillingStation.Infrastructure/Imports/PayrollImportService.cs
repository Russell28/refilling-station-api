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
    public class PayrollImportService : IPayrollImportService
    {
        private readonly AppDbContext _context;

        public PayrollImportService(AppDbContext context)
        {
            _context = context;
        }

        // STEP 1: Parsed row (validated + typed, but still contains EmployeeName)
        private sealed class ParsedPayrollRow
        {
            public DateOnly EarnedDate { get; set; }
            public DateOnly? PaidDate { get; set; }
            public string EmployeeName { get; set; } = string.Empty;
            public decimal SalaryAmount { get; set; }
            public decimal CashPaid { get; set; }
            public string? Notes { get; set; }
        }

        // STEP 2: Normalized row (FK resolved, ready for domain)
        private sealed class NormalizedPayrollRow
        {
            public DateOnly EarnedDate { get; set; }
            public DateOnly? PaidDate { get; set; }
            public int EmployeeId { get; set; }
            public decimal SalaryAmount { get; set; }
            public decimal CashPaid { get; set; }
            public string? Notes { get; set; }
        }

        public async Task<ImportResult> ImportAsync(Stream stream)
        {
            var result = new ImportResult();

            // Raw CSV → PayrollImportRowRequest
            var csvRows = CsvParser.Parse(stream, new PayrollImportRowRequestMap());
            result.TotalRows = csvRows.Count;

            var normalizedRows = new List<(int RowNumber, NormalizedPayrollRow Row)>();

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
                    // STEP 1: Parse CSV → ParsedPayrollRow
                    var parsed = ParseCsvRow(csvRow);

                    // STEP 2: Normalize → NormalizedPayrollRow
                    var normalized = NormalizeParsedRow(parsed, employeeLookup);

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
            var payrolls = normalizedRows.Select(x => new PayrollEntry
            {
                EarnedDate = x.Row.EarnedDate,
                EmployeeId = x.Row.EmployeeId,
                SalaryAmount = x.Row.SalaryAmount,
                Notes = x.Row.Notes
            });

            _context.PayrollEntries.AddRange(payrolls);
            await _context.SaveChangesAsync();

            result.InsertedRows = normalizedRows.Count;
            return result;
        }

        // STEP 1: Parse CSV row → ParsedPayrollRow
        private ParsedPayrollRow ParseCsvRow(PayrollImportRowRequest row)
        {
            var earnedDate = ImportParsingHelpers.ParseRequiredDate(row.EarnedDate, "Earned Date");
            var paidDate = ImportParsingHelpers.ParseOptionalDate(row.PaidDate, "Paid Date");

            return new ParsedPayrollRow
            {
                EarnedDate = earnedDate,
                PaidDate = paidDate,
                EmployeeName = ImportParsingHelpers.ParseRequiredString(row.EmployeeName, "Employee Name"),
                SalaryAmount = ImportParsingHelpers.ParseNonNegativeDecimal(row.SalaryAmount, "Salary Amount"),
                CashPaid = ImportParsingHelpers.ParseNonNegativeDecimal(row.CashPaid, "Cash Paid"),
                Notes = ImportParsingHelpers.ParseOptionalString(row.Notes)
            };
        }

        // STEP 2: Normalize parsed row → NormalizedPayrollRow
        private NormalizedPayrollRow NormalizeParsedRow(
            ParsedPayrollRow parsed,
            Dictionary<string, int> employeeLookup)
        {
            var key = parsed.EmployeeName.Trim().ToLower();

            if (!employeeLookup.TryGetValue(key, out var employeeId))
                throw new Exception($"Category '{parsed.EmployeeName}' does not exist.");

            return new NormalizedPayrollRow
            {
                EarnedDate = parsed.EarnedDate,
                PaidDate = parsed.PaidDate,
                EmployeeId = employeeId,
                SalaryAmount = parsed.SalaryAmount,
                CashPaid = parsed.CashPaid,
                Notes = parsed.Notes,
            };
        }
    }
}
