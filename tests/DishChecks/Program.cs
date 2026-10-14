using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
await using var db = new AppDbContext(options);
await db.Database.EnsureCreatedAsync();
var legacy = new Restaurant { Name = "Eski kullanıcı işletmesi", Dish = "Pilav", InformationSource = "Kullanıcı beyanı", Questions = "Yağ kaynağı?" };
var demo = new Restaurant { Name = "Örnek Esnaf Lokantası", Dish = "Mercimek çorbası", InformationSource = "Örnek kayıt; işletme tarafından doğrulanmadı." };
var foreign = new Restaurant { Name = "Foreign fixture", Dish = "Soup" };
db.Restaurants.AddRange(legacy, demo, foreign); await db.SaveChangesAsync();
await db.Database.ExecuteSqlRawAsync("DROP TABLE RestaurantMetadata");
await RestaurantCatalog.InitializeAsync(db);
db.RestaurantMetadata.Add(new RestaurantMetadata { RestaurantId = foreign.Id, CountryCode = "DE", City = "Berlin" }); await db.SaveChangesAsync();
await RestaurantCatalog.InitializeAsync(db);
void Check(bool value, string message) { if (!value) throw new Exception(message); }
Check(await db.Restaurants.CountAsync() == 3, "Eski kayıtlar değişti");
Check(await db.RestaurantMetadata.CountAsync() == 2, "Başlangıç çoğalttı");
var catalog = new RestaurantCatalog(new Factory(options));
var old = await catalog.GetAsync(legacy.Id);
Check(old is { Restaurant.CountryCode: "", Restaurant.City: "", Restaurant.IsDemo: false }, "Eski işletmeye kurgu coğrafya atandı");
Check(old!.Questions.Contains("Yağ kaynağı?") && old.Questions.Contains("Et suyu"), "Eski sorular/bulyon sorusu kayboldu");
Check((await catalog.SearchAsync("", "İSTANBUL")).Single().Id == demo.Id, "Türkçe şehir filtresi");
Check((await catalog.SearchAsync("", "istanbul")).Single().Id == demo.Id, "Türkçe küçük i filtresi");
Check((await catalog.SearchAsync("PİLAV", "")).Single().Id == legacy.Id, "Yemek araması");
Check((await catalog.SearchAsync("", "Berlin")).Count == 0 && await catalog.GetAsync(foreign.Id) is null, "Türkiye kapsamı dışı kayıt gösterildi");
Check(await catalog.GetAsync(-1) is null, "Eksik yemek kontrolü");
Check((await catalog.GetAsync(demo.Id)) is { Restaurant.IsDemo: true, Restaurant.ConfirmedIngredients: "" }, "Demo doğrulanmış içerik kazandı");
Check((await catalog.SearchAsync("", "Bursa")).Count == 0, "Şehir için kayıt uyduruldu");
Console.WriteLine("PASS: restoran veri korunması, tekrar başlangıç, Türkiye kapsamı, Türkçe şehir/yemek filtresi, demo ayrımı ve eksik kayıt.");
sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new(options);
    public Task<AppDbContext> CreateDbContextAsync(CancellationToken token = default) => Task.FromResult(new AppDbContext(options));
}
