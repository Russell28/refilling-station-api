using Microsoft.EntityFrameworkCore;
using RefillingStation.Api.Entities;

namespace RefillingStation.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

        public DbSet<Trip> Trips => Set<Trip>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.Property(x => x.Source).HasMaxLength(100);
                entity.Property(x => x.TripType).HasMaxLength(100);
                entity.Property(x => x.Employee).HasMaxLength(100);
                entity.Property(x => x.CustomerType).HasMaxLength(100);
                entity.Property(x => x.Notes).HasMaxLength(500);
            });
        }
        
    }
}
