using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ICustomerDebtRepository : IBaseRepository<CustomerDebtEntry>, IDateRangeSearchableRepository<CustomerDebtEntry>
    {
        
    }
}
