using RefillingStation.Application.DTOs.PayrollEntries;

namespace RefillingStation.Tests.Services.Builders
{
    public class PayrollCreateRequestBuilder
    {
        private DateOnly _earnedDate = DateOnly.FromDateTime(DateTime.Now);
        private DateOnly? _paidDate = DateOnly.FromDateTime(DateTime.Now);
        private int _employeeId = 1;
        private decimal _salaryAmount = 5000m;
        private decimal _cashPaid = 5000m;
        private string? _notes = null;

        public PayrollCreateRequestBuilder WithEarnedDate(DateOnly earnedDate)
        {
            _earnedDate = earnedDate;
            return this;
        }

        public PayrollCreateRequestBuilder WithPaidDate(DateOnly? paidDate)
        {
            _paidDate = paidDate;
            return this;
        }

        public PayrollCreateRequestBuilder WithEmployeeId(int employeeId)
        {
            _employeeId = employeeId;
            return this;
        }

        public PayrollCreateRequestBuilder WithSalaryAmount(decimal salaryAmount)
        {
            _salaryAmount = salaryAmount;
            return this;
        }

        public PayrollCreateRequestBuilder WithCashPaid(decimal cashPaid)
        {
            _cashPaid = cashPaid;
            return this;
        }

        public PayrollCreateRequestBuilder WithNotes(string? notes)
        {
            _notes = notes;
            return this;
        }

        public PayrollEntryCreateRequest Build()
        {
            return new PayrollEntryCreateRequest
            {
                EarnedDate = _earnedDate,
                PaidDate = _paidDate,
                EmployeeId = _employeeId,
                SalaryAmount = _salaryAmount,
                CashPaid = _cashPaid,
                Notes = _notes
            };
        }
    }
}
