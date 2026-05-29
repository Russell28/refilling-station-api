using FluentValidation;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.Expenses;
using RefillingStation.Application.DTOs.Payrolls;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IValidator<PayrollCreateRequest> _validator;

        public PayrollService(
            IPayrollRepository payrollRepository,
            IEmployeeRepository employeeRepository,
            IValidator<PayrollCreateRequest> validator)
        {
            _payrollRepository = payrollRepository;
            _employeeRepository = employeeRepository;
            _validator = validator;
        }
        public async Task<List<PayrollDetailResponse>> GetAllAsync()
        {
            var payrolls = await _payrollRepository.GetAllAsync();

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
            var payroll = await _payrollRepository.GetByIdAsync(id);

            if (payroll is null)
                throw new NotFoundException("Payroll", id);

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

            var employeeExists = await _employeeRepository.ExistsAsync(e => e.Id == request.EmployeeId);

            if (!employeeExists)
                throw new NotFoundException("Employee", request.EmployeeId);

            var payroll = new PayrollEntry
            {
                EarnedDate = request.EarnedDate,
                PaidDate = request.PaidDate,
                EmployeeId = request.EmployeeId,
                SalaryAmount = request.SalaryAmount,
                CashPaid = request.CashPaid,
                Notes = request.Notes
            };

            await _payrollRepository.AddAsync(payroll);
            await _payrollRepository.SaveChangesAsync();

            return payroll.Id;
        }

        public async Task UpdateAsync(int id, PayrollCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var employeeExists = await _employeeRepository.ExistsAsync(e => e.Id == request.EmployeeId);

            if (!employeeExists)
                throw new NotFoundException("Employee", request.EmployeeId);

            var payroll = await _payrollRepository.GetByIdAsync(id);

            if (payroll is null)
                throw new NotFoundException("Payroll", id);

            payroll.EarnedDate = request.EarnedDate;
            payroll.PaidDate = request.PaidDate;
            payroll.EmployeeId = request.EmployeeId;
            payroll.SalaryAmount = request.SalaryAmount;
            payroll.CashPaid = request.CashPaid;
            payroll.Notes = request.Notes;

            await _payrollRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payroll = await _payrollRepository.GetByIdAsync(id);

            if (payroll is null)
                throw new NotFoundException("Payroll", id);

            _payrollRepository.Remove(payroll);

            await _payrollRepository.SaveChangesAsync();
        }

        public async Task<List<PayrollDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request)
        {
            var options = new DateRangeOptions
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Page = request.Page
            };

            var payrolls = await _payrollRepository.SearchByDateRangeAsync(options);

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
    }
}
