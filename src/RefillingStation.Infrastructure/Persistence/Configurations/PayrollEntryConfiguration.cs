using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Configurations
{
    public class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
    {
        public void Configure(EntityTypeBuilder<PayrollEntry> builder)
        {
            builder.ToTable("PayrollEntries");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EarnedDate)
                .IsRequired();

            builder.HasOne(x => x.Employee)
                .WithMany(e => e.PayrollEntries)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
