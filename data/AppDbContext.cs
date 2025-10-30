using AutoInsight.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoInsight.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Yard> Yards => Set<Yard>();
    }
}
