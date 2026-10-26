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

    public DbSet<CorrectionReview> CorrectionReviews => Set<CorrectionReview>();

    public DbSet<FeedbackSubmission> FeedbackSubmissions => Set<FeedbackSubmission>();

    public DbSet<CertificateEvidence> CertificateEvidence => Set<CertificateEvidence>();
    public DbSet<CertificateEvidenceReview> CertificateEvidenceReviews => Set<CertificateEvidenceReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CertificateEvidence>().ToTable("CertificateEvidence");
        modelBuilder.Entity<CertificateEvidence>().Property(x=>x.Revision).IsConcurrencyToken();
        modelBuilder.Entity<CertificateEvidenceReview>().ToTable("CertificateEvidenceReviews");
        modelBuilder.Entity<CertificateEvidenceReview>().HasIndex(x=>new{x.SubmissionId,x.Revision}).IsUnique();
        modelBuilder.Entity<CertificateEvidenceReview>().HasOne<CertificateEvidence>().WithMany().HasForeignKey(x=>x.SubmissionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<FeedbackSubmission>().ToTable("FeedbackSubmissions");
        modelBuilder.Entity<FeedbackSubmission>().HasKey(x => x.Id);
        modelBuilder.Entity<CorrectionReview>().ToTable("CorrectionReviews");
        modelBuilder.Entity<CorrectionReview>().HasKey(r => r.Id);
        modelBuilder.Entity<CorrectionReview>().HasIndex(r => new { r.SubmissionId, r.Revision }).IsUnique();
        modelBuilder.Entity<CorrectionReview>().HasOne<CorrectionSubmission>().WithMany().HasForeignKey(r => r.SubmissionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CorrectionSubmission>().Property(c => c.Status).IsConcurrencyToken();
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
