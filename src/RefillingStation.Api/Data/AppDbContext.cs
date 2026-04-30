using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RefillingStation.Api.Entities;

namespace RefillingStation.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<CustomerDebtEntry> CustomerDebtEntries => Set<CustomerDebtEntry>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
        public DbSet<MonthlyClosing> MonthlyClosings => Set<MonthlyClosing>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                {
                    property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                        v => v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
            base.OnModelCreating(modelBuilder);

            // User Constraint
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasColumnType("citext")
                    .HasMaxLength(50);

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasColumnType("citext")
                    .HasMaxLength(20);

                entity.HasIndex(x => x.Username)
                    .IsUnique();
            });

            // Employee Constraint
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(x => x.FirstName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("citext");

                entity.Property(x => x.LastName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("citext");

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(15)
                    .HasColumnType("citext")
                    .IsRequired(false);

                entity.Property(x => x.Role)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .IsRequired();
            });

            // Trip
            modelBuilder.Entity<Trip>(entity =>
            {
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

            // Customer 
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });

            // Expense Category
            modelBuilder.Entity<ExpenseCategory>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("citext");
                entity.HasIndex(x => x.Name)
                    .IsUnique();
                entity.Property(x => x.Description)
                    .HasMaxLength(500)
                    .IsRequired(false);
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
