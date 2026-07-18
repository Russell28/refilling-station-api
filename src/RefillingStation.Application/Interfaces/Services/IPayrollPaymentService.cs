using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.PayrollEntries;
using RefillingStation.Application.DTOs.PayrollPayments;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IPayrollPaymentService
    {
        Task<List<PayrollPaymentDetailResponse>> GetAllAsync();
        Task<PayrollPaymentDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(PayrollPaymentCreateRequest request);
        Task UpdateAsync(int id, PayrollPaymentCreateRequest request);
        Task DeleteAsync(int id);
        Task<List<PayrollPaymentDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request);

    }
}
