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
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<Record>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Location)
                .HasColumnType("geometry(Point,4326)")
                ;
            e.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
