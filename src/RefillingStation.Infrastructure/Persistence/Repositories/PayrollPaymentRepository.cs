using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

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

        public async Task<List<PayrollPayment>> SearchByDateRangeAsync(DateRangeOptions options)
        {
            const int PageSize = 50;

            var query = _context.PayrollPayments
                .AsNoTracking()
                .Include(x => x.Employee)
                .AsQueryable();

            if (options.StartDate.HasValue)
                query = query.Where(x => x.PaidDate >= options.StartDate.Value);

            if (options.EndDate.HasValue)
                query = query.Where(x => x.PaidDate <= options.EndDate.Value);

            return await query
                .OrderByDescending(x => x.PaidDate)
                .ThenBy(p => p.Employee.FirstName)
                .ThenBy(p => p.Employee.LastName)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
