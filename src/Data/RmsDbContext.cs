using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RmsDemo.Models;

namespace RmsDemo.Data;

public class RmsDbContext : DbContext
{
    public RmsDbContext(DbContextOptions<RmsDbContext> options) : base(options) { }

    public DbSet<Record> Records => Set<Record>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var isNpgsql = Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

        if (isNpgsql)
        {
            modelBuilder.HasPostgresExtension("postgis");
        }

        modelBuilder.Entity<Record>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            if (isNpgsql)
            {
                e.Property(x => x.Location).HasColumnType("geometry(Point,4326)");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                // InMemory provider: no relational column types or SQL defaults
                e.Property(x => x.Location);
            }
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
