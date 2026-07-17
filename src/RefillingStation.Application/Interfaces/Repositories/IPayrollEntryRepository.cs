using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IPayrollEntryRepository : IBaseRepository<PayrollEntry>, IDateRangeSearchableRepository<PayrollEntry>
    {
        
    }
}
