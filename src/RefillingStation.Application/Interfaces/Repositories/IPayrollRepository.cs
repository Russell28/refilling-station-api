using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IPayrollRepository : IBaseRepository<PayrollEntry>, IDateRangeSearchableRepository<PayrollEntry>
    {
        
    }
}
