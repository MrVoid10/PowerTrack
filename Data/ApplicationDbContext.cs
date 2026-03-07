using Microsoft.EntityFrameworkCore;
using PowerTrack.Models;

namespace PowerTrack.Data
{
  public class ApplicationDbContext : DbContext
  {
    public DbSet<User> Users { get; set; }
    public DbSet<EnergyConsumption> EnergyConsumptions { get; set; }
    public DbSet<Tariff> Tariffs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Set precision and scale for decimal columns
      modelBuilder.Entity<EnergyConsumption>()
          .Property(e => e.ConsumptionKWh)
          .HasPrecision(10, 2);

      modelBuilder.Entity<EnergyConsumption>()
          .Property(e => e.PricePerKWh)
          .HasPrecision(10, 4);

      modelBuilder.Entity<Tariff>()
          .Property(t => t.PricePerKWh)
          .HasPrecision(10, 4);
    }
  }
}
