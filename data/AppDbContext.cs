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
            modelBuilder.HasPostgresEnum<VehicleStatus>();

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

            modelBuilder.Entity<YardVehicle>(entity =>
            {
                entity.ToTable("yard_vehicles");
                entity.Property(e => e.Status).HasColumnType("vehicle_status");
                entity.Property(e => e.EnteredAt);
                entity.Property(e => e.LeftAt);

                entity.HasOne(e => e.Vehicle)
                      .WithMany()
                      .HasForeignKey(e => e.VehicleId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Yard)
                      .WithMany(y => y.Vehicles)
                      .HasForeignKey(e => e.YardId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
