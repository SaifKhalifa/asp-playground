using AspPlayground.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspPlayground.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>(e =>
        {
            e.ToTable("Vehicles", t => t.HasCheckConstraint("CK_Vehicles_Price_Positive", "[Price] > 0"));
            e.ToTable("Vehicles", t => t.HasCheckConstraint("CK_Vehicles_Mileage_Positive", "[Mileage] >= 0"));


            e.HasKey(v => v.Id);
            e.Property(v => v.Id).ValueGeneratedOnAdd();

            e.Property(v => v.Brand).HasMaxLength(100).IsRequired();
            e.Property(v => v.Model).HasMaxLength(100).IsRequired();

            e.Property(v => v.ManufacturingYear).IsRequired();
            e.Property(v => v.Price).HasColumnType("decimal(12,2)").IsRequired();
            e.Property(v => v.Color).HasMaxLength(50).IsRequired();
            e.Property(v => v.Mileage).IsRequired();

            e.Property(v => v.AvailabilityStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(AvailabilityStatus.Available);
        });
    }
}
