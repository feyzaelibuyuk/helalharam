using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
var factory = new Factory(options);
await using var db = new AppDbContext(options);
await db.Database.EnsureCreatedAsync();
var original = new Product { Name = "Eski kullanıcı kaydı", Barcode = "OLD", Note = "Korunacak not", Source = "Kullanıcı kaydı" };
db.Products.Add(original); await db.SaveChangesAsync();
await db.Database.ExecuteSqlRawAsync("DROP TABLE ProductMetadata");
await CatalogSetup.InitializeAsync(db);
var count = await db.Products.CountAsync();
await CatalogSetup.InitializeAsync(db);
Check(await db.Products.CountAsync() == count, "Tekrar başlangıçta demo kayıt çoğalmamalı");
Check((await db.Products.FindAsync(original.Id))?.Note == "Korunacak not", "Eski ürün verisi korunmalı");
var catalog = new ProductCatalog(factory);
var unknown = await catalog.GetDetailAsync(original.Id);
Check(unknown is { Product.CountryCode: "", Alternatives.Count: 0 }, "Metadatasız eski kayıt uygunluk kazanmamalı");
Check(await catalog.GetDetailAsync(-1) is null, "Bulunmayan id boş dönmeli");
Check(ProductCatalog.ValidCountry("tr") && !ProductCatalog.ValidCountry("TUR") && !ProductCatalog.ValidCountry("12"), "Ülke kodu doğrulama");

async Task<Product> Add(string name, string barcode, string country, string category, bool verified, string status = "Uygun", bool demo = false, string url = "https://example.com/test-evidence", DateTime? checkedOn = null)
{
    var p = new Product { Name = name, Brand = "Test fixture", Barcode = barcode, Assessment = status, Source = "Test kaynağı" };
    db.Products.Add(p); await db.SaveChangesAsync();
    db.ProductMetadata.Add(new ProductMetadata { ProductId = p.Id, CountryCode = country, Category = category,
        IsVerified = verified, SourceUrl = url, LastCheckedUtc = checkedOn ?? DateTime.UtcNow.AddDays(-1), IsDemo = demo });
    await db.SaveChangesAsync(); return p;
}
var queried = await Add("Sorgulanan", "SAME", "TR", "Çikolata", false, "Şüpheli gıda");
var good = await Add("Kaynaklı alternatif", "GOOD", "TR", "Çikolata", true);
await Add("Başka ülke", "SAME", "DE", "Çikolata", true);
await Add("Başka kategori", "WRONGCAT", "TR", "Yoğurt", true);
await Add("Doğrulanmamış", "UNVERIFIED", "TR", "Çikolata", false);
await Add("Şüpheli", "SUSPECT", "TR", "Çikolata", true, "Şüpheli gıda");
await Add("Demo gerçek öneriye karışmamalı", "DEMO-MIX", "TR", "Çikolata", true, demo: true);
await Add("Geçersiz kaynak", "BADURL", "TR", "Çikolata", true, url: "javascript:alert(1)");
await Add("Gelecek tarih", "FUTURE", "TR", "Çikolata", true, checkedOn: DateTime.UtcNow.AddDays(1));
var detail = await catalog.GetDetailAsync(queried.Id);
Check(detail?.Alternatives.Count == 1 && detail.Alternatives[0].Product.Id == good.Id, "Alternatifler ülke, kategori, uygunluk, kanıt ve demo ayrımını sağlamalı");
Check((await catalog.SearchAsync("SAME", exactBarcode: true)).Count == 2, "Aynı barkodun ülke varyantları korunmalı");
Check((await catalog.SearchAsync("SAME", "tr", exactBarcode: true)).Single().Id == queried.Id, "Ülke filtresi ve küçük harf kodu");
Check((await catalog.SearchAsync("MISSING", exactBarcode: true)).Count == 0, "Bilinmeyen barkod boş sonuç");
var demoProducts = await catalog.SearchAsync("DEMO-CHOCO-TR", exactBarcode: true);
var demoDetail = await catalog.GetDetailAsync(demoProducts.Single().Id);
Check(demoDetail is { Product.IsDemo: true } && demoDetail.AlternativeMessage.Contains("gerçek ürün tavsiyesi değildir"), "Demo uygun ürün diye sunulmamalı");
Console.WriteLine("PASS: mevcut veri korunması, tekrar başlangıç, ülke/barkod, boş sonuç ve alternatif güvenilirliği kontrolleri.");
static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new(options);
    public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
}
