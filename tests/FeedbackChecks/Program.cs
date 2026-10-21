using BlazorApp1.Data;
using BlazorApp1.Services;
using HelalRehber.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
await using var db = new AppDbContext(options); await db.Database.EnsureCreatedAsync();
db.Products.Add(new() { Name = "Korunacak ürün" }); await db.SaveChangesAsync();
await db.Database.ExecuteSqlRawAsync("DROP TABLE FeedbackSubmissions");
await FeedbackService.InitializeAsync(db); await FeedbackService.InitializeAsync(db);
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["Admin:PasswordHash"] = new PasswordHasher<string>().HashPassword("admin", "Test-only-password-123") }).Build();
var access = new AdminAccess(config); var service = new FeedbackService(new Factory(options), access);
void Check(bool value,string reason) { if(!value) throw new Exception(reason); }
FeedbackInput Input() => new() { SubmissionId = Guid.NewGuid(), Category = "bug", Message = "Arama sırasında hata oluştu.", Client = "android", Version = "0.1.0" };
var input = Input(); Check((await service.SubmitAsync(input)).HttpStatus == 201, "Kayıt");
Check((await service.SubmitAsync(input)).HttpStatus == 200, "Tekrar");
input.Message = "Başka bir hata açıklaması"; Check((await service.SubmitAsync(input)).HttpStatus == 409, "Çakışma");
var invalid = Input(); invalid.Message = " "; Check((await service.SubmitAsync(invalid)).HttpStatus == 400, "Boş metin");
invalid = Input(); invalid.Category = "unknown"; Check((await service.SubmitAsync(invalid)).HttpStatus == 400, "Kategori");
invalid = Input(); invalid.Message = new string('x',2001); Check((await service.SubmitAsync(invalid)).HttpStatus == 400, "Uzunluk");
try { await service.QueueAsync(new ClaimsPrincipal(),1); throw new Exception("Anonim erişim"); } catch(UnauthorizedAccessException) { }
for(var i=0;i<26;i++) Check((await service.SubmitAsync(Input())).HttpStatus == 201,"Liste kaydı");
var queue = await service.QueueAsync(access.Principal(),2); Check(queue.Total == 27 && queue.Items.Count == 2,"Sayfalama");
Check(await db.Products.CountAsync() == 1 && await db.CorrectionSubmissions.CountAsync() == 0,"Eski veri korunması");
Console.WriteLine("PASS: eski şema, veri korunması, doğrulama, tekrar/çakışma, anonim erişim engeli ve sayfalama.");
sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{ public AppDbContext CreateDbContext() => new(options); }
