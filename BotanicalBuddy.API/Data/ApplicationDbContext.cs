using BotanicalBuddy.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BotanicalBuddy.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<UserPlant> UserPlants { get; set; } = null!;
    public DbSet<PlantCareLog> PlantCareLogs { get; set; } = null!;
    public DbSet<WeatherData> WeatherData { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // Address entity configuration
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // UserPlant entity configuration
        modelBuilder.Entity<UserPlant>(entity =>
        {
            entity.HasOne(up => up.User)
                .WithMany(u => u.UserPlants)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Address)
                .WithMany(a => a.UserPlants)
                .HasForeignKey(up => up.AddressId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // PlantCareLog entity configuration
        modelBuilder.Entity<PlantCareLog>(entity =>
        {
            entity.HasOne(pcl => pcl.UserPlant)
                .WithMany(up => up.CareLogs)
                .HasForeignKey(pcl => pcl.UserPlantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DateTime);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        // WeatherData entity configuration
        modelBuilder.Entity<WeatherData>(entity =>
        {
            entity.HasOne(wd => wd.Address)
                .WithMany(a => a.WeatherData)
                .HasForeignKey(wd => wd.AddressId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.AddressId, e.Date }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        // Subscription entity configuration
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // Seed subscription tier data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Note: Subscription tiers will be managed at the application level
        // This is just a placeholder for future seed data if needed
    }
}
