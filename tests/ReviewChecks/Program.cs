using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
var password = Guid.NewGuid().ToString("N") + "!";
var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Admin:PasswordHash",new PasswordHasher<string>().HashPassword("admin",password)}}).Build();
var access = new AdminAccess(settings); var admin = access.Principal();
void Check(bool ok, string message){if(!ok)throw new Exception(message);}
Check(access.Verify("admin",password) && !access.Verify("admin","wrong-password-123"), "Parola doğrulama");
Check(!new AdminAccess(new ConfigurationBuilder().Build()).Verify("admin",password), "Yapılandırılmamış giriş açık");
using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
var factory = new Factory(options);
await using var db = new AppDbContext(options); await db.Database.EnsureCreatedAsync();
var product = new Product {Name="Fixture",Ingredients="Eski içerik"}; db.Products.Add(product);
var row = new CorrectionSubmission { Id=Guid.NewGuid(),Kind="catalog",TargetId="1",TargetName="Fixture",Field="ingredients",Suggestion="Fixture öneri",CreatedUtc=DateTime.UtcNow};
db.CorrectionSubmissions.Add(row); await db.SaveChangesAsync();
await db.Database.ExecuteSqlRawAsync("DROP TABLE CorrectionReviews");
await ReviewService.InitializeAsync(db); await ReviewService.InitializeAsync(db);
var service = new ReviewService(factory,access);
try{await service.QueueAsync(new ClaimsPrincipal(),null,null,1);throw new Exception("Anonim erişim açık");}catch(UnauthorizedAccessException){}
var member = new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,"member")}, "test"));
try{await service.QueueAsync(member,null,null,1);throw new Exception("Yönetici olmayan kullanıcı erişebildi");}catch(UnauthorizedAccessException){}
var first = new ReviewInput {ActionId=Guid.NewGuid(),ExpectedRevision=0,Status="needs_info",Note="Kaynak bilgisi tamamlanmalı."};
Check((await service.SaveAsync(admin,row.Id,first)).HttpStatus==200,"İnceleme kaydedilmedi");
Check((await service.SaveAsync(admin,row.Id,first)).HttpStatus==200,"Tekrar inceleme başarısız");
Check((await service.GetAsync(admin,row.Id))?.History.Count==1,"Geçmiş çoğaldı");
var stale = new ReviewInput {ActionId=Guid.NewGuid(),ExpectedRevision=0,Status="accepted",Note="Eski sekme kaydı."};
Check((await service.SaveAsync(admin,row.Id,stale)).HttpStatus==409,"Eski sürüm kabul edildi");
var second = new ReviewInput {ActionId=Guid.NewGuid(),ExpectedRevision=1,Status="accepted",Note="Öneri kaynakla karşılaştırıldı; katalog henüz değiştirilmedi."};
Check((await service.SaveAsync(admin,row.Id,second)).HttpStatus==200,"Uygun bulma başarısız");
var current = await service.GetAsync(admin,row.Id);
Check(current is { Revision:2, Submission.Status:"accepted", History.Count:2 },"Durum/geçmiş korunmadı");
Check(current!.History[0].PreviousStatus=="needs_info" && current.History[0].Reviewer=="admin","Denetim izi eksik");
await db.Entry(product).ReloadAsync(); Check(product.Ingredients=="Eski içerik","Katalog değişti");
Check((await service.QueueAsync(admin,"pending",null,1)).Items.Count==0 && (await service.QueueAsync(admin,"accepted","catalog",999)).Page==1,"Filtre/sayfa hatası");
second.ActionId=Guid.NewGuid(); second.ExpectedRevision=2; second.Status="approved-halal";
Check((await service.SaveAsync(admin,row.Id,second)).HttpStatus==400,"Geçersiz karar kabul edildi");
Check(await service.GetAsync(admin,Guid.NewGuid()) is null,"Eksik kayıt");
for(var i=0;i<30;i++) db.CorrectionSubmissions.Add(new CorrectionSubmission {Id=Guid.NewGuid(),Kind="catalog",TargetId="1",TargetName="Pagination fixture",Field="other",Suggestion="Fixture",CreatedUtc=DateTime.UtcNow});
await db.SaveChangesAsync();
var lastPage=await service.QueueAsync(admin,"pending","catalog",999);
Check(lastPage.Page==2 && lastPage.Items.Count==5 && lastPage.Total==30,"Sayfalama sınırları hatalı");
settings["Admin:PasswordHash"]=new PasswordHasher<string>().HashPassword("admin",Guid.NewGuid().ToString("N"));
try{await service.GetAsync(admin,row.Id);throw new Exception("Eski parola oturumu geçerli kaldı");}catch(UnauthorizedAccessException){}
Console.WriteLine("PASS: kapalı varsayılan giriş, yetki, parola, geçmiş, tekrar işlem, eski sürüm, filtreler ve katalog değişmezliği.");
sealed class Factory(DbContextOptions<AppDbContext> options): IDbContextFactory<AppDbContext>{public AppDbContext CreateDbContext()=>new(options);public Task<AppDbContext> CreateDbContextAsync(CancellationToken token=default)=>Task.FromResult(new AppDbContext(options));}
