using FluentValidation;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.PayrollPayments;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class PayrollPaymentService : IPayrollPaymentService
    {
        private readonly IPayrollPaymentRepository _payrollPaymentRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IValidator<PayrollPaymentCreateRequest> _validator;

        public PayrollPaymentService(
            IPayrollPaymentRepository payrollPaymentRepository,
            IEmployeeRepository employeeRepository,
            IValidator<PayrollPaymentCreateRequest> validator)
        {
            _payrollPaymentRepository = payrollPaymentRepository;
            _employeeRepository = employeeRepository;
            _validator = validator;
        }
        public async Task<List<PayrollPaymentDetailResponse>> GetAllAsync()
        {
            var payments = await _payrollPaymentRepository.GetAllAsync();

            return payments
                .Select(x => new PayrollPaymentDetailResponse
                (
                    x.Id,
                    x.EmployeeId,
                    x.Employee.FullName,
                    x.PaidDate,
                    x.AmountPaid,
                    x.Notes
                ))
                .ToList();
        }

        public async Task<PayrollPaymentDetailResponse> GetByIdAsync(int id)
        {
            var payment = await _payrollPaymentRepository.GetByIdAsync(id);

            if (payment is null)
                throw new NotFoundException("Payroll Payment", id);

            return new PayrollPaymentDetailResponse(
                payment.Id,
                payment.EmployeeId,
                payment.Employee.FullName,
                payment.PaidDate,
                payment.AmountPaid,
                payment.Notes
            );
        }

        public async Task<int> CreateAsync(PayrollPaymentCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var employeeExists = await _employeeRepository.ExistsAsync(e => e.Id == request.EmployeeId);

            if (!employeeExists)
                throw new NotFoundException("Employee", request.EmployeeId);

            var payment = new PayrollPayment
            {
                EmployeeId = request.EmployeeId,
                PaidDate = request.PaidDate,
                AmountPaid = request.AmountPaid,
                Notes = request.Notes
            };

            await _payrollPaymentRepository.AddAsync(payment);
            await _payrollPaymentRepository.SaveChangesAsync();

            return payment.Id;
        }

        public async Task UpdateAsync(int id, PayrollPaymentCreateRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var employeeExists = await _employeeRepository.ExistsAsync(e => e.Id == request.EmployeeId);

            if (!employeeExists)
                throw new NotFoundException("Employee", request.EmployeeId);

            var payment = await _payrollPaymentRepository.GetByIdAsync(id);

            if (payment is null)
                throw new NotFoundException("Payroll", id);

            payment.EmployeeId = request.EmployeeId;
            payment.PaidDate = request.PaidDate;
            payment.AmountPaid = request.AmountPaid;
            payment.Notes = request.Notes;

            await _payrollPaymentRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await _payrollPaymentRepository.GetByIdAsync(id);

            if (payment is null)
                throw new NotFoundException("Payroll Payment", id);

            _payrollPaymentRepository.Remove(payment);

            await _payrollPaymentRepository.SaveChangesAsync();
        }

        public async Task<List<PayrollPaymentDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request)
        {
            var options = new DateRangeOptions
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Page = request.Page
            };

            var payrolls = await _payrollPaymentRepository.SearchByDateRangeAsync(options);

            return payrolls
                .Select(x => new PayrollPaymentDetailResponse
                (
                    x.Id,
                    x.EmployeeId,
                    x.Employee.FullName,
                    x.PaidDate,
                    x.AmountPaid,
                    x.Notes
                ))
                .ToList();
        }
    }
}
