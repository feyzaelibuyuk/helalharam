using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public sealed record RestaurantRecord(int Id, string Name, string Dish, string PossibleIngredients,
    string Questions, string InformationSource, string CountryCode, string City, string Branch,
    string Language, string ConfirmedIngredients, bool IsDemo, DateTime? LastCheckedUtc);
public sealed record DishDetail(RestaurantRecord Restaurant, string Questions, string Note);
public sealed class RestaurantCatalog(IDbContextFactory<AppDbContext> factory)
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS RestaurantMetadata (
                RestaurantId INTEGER NOT NULL PRIMARY KEY,
                CountryCode TEXT NOT NULL DEFAULT '', City TEXT NOT NULL DEFAULT '',
                Branch TEXT NOT NULL DEFAULT '', Language TEXT NOT NULL DEFAULT 'tr',
                ConfirmedIngredients TEXT NOT NULL DEFAULT '', IsDemo INTEGER NOT NULL DEFAULT 0,
                LastCheckedUtc TEXT NULL,
                FOREIGN KEY (RestaurantId) REFERENCES Restaurants(Id) ON DELETE CASCADE
            )
            """);
        var demos = await db.Restaurants.Where(r => r.InformationSource == "Örnek kayıt; işletme tarafından doğrulanmadı." &&
            (r.Name == "Örnek Esnaf Lokantası" || r.Name == "Örnek Izgara Salonu")).ToListAsync();
        foreach (var demo in demos)
            if (!await db.RestaurantMetadata.AnyAsync(m => m.RestaurantId == demo.Id))
                db.RestaurantMetadata.Add(new RestaurantMetadata { RestaurantId = demo.Id, CountryCode = "TR",
                    City = demo.Name == "Örnek Esnaf Lokantası" ? "İstanbul" : "Ankara", Branch = "Kurgu Merkez Şube", IsDemo = true });
        await db.SaveChangesAsync();
    }
    private static IQueryable<RestaurantRecord> Records(AppDbContext db, int? id = null) =>
        from r in db.Restaurants.AsNoTracking()
        where id == null || r.Id == id
        join m in db.RestaurantMetadata.AsNoTracking() on r.Id equals m.RestaurantId into metadata
        from m in metadata.DefaultIfEmpty()
        where m == null || m.CountryCode == "TR" || m.CountryCode == ""
        orderby r.Name
        select new RestaurantRecord(r.Id, r.Name, r.Dish, r.PossibleIngredients, r.Questions, r.InformationSource,
            m == null ? "" : m.CountryCode, m == null ? "" : m.City, m == null ? "" : m.Branch,
            m == null ? "" : m.Language, m == null ? "" : m.ConfirmedIngredients,
            m != null && m.IsDemo, m == null ? null : m.LastCheckedUtc);
    public async Task<List<RestaurantRecord>> SearchAsync(string? query, string? city)
    {
        await using var db = await factory.CreateDbContextAsync();
        var term = query?.Trim() ?? ""; var place = city?.Trim() ?? "";
        var records = await Records(db).ToListAsync();
        // Turkish city/dish comparison handles İ/i and I/ı consistently.
        var compare = System.Globalization.CultureInfo.GetCultureInfo("tr-TR").CompareInfo;
        bool Has(string value, string sought) => compare.IndexOf(value, sought, System.Globalization.CompareOptions.IgnoreCase) >= 0;
        return records.Where(r => (term.Length == 0 || Has(r.Name, term) || Has(r.Dish, term) || Has(r.PossibleIngredients, term) || Has(r.Branch, term)) &&
            (place.Length == 0 || Has(r.City, place))).ToList();
    }
    public async Task<DishDetail?> GetAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var record = await Records(db, id).FirstOrDefaultAsync();
        if (record is null) return null;
        var questions = (record.Questions.Length > 0 ? "Kayıttaki sorular:\n" + record.Questions + "\n\n" : "") +
            helalharam.MenuQuestions.Prepare(record.Dish + " " + record.PossibleIngredients, "tr");
        return new(record, questions, "İşletmeye danışın. Tarif, etin kaynağı, ortak ekipman ve varsa sertifikanın şube/üretim kapsamı doğrulanmalıdır. Bu ekran helal onayı vermez; restoran sertifikası doğrulanmış değildir.");
    }
}
