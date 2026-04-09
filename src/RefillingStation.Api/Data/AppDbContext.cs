using Microsoft.EntityFrameworkCore;
using RefillingStation.Api.Entities;

namespace RefillingStation.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<CustomerDebtEntry> CustomerDebtEntries => Set<CustomerDebtEntry>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
        public DbSet<MonthlyClosing> MonthlyClosings => Set<MonthlyClosing>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Constraint
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(x => x.Username)
                    .IsUnique();
            });

            // Trip
            modelBuilder.Entity<Trip>(entity =>
            {
                entity.Property(x => x.EmployeeName)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(x => x.Source).HasMaxLength(100);
                entity.Property(x => x.TripType).HasMaxLength(50);
                entity.Property(x => x.CustomerCategory).HasMaxLength(50);
                entity.Property(x => x.Notes).HasMaxLength(500);

                entity.Property(x => x.Date)
                    .HasConversion(
                        v => v.ToDateTime(TimeOnly.MinValue),
                        v => DateOnly.FromDateTime(v)
                    )
                    .HasColumnType("date");

                entity.HasIndex(x => new { x.Date, x.TripNumber }).IsUnique();
            });

            // Monthly Closing
            modelBuilder.Entity<MonthlyClosing>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Month)
                    .IsRequired()
                    .HasMaxLength(7);
                entity.HasIndex(x => x.Month)
                    .IsUnique();

                entity.Property(x => x.TotalCashCollected).HasColumnType("decimal(18,2)");
                entity.Property(x => x.TotalExpenses).HasColumnType("decimal(18,2)");
                entity.Property(x => x.TotalPayrollEarned).HasColumnType("decimal(18,2)");
                entity.Property(x => x.NetProfit).HasColumnType("decimal(18,2)");
                entity.Property(x => x.ManagerShare).HasColumnType("decimal(18,2)");
                entity.Property(x => x.OwnerShare).HasColumnType("decimal(18,2)");
            });
        }

    }
}
