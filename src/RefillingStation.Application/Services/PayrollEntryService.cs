using FluentValidation;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.PayrollEntries;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class PayrollEntryService : IPayrollEntryService
    {
        private readonly IPayrollEntryRepository _payrollRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IValidator<PayrollEntryCreateRequest> _validator;

        public PayrollEntryService(
            IPayrollEntryRepository payrollRepository,
            IEmployeeRepository employeeRepository,
            IValidator<PayrollEntryCreateRequest> validator)
        {
            _payrollRepository = payrollRepository;
            _employeeRepository = employeeRepository;
            _validator = validator;
        }
        public async Task<List<PayrollEntryDetailResponse>> GetAllAsync()
        {
            var payrolls = await _payrollRepository.GetAllAsync();

            return payrolls
                .Select(x => new PayrollEntryDetailResponse
                (
                    x.Id,
                    x.EarnedDate,
                    x.EmployeeId,
                    x.Employee.FullName,
                    x.SalaryAmount,
                    x.Notes
                ))
                .ToList();
        }

        public async Task<PayrollEntryDetailResponse> GetByIdAsync(int id)
        {
            var payroll = await _payrollRepository.GetByIdAsync(id);

            if (payroll is null)
                throw new NotFoundException("Payroll", id);

            return new PayrollEntryDetailResponse(
                payroll.Id,
                payroll.EarnedDate,
                payroll.EmployeeId,
                payroll.Employee.FullName,
                payroll.SalaryAmount,
                payroll.Notes
            );
        }

        public async Task<int> CreateAsync(PayrollEntryCreateRequest request)
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
                EmployeeId = request.EmployeeId,
                SalaryAmount = request.SalaryAmount,
                Notes = request.Notes
            };

            await _payrollRepository.AddAsync(payroll);
            await _payrollRepository.SaveChangesAsync();

            return payroll.Id;
        }

        public async Task UpdateAsync(int id, PayrollEntryCreateRequest request)
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
            payroll.EmployeeId = request.EmployeeId;
            payroll.SalaryAmount = request.SalaryAmount;
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

        public async Task<List<PayrollEntryDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request)
        {
            var options = new DateRangeOptions
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Page = request.Page
            };

            var payrolls = await _payrollRepository.SearchByDateRangeAsync(options);

            return payrolls
                .Select(x => new PayrollEntryDetailResponse
                (
                    x.Id,
                    x.EarnedDate,
                    x.EmployeeId,
                    x.Employee.FullName,
                    x.SalaryAmount,
                    x.Notes
                ))
                .ToList();
        }
    }
}
