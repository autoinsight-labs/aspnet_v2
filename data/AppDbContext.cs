using AutoInsight.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Yard> Yards => Set<Yard>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<VehicleModel>();

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("vehicles");
                entity.Property(v => v.Model).HasColumnType("vehicle_model");
                entity.Property(v => v.Plate).IsRequired();
                entity.Property(v => v.OwnerId).IsRequired();
            });
        }
    }
}
