using BlazorApp1.Data;
using BlazorApp1.Models;
using HelalRehber.Shared;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public sealed record CorrectionResult(int HttpStatus, string Message, CorrectionReceipt? Receipt = null);
public sealed class CorrectionService(IDbContextFactory<AppDbContext> factory, ProductCatalog products, RestaurantCatalog restaurants, OpenFoodFactsCatalog off)
{
    public static Task InitializeAsync(AppDbContext db) => db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS CorrectionSubmissions (
            Id TEXT NOT NULL PRIMARY KEY, Kind TEXT NOT NULL, TargetId TEXT NOT NULL,
            TargetName TEXT NOT NULL, Field TEXT NOT NULL, Suggestion TEXT NOT NULL,
            SourceUrl TEXT NOT NULL, Status TEXT NOT NULL DEFAULT 'pending', CreatedUtc TEXT NOT NULL
        )
        """);
    public async Task<string?> TargetNameAsync(string kind, string id, CancellationToken token = default)
    {
        if (!CorrectionRules.ValidTarget(kind, id)) return null;
        if (kind == "catalog") return (await products.GetDetailAsync(int.Parse(id)))?.Product.Name;
        if (kind == "restaurant")
        {
            var record = await restaurants.GetAsync(int.Parse(id));
            return record is null ? null : record.Restaurant.Name + " · " + record.Restaurant.Dish;
        }
        return (await off.FindAsync(id, token)).Product?.Name;
    }
    public async Task<CorrectionResult> SubmitAsync(CorrectionInput input, CancellationToken token = default)
    {
        var errors = CorrectionRules.Validate(input);
        if (errors.Count > 0) return new(400, string.Join(" ", errors));
        var suggestion = input.Suggestion.Trim(); var source = (input.SourceUrl ?? "").Trim();
        await using var db = await factory.CreateDbContextAsync(token);
        var existing = await db.CorrectionSubmissions.FindAsync([input.SubmissionId], token);
        CorrectionResult Existing(CorrectionSubmission row) => row.Kind == input.Kind && row.TargetId == input.TargetId && row.Field == input.Field && row.Suggestion == suggestion && row.SourceUrl == source
            ? new(200, "Bu öneri daha önce kaydedildi.", new(row.Id, row.Status, CorrectionRules.StatusLabel(row.Status) + "; katalog değiştirilmedi."))
            : new(409, "Bu gönderim kimliği başka bir öneride kullanıldı. Yeni öneri için formu yeniden açın.");
        if (existing is not null) return Existing(existing);
        var name = await TargetNameAsync(input.Kind, input.TargetId, token);
        if (name is null) return new(404, "Ürün/yemek bilgisi alınamadı. Kaydın detayını açıp tekrar deneyin.");
        var row = new CorrectionSubmission { Id = input.SubmissionId, Kind = input.Kind, TargetId = input.TargetId,
            TargetName = name, Field = input.Field, Suggestion = suggestion, SourceUrl = source, CreatedUtc = DateTime.UtcNow, Status = "pending" };
        db.CorrectionSubmissions.Add(row);
        try { await db.SaveChangesAsync(token); }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            existing = await db.CorrectionSubmissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.SubmissionId, token);
            if (existing is not null) return Existing(existing);
            throw;
        }
        return new(201, "Öneriniz incelemeye alındı. Katalog bilgisi doğrudan değiştirilmedi.", new(row.Id, row.Status, "İnceleme bekliyor"));
    }
}
