using BlazorApp1.Data;
using BlazorApp1.Models;
using HelalRehber.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BlazorApp1.Services;
public sealed record FeedbackResult(int HttpStatus, string Message, Guid? Receipt = null);
public sealed record FeedbackQueue(List<FeedbackSubmission> Items, int Total, int Page);
public sealed class FeedbackService(IDbContextFactory<AppDbContext> factory, AdminAccess access)
{
    public static Task InitializeAsync(AppDbContext db) => db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS FeedbackSubmissions (
        Id TEXT NOT NULL PRIMARY KEY, Category TEXT NOT NULL, Message TEXT NOT NULL,
        Client TEXT NOT NULL, Version TEXT NOT NULL, CreatedUtc TEXT NOT NULL)
        """);
    public async Task<FeedbackResult> SubmitAsync(FeedbackInput input, CancellationToken token = default)
    {
        var errors = FeedbackRules.Validate(input);
        if (errors.Count > 0) return new(400, string.Join(" ", errors));
        var message = input.Message.Trim(); var version = input.Version?.Trim() ?? "";
        await using var db = await factory.CreateDbContextAsync(token);
        FeedbackResult Existing(FeedbackSubmission row) => row.Category == input.Category && row.Message == message && row.Client == input.Client && row.Version == version
            ? new(200, "Bu geri bildirim daha önce kaydedildi.", row.Id)
            : new(409, "Gönderim kimliği başka bir geri bildirimde kullanıldı. Yeni gönderim için formu yeniden açın.");
        var existing = await db.FeedbackSubmissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.SubmissionId, token);
        if (existing is not null) return Existing(existing);
        var row = new FeedbackSubmission { Id = input.SubmissionId, Category = input.Category, Message = message, Client = input.Client, Version = version, CreatedUtc = DateTime.UtcNow };
        db.FeedbackSubmissions.Add(row);
        try { await db.SaveChangesAsync(token); }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            existing = await db.FeedbackSubmissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.SubmissionId, token);
            if (existing is not null) return Existing(existing);
            throw;
        }
        return new(201, "Geri bildiriminiz kaydedildi. Teşekkür ederiz.", row.Id);
    }
    public async Task<FeedbackQueue> QueueAsync(ClaimsPrincipal user, int page)
    {
        if (!access.IsCurrent(user)) throw new UnauthorizedAccessException();
        await using var db = await factory.CreateDbContextAsync();
        var query = db.FeedbackSubmissions.AsNoTracking(); var total = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (total + 24) / 25));
        return new(await query.OrderByDescending(x => x.CreatedUtc).ThenBy(x => x.Id).Skip((page - 1) * 25).Take(25).ToListAsync(), total, page);
    }
}
