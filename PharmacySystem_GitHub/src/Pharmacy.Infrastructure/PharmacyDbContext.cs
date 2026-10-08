using Microsoft.EntityFrameworkCore;
using Pharmacy.Domain;

namespace Pharmacy.Infrastructure;

public sealed class PharmacyDbContext : DbContext
{
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Sale> Sales => Set<Sale>();

    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Category).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Price).HasPrecision(10, 2);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Total).HasPrecision(10, 2);
        });
    }
}
