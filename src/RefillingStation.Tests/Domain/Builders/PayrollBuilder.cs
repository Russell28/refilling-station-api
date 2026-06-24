using RefillingStation.Domain.Entities;

namespace RefillingStation.Tests.Domain.Builders
{
    public class PayrollBuilder
    {
        private int _id = 1;
        private DateOnly _earnedDate = DateOnly.FromDateTime(DateTime.Now);
        private DateOnly? _paidDate = DateOnly.FromDateTime(DateTime.Now);
        private int _employeeId = 1;
        private decimal _salaryAmount = 5000m;
        private decimal _cashPaid = 5000m;
        private string? _notes = null;
        private Employee _employee = new EmployeeBuilder().Build();

        public PayrollBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public PayrollBuilder WithEarnedDate(DateOnly earnedDate)
        {
            _earnedDate = earnedDate;
            return this;
        }

        public PayrollBuilder WithPaidDate(DateOnly? paidDate)
        {
            _paidDate = paidDate;
            return this;
        }

        public PayrollBuilder WithEmployeeId(int employeeId)
        {
            _employeeId = employeeId;
            return this;
        }

        public PayrollBuilder WithSalaryAmount(decimal salaryAmount)
        {
            _salaryAmount = salaryAmount;
            return this;
        }

        public PayrollBuilder WithCashPaid(decimal cashPaid)
        {
            _cashPaid = cashPaid;
            return this;
        }

        public PayrollBuilder WithNotes(string? notes)
        {
            _notes = notes;
            return this;
        }

        public PayrollBuilder WithEmployee(Employee employee)
        {
            _employee = employee;
            _employeeId = employee.Id;
            return this;
        }

        public PayrollEntry Build()
        {
            return new PayrollEntry
            {
                Id = _id,
                EarnedDate = _earnedDate,
                PaidDate = _paidDate,
                EmployeeId = _employeeId,
                SalaryAmount = _salaryAmount,
                CashPaid = _cashPaid,
                Notes = _notes,
                Employee = _employee
            };
        }
    }
}
