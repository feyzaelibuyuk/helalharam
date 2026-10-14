using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<ProductMetadata> ProductMetadata => Set<ProductMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductMetadata>().ToTable("ProductMetadata");
        modelBuilder.Entity<ProductMetadata>().HasKey(m => m.ProductId);
        modelBuilder.Entity<ProductMetadata>().HasOne<Product>().WithOne()
            .HasForeignKey<ProductMetadata>(m => m.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
