using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using RefillingStation.Api.Common.Import;
using RefillingStation.Api.Common.Utilities;
using RefillingStation.Api.Data;
using RefillingStation.Api.Entities;
using RefillingStation.Api.Features.Employees.dtos;
using RefillingStation.Api.Features.Payrolls.dtos;
using System.Globalization;

namespace RefillingStation.Api.Features.Payrolls
{
    public class PayrollEntryImportService
    {
        private readonly AppDbContext _db;

        public PayrollEntryImportService(AppDbContext db)
        {
            _db = db;
        }

        private sealed class ParsedPayrollRow
        {
            public int RowNumber { get; set; }
            public PayrollEntry Payroll { get; set; } = null!;
        }

        private sealed class ParsedPayroll : CreatePayrollRequest
        {
            public string EmployeeName { get; set; } = string.Empty;
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

            var parsedRows = new List<ParsedPayrollRow>();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            });

            var rows = csv.GetRecords<PayrollEntryImportRowDto>().ToList();
            result.TotalRows = rows.Count;

            var employees = await _db.Employees
                .Select(e => new EmployeeListItem(e.Id, e.FullName))
                .ToListAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var rowNumber = i + 2; // header is row 1
                var row = rows[i];

                try
                {
                    var parsedPayroll = MapRowToPayroll(row);
                    var payroll = new PayrollEntry
                    {
                        EarnedDate = parsedPayroll.EarnedDate,
                        PaidDate = parsedPayroll.PaidDate,
                        EmployeeId = employees.FirstOrDefault(e => e.Name.Trim() == parsedPayroll.EmployeeName)?.Id
                            ?? throw new Exception($"Employee '{parsedPayroll.EmployeeName}' not found"),
                        SalaryAmount = parsedPayroll.SalaryAmount,
                        CashPaid = parsedPayroll.CashPaid,
                        Notes = parsedPayroll.Notes
                    };
                    parsedRows.Add(new ParsedPayrollRow
                    {
                        RowNumber = rowNumber,
                        Payroll = payroll,
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

            _db.PayrollEntries.AddRange(parsedRows.Select(x => x.Payroll));
            await _db.SaveChangesAsync();

            result.InsertedRows = parsedRows.Count;
            result.FailedRows = result.Errors.Count;

            return result;

        }

        private ParsedPayroll MapRowToPayroll(PayrollEntryImportRowDto row)
        {
            var earnedDate = InputParser.ParseRequiredDate(row.EarnedDate, "Earned Date");
            var paidDate = InputParser.ParseOptionalDate(row.PaidDate, "Paid Date");

            return new ParsedPayroll
            {
                EarnedDate = earnedDate.ToDateTime(TimeOnly.MinValue), // Convert dateonly to datetime midnight
                PaidDate = paidDate?.ToDateTime(TimeOnly.MinValue), 
                EmployeeName = InputParser.ParseRequiredString(row.EmployeeName, "Employee Name"),
                SalaryAmount = InputParser.ParseNonNegativeDecimal(row.SalaryAmount, "Salary Amount"),
                CashPaid = InputParser.ParseNonNegativeDecimal(row.CashPaid, "Cash Paid"),
                Notes = InputParser.ParseOptionalString(row.Notes)
            };
        }
    }
}
