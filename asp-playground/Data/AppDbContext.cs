using asp_playground.Entities;
using Microsoft.EntityFrameworkCore;

namespace asp_playground.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Donation> Donations => Set<Donation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Donation>(e =>
        {
            e.ToTable("Donations", t => t.HasCheckConstraint("CK_Donations_Amount_Positive", "[Amount] > 0"));
            e.HasKey(d => d.Id);
            e.Property(d => d.Amount).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(d => d.DonorName).HasMaxLength(100).IsRequired();
            e.Property(d => d.IsAnonymous).IsRequired();
            e.Property(d => d.CreatedAt).IsRequired();
            e.HasIndex(d => d.CreatedAt);
        });
    }
}
