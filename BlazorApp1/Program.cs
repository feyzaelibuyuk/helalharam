using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databasePath = Path.Combine(builder.Environment.ContentRootPath, "helalharam.db");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { Name = "Örnek Sütlü Çikolata", Brand = "Örnek Marka", Barcode = "8690000000011", Ingredients = "Şeker, kakao yağı, süt tozu, kakao kitlesi, emülgatör (soya lesitini), aroma verici.", Assessment = "Ek bilgi gerekli", Note = "Aroma vericinin kaynağı ürün bilgisinde açıklanmıyor. Üreticiye danışın.", Source = "Demo kayıt; üretici bilgisiyle doğrulanmadı." },
            new Product { Name = "Örnek Çilekli Yoğurt", Brand = "Örnek Marka", Barcode = "8690000000028", Ingredients = "Yoğurt, çilek preparatı, şeker, kıvam artırıcı.", Assessment = "Bilgi bekleniyor", Note = "Bu demo kaydında katkı maddelerinin ayrıntısı doğrulanmadı.", Source = "Demo kayıt; üretici bilgisiyle doğrulanmadı." },
            new Product { Name = "Örnek Sebzeli Kraker", Brand = "Örnek Marka", Barcode = "8690000000035", Ingredients = "Buğday unu, bitkisel yağ, tuz, baharat.", Assessment = "Bilgi bekleniyor", Note = "Ambalaj ve üretici bilgisi kontrol edilmeli.", Source = "Demo kayıt; üretici bilgisiyle doğrulanmadı." });
        db.Restaurants.AddRange(
            new Restaurant { Name = "Örnek Esnaf Lokantası", Dish = "Mercimek çorbası", PossibleIngredients = "Mercimek, sebze, baharat; tarif ve yağ türü işletmeye göre değişebilir.", Questions = "Et suyu kullanılıyor mu? Hangi yağ ile hazırlanıyor?", InformationSource = "Örnek kayıt; işletme tarafından doğrulanmadı." },
            new Restaurant { Name = "Örnek Izgara Salonu", Dish = "Tavuk şiş", PossibleIngredients = "Tavuk, marinasyon ve baharat; hazırlama alanı ortak olabilir.", Questions = "Etin tedarik kaynağı nedir? Aynı ızgara veya ekipman kullanılıyor mu?", InformationSource = "Örnek kayıt; işletme tarafından doğrulanmadı." });
        await db.SaveChangesAsync();
    }
}

app.MapGet("/api/products", async (IDbContextFactory<AppDbContext> factory, string? q) =>
{
    await using var db = await factory.CreateDbContextAsync();
    var products = db.Products.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(q))
        products = products.Where(p => p.Name.Contains(q) || p.Brand.Contains(q) || p.Barcode.Contains(q) || p.Ingredients.Contains(q));
    return Results.Ok(await products.OrderBy(p => p.Name).ToListAsync());
});

app.MapGet("/api/restaurants", async (IDbContextFactory<AppDbContext> factory, string? q) =>
{
    await using var db = await factory.CreateDbContextAsync();
    var restaurants = db.Restaurants.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(q))
        restaurants = restaurants.Where(r => r.Name.Contains(q) || r.Dish.Contains(q));
    return Results.Ok(await restaurants.OrderBy(r => r.Name).ToListAsync());
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
