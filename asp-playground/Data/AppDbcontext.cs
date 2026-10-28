using asp_playground.Entities;
using Microsoft.EntityFrameworkCore;

namespace asp_playground.Data;

public class AppDbcontext(DbContextOptions<AppDbcontext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserProfile>(u =>
        {
            u.HasKey(p => p.Id);
            u.Property(p => p.Name).IsRequired().HasMaxLength(100);
            u.Property(p => p.Email).IsRequired().HasMaxLength(100);
            u.HasIndex(p => p.Email).IsUnique();
            u.Property(p => p.HashedPassword).IsRequired().HasMaxLength(500);
            u.Property(p => p.Role).HasConversion<string>().HasMaxLength(20);
        });
    }
}
