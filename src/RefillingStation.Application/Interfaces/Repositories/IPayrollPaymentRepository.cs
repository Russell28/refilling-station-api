using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IPayrollPaymentRepository : IBaseRepository<PayrollPayment>, IDateRangeSearchableRepository<PayrollPayment>
    {

    }
}
