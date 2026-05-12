using FluentValidation;
using RefillingStation.Application.DTOs.Payrolls;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _repository;
        private readonly IValidator<PayrollCreateRequest> _validator;

        public PayrollService(
            IPayrollRepository repository,
            IValidator<PayrollCreateRequest> validator)
        {
            _repository = repository;
            _validator = validator;
        }
        public async Task<List<PayrollDetailResponse>> GetAllAsync()
        {
            var payrolls = await _repository.GetAllAsync();

            return payrolls
                .Select(x => new PayrollDetailResponse
                (
                    x.Id,
                    x.EarnedDate,
                    x.PaidDate,
                    x.EmployeeId,
                    x.Employee.FullName,
                    x.SalaryAmount,
                    x.CashPaid,
                    x.Notes
                ))
                .ToList();
        }

        public async Task<PayrollDetailResponse> GetByIdAsync(int id)
        {
            var payroll = await _repository.GetByIdAsync(id);

            if (payroll is null)
                throw new Exception("Payroll not found.");

            return new PayrollDetailResponse(
                payroll.Id,
                payroll.EarnedDate,
                payroll.PaidDate,
                payroll.EmployeeId,
                payroll.Employee.FullName,
                payroll.SalaryAmount,
                payroll.CashPaid,
                payroll.Notes
            );
        }

        public async Task<int> CreateAsync(PayrollCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var payroll = new PayrollEntry
            {
                EarnedDate = request.EarnedDate,
                PaidDate = request.PaidDate,
                EmployeeId = request.EmployeeId,
                SalaryAmount = request.SalaryAmount,
                CashPaid = request.CashPaid,
                Notes = request.Notes
            };

            await _repository.AddAsync(payroll);
            await _repository.SaveChangesAsync();

            return payroll.Id;
        }

        public async Task UpdateAsync(int id, PayrollCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var payroll = await _repository.GetByIdAsync(id);

            if (payroll is null)
                throw new Exception("Payroll not found.");

            payroll.EarnedDate = request.EarnedDate;
            payroll.PaidDate = request.PaidDate;
            payroll.EmployeeId = request.EmployeeId;
            payroll.SalaryAmount = request.SalaryAmount;
            payroll.CashPaid = request.CashPaid;
            payroll.Notes = request.Notes;

            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payroll = await _repository.GetByIdAsync(id);

            if (payroll is null)
                throw new Exception("Payroll not found.");

            _repository.Remove(payroll);

            await _repository.SaveChangesAsync();
        }
    }
}
