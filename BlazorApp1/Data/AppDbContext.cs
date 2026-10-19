using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<ProductMetadata> ProductMetadata => Set<ProductMetadata>();

    public DbSet<RestaurantMetadata> RestaurantMetadata => Set<RestaurantMetadata>();

    public DbSet<CorrectionSubmission> CorrectionSubmissions => Set<CorrectionSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CorrectionSubmission>().ToTable("CorrectionSubmissions");
        modelBuilder.Entity<CorrectionSubmission>().HasKey(c => c.Id);
        modelBuilder.Entity<RestaurantMetadata>().ToTable("RestaurantMetadata");
        modelBuilder.Entity<RestaurantMetadata>().HasKey(m => m.RestaurantId);
        modelBuilder.Entity<RestaurantMetadata>().HasOne<Restaurant>().WithOne().HasForeignKey<RestaurantMetadata>(m => m.RestaurantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProductMetadata>().ToTable("ProductMetadata");
        modelBuilder.Entity<ProductMetadata>().HasKey(m => m.ProductId);
        modelBuilder.Entity<ProductMetadata>().HasOne<Product>().WithOne()
            .HasForeignKey<ProductMetadata>(m => m.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
