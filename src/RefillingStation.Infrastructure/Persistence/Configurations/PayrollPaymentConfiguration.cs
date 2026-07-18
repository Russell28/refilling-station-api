using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Configurations
{
    public class PayrollPaymentConfiguration : IEntityTypeConfiguration<PayrollPayment>
    {
        public void Configure(EntityTypeBuilder<PayrollPayment> builder)
        {
            builder.ToTable("PayrollPayments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PaidDate)
                .IsRequired();

            builder.Property(x => x.AmountPaid)
                .IsRequired();

            builder.HasOne(x => x.Employee)
                .WithMany(e => e.PayrollPayments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
