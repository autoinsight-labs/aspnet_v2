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
            modelBuilder.HasPostgresEnum<EmployeeRole>();

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("vehicles");
                entity.Property(v => v.Model).HasColumnType("vehicle_model");
                entity.Property(v => v.Plate).IsRequired();
                entity.Property(v => v.OwnerId).IsRequired();
            });

            modelBuilder.Entity<YardEmployee>(entity =>
            {
                entity.ToTable("yard_employees");
                entity.Property(e => e.Role).HasColumnType("employee_role");
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.YardId).IsRequired();

                entity.HasOne(e => e.Yard)
                      .WithMany(y => y.Employees)
                      .HasForeignKey(e => e.YardId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
