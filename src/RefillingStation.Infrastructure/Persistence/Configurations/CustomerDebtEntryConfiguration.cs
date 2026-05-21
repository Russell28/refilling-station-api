using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Configurations
{
    public class CustomerDebtEntryConfiguration : IEntityTypeConfiguration<CustomerDebtEntry>
    {
        public void Configure(EntityTypeBuilder<CustomerDebtEntry> builder)
        {
            builder.ToTable("CustomerDebtEntries");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.Amount)
                .IsRequired();

            builder.HasOne(x => x.Customer)
                .WithMany(c => c.CustomerDebtEntries)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
