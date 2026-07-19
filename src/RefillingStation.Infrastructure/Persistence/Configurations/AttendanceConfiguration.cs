

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RefillingStation.Domain.Enitities;

namespace RefillingStation.Infrastructure.Persistence.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.ToTable("Attendances");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.EmployeeId)
                .IsRequired();

            builder.Property(x => x.IsPresent)
                .IsRequired();

            // Relationship
            builder.HasOne(x => x.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(x => x.EmployeeId);

            // Unique constraint: one record per employee per date
            builder.HasIndex(x => new { x.Date, x.EmployeeId })
                .IsUnique();
        }
    }
}
