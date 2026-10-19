using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazorApp1.Services;

public sealed class ReviewInput
{
    public Guid ActionId { get; set; }
    public int ExpectedRevision { get; set; }
    public string Status { get; set; } = "pending";
    public string Note { get; set; } = "";
}
public sealed record ReviewResult(int HttpStatus, string Message);
public sealed record ReviewDetail(CorrectionSubmission Submission, List<CorrectionReview> History, int Revision);
public sealed record ReviewQueue(List<CorrectionSubmission> Items, int Total, int Pending, int NeedsInfo, int Accepted, int Rejected, int Page);
public sealed class ReviewService(IDbContextFactory<AppDbContext> factory, AdminAccess access)
{
    public static readonly string[] Statuses = ["pending", "needs_info", "accepted", "rejected"];
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS CorrectionReviews (
                Id TEXT NOT NULL PRIMARY KEY, SubmissionId TEXT NOT NULL, Revision INTEGER NOT NULL,
                PreviousStatus TEXT NOT NULL, Status TEXT NOT NULL, Note TEXT NOT NULL,
                Reviewer TEXT NOT NULL, CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (SubmissionId) REFERENCES CorrectionSubmissions(Id) ON DELETE RESTRICT
            )
            """);
        await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_CorrectionReviews_SubmissionId_Revision ON CorrectionReviews(SubmissionId, Revision)");
    }
    private void Authorize(ClaimsPrincipal user) { if (!access.IsCurrent(user)) throw new UnauthorizedAccessException(); }
    public async Task<ReviewQueue> QueueAsync(ClaimsPrincipal user, string? status, string? kind, int page)
    {
        Authorize(user);
        await using var db = await factory.CreateDbContextAsync();
        var all = db.CorrectionSubmissions.AsNoTracking(); var query = all;
        if (Statuses.Contains(status)) query = query.Where(x => x.Status == status);
        if (kind is "catalog" or "off" or "restaurant") query = query.Where(x => x.Kind == kind);
        var total = await query.CountAsync(); page = Math.Clamp(page, 1, Math.Max(1, (total + 24) / 25));
        return new(await query.OrderByDescending(x => x.CreatedUtc).ThenBy(x => x.Id).Skip((page - 1) * 25).Take(25).ToListAsync(), total,
            await all.CountAsync(x => x.Status == "pending"), await all.CountAsync(x => x.Status == "needs_info"),
            await all.CountAsync(x => x.Status == "accepted"), await all.CountAsync(x => x.Status == "rejected"), page);
    }
    public async Task<ReviewDetail?> GetAsync(ClaimsPrincipal user, Guid id)
    {
        Authorize(user); await using var db = await factory.CreateDbContextAsync();
        var row = await db.CorrectionSubmissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (row is null) return null;
        var events = await db.CorrectionReviews.AsNoTracking().Where(x => x.SubmissionId == id).OrderByDescending(x => x.Revision).ToListAsync();
        return new(row, events, events.FirstOrDefault()?.Revision ?? 0);
    }
    public async Task<ReviewResult> SaveAsync(ClaimsPrincipal user, Guid id, ReviewInput input)
    {
        Authorize(user);
        if (input.ActionId == Guid.Empty || !Statuses.Contains(input.Status) || input.ExpectedRevision < 0 || (input.Note?.Trim().Length ?? 0) is < 5 or > 2000)
            return new(400, "Durum seçin ve 5–2.000 karakter inceleme notu girin.");
        var note = input.Note!.Trim();
        await using var db = await factory.CreateDbContextAsync();
        ReviewResult Same(CorrectionReview existing) => existing.SubmissionId == id && existing.Status == input.Status && existing.Note == note
            ? new(200, "Bu inceleme daha önce kaydedildi.") : new(409, "İşlem kimliği başka bir incelemede kullanıldı. Formu yenileyin.");
        var previous = await db.CorrectionReviews.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.ActionId);
        if (previous is not null) return Same(previous);
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var row = await db.CorrectionSubmissions.SingleOrDefaultAsync(x => x.Id == id);
            if (row is null) return new(404, "Gönderim bulunamadı.");
            var revision = await db.CorrectionReviews.Where(x => x.SubmissionId == id).Select(x => (int?)x.Revision).MaxAsync() ?? 0;
            if (revision != input.ExpectedRevision) return new(409, "Bu kayıt başka bir sekmede değişti. Notunuzu kopyalayıp formu yenileyin.");
            db.CorrectionReviews.Add(new CorrectionReview { Id = input.ActionId, SubmissionId = id, Revision = revision + 1,
                PreviousStatus = row.Status, Status = input.Status, Note = note, Reviewer = user.Identity?.Name ?? "admin", CreatedUtc = DateTime.UtcNow });
            row.Status = input.Status;
            await db.SaveChangesAsync(); await transaction.CommitAsync();
            return new(200, "İnceleme kaydedildi. Katalog bilgisi değiştirilmedi.");
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            previous = await db.CorrectionReviews.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.ActionId);
            return previous is not null ? Same(previous) : new(409, "Kayıt eşzamanlı değişti. Formu yenileyin.");
        }
    }
}
