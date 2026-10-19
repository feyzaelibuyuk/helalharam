using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services;
using HelalRehber.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
var factory = new Factory(options);
await using var db = new AppDbContext(options); await db.Database.EnsureCreatedAsync();
var product = new Product { Name = "Test ürünü", Ingredients = "Eski içerik", Assessment = "Eski değerlendirme" };
var restaurant = new Restaurant { Name = "Test işletmesi", Dish = "Pilav", InformationSource = "Eski kaynak" };
db.Products.Add(product); db.Restaurants.Add(restaurant); await db.SaveChangesAsync();
await RestaurantCatalog.InitializeAsync(db);
await db.Database.ExecuteSqlRawAsync("DROP TABLE CorrectionSubmissions");
await CorrectionService.InitializeAsync(db); await CorrectionService.InitializeAsync(db);
using var cache = new MemoryCache(new MemoryCacheOptions());
var off = new OpenFoodFactsCatalog(new Clients(), cache);
var service = new CorrectionService(factory, new ProductCatalog(factory), new RestaurantCatalog(factory), off);
void Check(bool value, string reason) { if (!value) throw new Exception(reason); }
CorrectionInput Input(string kind, string id) => new() { SubmissionId = Guid.NewGuid(), Kind = kind, TargetId = id, Field = "ingredients", Suggestion = "Test önerisi: içerik bilgisi eksik." };
var input = Input("catalog", product.Id.ToString());
var created = await service.SubmitAsync(input);
Check(created.HttpStatus == 201 && created.Receipt?.Status == "pending", "Bekleyen öneri kaydedilemedi");
Check((await service.SubmitAsync(input)).HttpStatus == 200 && await db.CorrectionSubmissions.CountAsync() == 1, "Tekrar gönderim çoğaldı");
input.Suggestion = "Aynı kimlik ile farklı metin gönderildi";
Check((await service.SubmitAsync(input)).HttpStatus == 409, "Kimlik çakışması kabul edildi");
Check((await service.SubmitAsync(Input("restaurant", restaurant.Id.ToString()))).HttpStatus == 201, "Yemek önerisi kaydedilmedi");
Check((await service.SubmitAsync(Input("off", "3017620422003"))).HttpStatus == 201, "OFF önerisi kaydedilmedi");
Check((await service.SubmitAsync(Input("catalog", "9999"))).HttpStatus == 404, "Olmayan ürün kabul edildi");
var bad = Input("catalog", product.Id.ToString()); bad.SourceUrl = "javascript:alert(1)";
Check((await service.SubmitAsync(bad)).HttpStatus == 400, "Tehlikeli URL kabul edildi");
bad.SourceUrl = "https://user:password@example.com";
Check(CorrectionRules.Validate(bad).Count > 0, "Şifre içeren kaynak kabul edildi");
bad.SourceUrl = "https://example.com/kaynak"; bad.Suggestion = "kısa";
Check(CorrectionRules.Validate(bad).Count > 0, "Kısa metin kabul edildi");
bad.Suggestion = new string('x', 2001);
Check(CorrectionRules.Validate(bad).Count > 0, "Uzun metin kabul edildi");
bad = Input("catalog", product.Id.ToString()); bad.SourceUrl = null!;
Check((await service.SubmitAsync(bad)).HttpStatus == 201, "Null isteğe bağlı kaynak çöktü");
bad = Input("catalog", product.Id.ToString()); bad.Field = "status";
Check((await service.SubmitAsync(bad)).HttpStatus == 400, "İzin verilmeyen alan kabul edildi");
bad = Input("catalog", product.Id.ToString()); bad.SubmissionId = Guid.Empty;
Check((await service.SubmitAsync(bad)).HttpStatus == 400, "Boş gönderim kimliği kabul edildi");
await db.Entry(product).ReloadAsync(); await db.Entry(restaurant).ReloadAsync();
Check(product.Ingredients == "Eski içerik" && product.Assessment == "Eski değerlendirme" && restaurant.InformationSource == "Eski kaynak", "Katalog otomatik değiştirildi");
Console.WriteLine("PASS: üç hedef türü, eski veri korunması, bekleyen kayıt, tekrar gönderim/çakışma, geçersiz kayıt ve alan/URL sınırları.");
sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new(options);
    public Task<AppDbContext> CreateDbContextAsync(CancellationToken token = default) => Task.FromResult(new AppDbContext(options));
}
sealed class Clients : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(new Handler()) { BaseAddress = new Uri("https://world.openfoodfacts.org/") };
}
sealed class Handler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"product":{"code":"3017620422003","product_name":"Fixture product","lang":"tr","ingredients_text":"un"}}""") });
}
