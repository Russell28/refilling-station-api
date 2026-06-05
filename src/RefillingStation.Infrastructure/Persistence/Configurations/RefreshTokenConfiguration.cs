using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RefillingStation.Domain.Enitities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            // Primary key
            builder.HasKey(x => x.Id);

            // Relationships
            builder.HasOne(x => x.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Properties
            builder.Property(x => x.Token)
                   .IsRequired()
                   .HasMaxLength(200); // adjust length depending on token size

            builder.HasIndex(x => x.Token)
                   .IsUnique();

            builder.Property(x => x.ExpiresAt)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.RevokedAt)
                   .IsRequired(false);

            builder.Property(x => x.IsRevoked)
                   .IsRequired();
        }
    }
}
