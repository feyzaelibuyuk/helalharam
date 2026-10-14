using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public static class CatalogSetup
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        // EnsureCreated mevcut veritabanına yeni tablo eklemez. Bu eklemeli geçiş
        // Products/Restaurants tablolarını ve kullanıcının kayıtlarını değiştirmez.
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ProductMetadata" (
                "ProductId" INTEGER NOT NULL PRIMARY KEY,
                "Category" TEXT NOT NULL DEFAULT '',
                "CountryCode" TEXT NOT NULL DEFAULT '',
                "IngredientsLanguage" TEXT NOT NULL DEFAULT '',
                "OriginalIngredients" TEXT NOT NULL DEFAULT '',
                "SourceUrl" TEXT NOT NULL DEFAULT '',
                "LastCheckedUtc" TEXT NULL,
                "IsVerified" INTEGER NOT NULL DEFAULT 0,
                "IsDemo" INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
            );
            """);
        const string source = "Demo kayıt; üretici bilgisiyle doğrulanmadı.";
        var demoDefinitions = new[]
        {
            (Barcode: "8690000000011", Category: "Çikolata"),
            (Barcode: "8690000000028", Category: "Yoğurt"),
            (Barcode: "8690000000035", Category: "Kraker"),
            (Barcode: "DEMO-CHOCO-TR", Category: "Çikolata"),
            (Barcode: "DEMO-YOGURT-TR", Category: "Yoğurt")
        };
        foreach (var definition in demoDefinitions)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Barcode == definition.Barcode && p.Source == source);
            if (product is null && definition.Barcode.StartsWith("DEMO-"))
            {
                product = new Product
                {
                    Name = definition.Category == "Çikolata" ? "Örnek Sade Çikolata" : "Örnek Sade Yoğurt",
                    Brand = "Örnek Marka", Barcode = definition.Barcode,
                    Ingredients = definition.Category == "Çikolata" ? "Kakao kitlesi, kakao yağı, şeker." : "Süt, yoğurt kültürü.",
                    Assessment = "Yetersiz bilgi", Source = source,
                    Note = "Alternatif akışını göstermek için kurgu kayıt. Gerçek uygunluk ve sertifika bilgisi doğrulanmadı."
                };
                db.Products.Add(product);
                await db.SaveChangesAsync();
            }
            if (product is null || await db.ProductMetadata.AnyAsync(m => m.ProductId == product.Id)) continue;
            db.ProductMetadata.Add(new ProductMetadata
            {
                ProductId = product.Id, Category = definition.Category, CountryCode = "TR",
                IngredientsLanguage = "tr", OriginalIngredients = product.Ingredients, IsDemo = true
            });
            await db.SaveChangesAsync();
        }
    }
}

public sealed class ProductCatalog(IDbContextFactory<AppDbContext> factory)
{
    public static bool ValidCountry(string? country) => string.IsNullOrWhiteSpace(country) ||
        (country.Trim().Length == 2 && country.Trim().All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z'));

    private static IQueryable<ProductRecord> Records(AppDbContext db) =>
        from p in db.Products.AsNoTracking()
        join info in db.ProductMetadata.AsNoTracking() on p.Id equals info.ProductId into metadata
        from m in metadata.DefaultIfEmpty()
        select new ProductRecord
        {
            Id = p.Id, Name = p.Name, Brand = p.Brand, Barcode = p.Barcode,
            Ingredients = p.Ingredients, Assessment = p.Assessment, Note = p.Note, Source = p.Source,
            Category = m == null ? "" : m.Category, CountryCode = m == null ? "" : m.CountryCode,
            IngredientsLanguage = m == null ? "" : m.IngredientsLanguage,
            OriginalIngredients = m == null ? "" : m.OriginalIngredients,
            SourceUrl = m == null ? "" : m.SourceUrl, LastCheckedUtc = m == null ? null : m.LastCheckedUtc,
            IsVerified = m != null && m.IsVerified, IsDemo = m != null && m.IsDemo
        };
    public async Task<List<ProductRecord>> SearchAsync(string? query, string? country = null, bool exactBarcode = false)
    {
        await using var db = await factory.CreateDbContextAsync();
        var records = Records(db);
        var term = query?.Trim() ?? "";
        var market = country?.Trim().ToUpperInvariant() ?? "";
        if (market.Length > 0) records = records.Where(p => p.CountryCode == market);
        if (term.Length > 0)
            records = exactBarcode ? records.Where(p => p.Barcode == term) :
                records.Where(p => p.Name.Contains(term) || p.Brand.Contains(term) || p.Barcode.Contains(term) || p.Ingredients.Contains(term));
        return await records.OrderBy(p => p.Name).ThenBy(p => p.Id).ToListAsync();
    }

    public async Task<ProductDetail?> GetDetailAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var product = await Records(db).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return null;
        var alternatives = new List<ProductAlternative>();
        if (product.Category.Length > 0 && product.CountryCode.Length > 0)
        {
            var candidates = await Records(db).Where(p => p.Id != id &&
                p.Category == product.Category && p.CountryCode == product.CountryCode).OrderBy(p => p.Name).ToListAsync();
            foreach (var candidate in candidates)
            {
                if (product.IsDemo && candidate.IsDemo && candidate.Barcode.StartsWith("DEMO-"))
                    alternatives.Add(new(candidate, "Aynı kategori ve ülke için kurgu alternatif örneği; uygunluğu doğrulanmadı."));
                else if (!product.IsDemo && !candidate.IsDemo && candidate.Assessment == "Uygun" &&
                    candidate.IsVerified && candidate.LastCheckedUtc is not null &&
                    candidate.LastCheckedUtc <= DateTime.UtcNow &&
                    Uri.TryCreate(candidate.SourceUrl, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
                    alternatives.Add(new(candidate, "Aynı kategori ve ülke; kaynaklı değerlendirmesi uygun olarak doğrulanmış."));
            }
        }
        return new(product, alternatives.Take(5).ToList(), product.IsDemo
            ? "Demo alternatifleri gerçek ürün tavsiyesi değildir. Henüz doğrulanmış katalog bağlantısı yok."
            : alternatives.Count == 0 ? "Bu kategori ve ülke için doğrulanmış uygun alternatif bulunamadı."
            : "Alternatifler seçili helal profili için kayıtlı değerlendirmelere dayanır; sertifika yerine geçmez.");
    }
}
