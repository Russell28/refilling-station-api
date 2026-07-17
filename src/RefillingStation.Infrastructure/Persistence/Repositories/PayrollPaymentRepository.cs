using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Enitities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class PayrollPaymentRepository : BaseRepository<PayrollPayment>, IPayrollPaymentRepository
    {
        public PayrollPaymentRepository(AppDbContext context) : base(context) { }

        public override async Task<List<PayrollPayment>> GetAllAsync()
            => await _context.PayrollPayments
                .AsNoTracking()
                .Include(x => x.Employee)
                .ToListAsync();

        public override async Task<PayrollPayment?> GetByIdAsync(int id)
            => await _context.PayrollPayments
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);
    }
}
