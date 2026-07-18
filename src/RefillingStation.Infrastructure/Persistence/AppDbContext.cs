using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) {}
        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<CustomerDebtEntry> CustomerDebtEntries => Set<CustomerDebtEntry>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
        public DbSet<PayrollPayment> PayrollPayments => Set<PayrollPayment>();
        public DbSet<MonthlyClosing> MonthlyClosings => Set<MonthlyClosing>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure all DateTime properties are stored as UTC
            ApplyUtcDateTimeConverter(modelBuilder);

            // Apply all configurations from the assembly containing AppDbContext (e.g., IEntityTypeConfiguration implementations)
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }
        private static void ApplyUtcDateTimeConverter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p =>
                        p.ClrType == typeof(DateTime) ||
                        p.ClrType == typeof(DateTime?)))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }
    }
}
