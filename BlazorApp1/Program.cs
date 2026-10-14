using HelalRehber.Shared;
using Microsoft.AspNetCore.Antiforgery;
using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databasePath = Path.Combine(builder.Environment.ContentRootPath, "helalharam.db");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ProductCatalog>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("OpenFoodFacts", client =>
{
    client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("HelalRehber/0.1 (+https://github.com/feyzaelibuyuk/helalharam)");
});
builder.Services.AddSingleton<CertificateRegistry>();
builder.Services.AddSingleton<OpenFoodFactsCatalog>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<BrowserFavorites>();
var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { Name = "Örnek Sütlü Çikolata", Brand = "Örnek Marka", Barcode = "8690000000011", Ingredients = "Şeker, kakao yağı, süt tozu, kakao kitlesi, emülgatör (soya lesitini), aroma verici.", Assessment = "Şüpheli gıda", Note = "Aroma vericinin kaynağı ürün bilgisinde açıklanmıyor. Bu nedenle ürünün uygunluğu doğrulanamıyor.", Source = "Demo kayıt; üretici bilgisiyle doğrulanmadı." },
            new Product { Name = "Örnek Çilekli Yoğurt", Brand = "Örnek Marka", Barcode = "8690000000028", Ingredients = "Yoğurt, çilek preparatı, şeker, kıvam artırıcı.", Assessment = "Bilgi bekleniyor", Note = "Bu demo kaydında katkı maddelerinin ayrıntısı doğrulanmadı.", Source = "Demo kayıt; üretici bilgisiyle doğrulanmadı." },
            new Product { Name = "Örnek Sebzeli Kraker", Brand = "Örnek Marka", Barcode = "8690000000035", Ingredients = "Buğday unu, bitkisel yağ, tuz, baharat.", Assessment = "Bilgi bekleniyor", Note = "Ambalaj ve üretici bilgisi kontrol edilmeli.", Source = "Demo kayıt; üretici bilgisiyle doğrulanmadı." });
        db.Restaurants.AddRange(
            new Restaurant { Name = "Örnek Esnaf Lokantası", Dish = "Mercimek çorbası", PossibleIngredients = "Mercimek, sebze, baharat; tarif ve yağ türü işletmeye göre değişebilir.", Questions = "Et suyu kullanılıyor mu? Hangi yağ ile hazırlanıyor?", InformationSource = "Örnek kayıt; işletme tarafından doğrulanmadı." },
            new Restaurant { Name = "Örnek Izgara Salonu", Dish = "Tavuk şiş", PossibleIngredients = "Tavuk, marinasyon ve baharat; hazırlama alanı ortak olabilir.", Questions = "Etin tedarik kaynağı nedir? Aynı ızgara veya ekipman kullanılıyor mu?", InformationSource = "Örnek kayıt; işletme tarafından doğrulanmadı." });
        await db.SaveChangesAsync();
    }
}

// Önceden oluşturulmuş demo kaydın eski yönlendirmesini güncelle.
await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    var demoProduct = await db.Products.FirstOrDefaultAsync(p =>
        p.Barcode == "8690000000011" &&
        p.Source == "Demo kayıt; üretici bilgisiyle doğrulanmadı." &&
        p.Note == "Aroma vericinin kaynağı ürün bilgisinde açıklanmıyor. Üreticiye danışın.");
    if (demoProduct is not null)
    {
        demoProduct.Assessment = "Şüpheli gıda";
        demoProduct.Note = "Aroma vericinin kaynağı ürün bilgisinde açıklanmıyor. Bu nedenle ürünün uygunluğu doğrulanamıyor.";
        await db.SaveChangesAsync();
    }
}
await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await CatalogSetup.InitializeAsync(db);
}

app.MapPost("/favorites/{action}", async (string action, HttpContext context, IAntiforgery antiforgery, BrowserFavorites favorites, ProductCatalog catalog, OpenFoodFactsCatalog off) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest("Form süresi dolmuş. Sayfayı yenileyin."); }
    if (action is not ("toggle" or "remove" or "clear")) return Results.NotFound();
    var form = await context.Request.ReadFormAsync();
    var kind = form["kind"].ToString(); var id = form["id"].ToString();
    var items = favorites.Read(context);
    if (action == "clear") { favorites.Write(context, []); return Results.Redirect("/favorites"); }
    if (!FavoriteList.Valid(kind, id)) return Results.BadRequest("Geçersiz ürün bağlantısı.");
    var removed = items.RemoveAll(x => x.Kind == kind && x.Id == id) > 0;
    if (action == "toggle" && !removed)
    {
        FavoriteItem? item;
        if (kind == "catalog")
        {
            var detail = await catalog.GetDetailAsync(int.Parse(id));
            item = detail is null ? null : new(kind, id, detail.Product.Name, detail.Product.Brand);
        }
        else
        {
            var result = await off.FindAsync(id, context.RequestAborted);
            item = result.Product is { } p ? new(kind, p.Barcode, p.Name, p.Brand) : null;
        }
        if (item is null) return Results.BadRequest("Ürün alınamadı. Ürün sayfasından tekrar deneyin.");
        if (!FavoriteList.Add(items, item)) return Results.BadRequest("Favori alanı doldu. Favoriler sayfasından bir ürünü çıkarın.");
    }
    favorites.Write(context, items);
    return Results.Redirect(action == "remove" ? "/favorites" : new FavoriteItem(kind, id, "", "").Url);
});

app.MapGet("/api/products", async (ProductCatalog catalog, string? q, string? country) =>
    !ProductCatalog.ValidCountry(country) ? Results.BadRequest("İki harfli ülke kodu kullanın (TR, DE gibi).") :
    Results.Ok(await catalog.SearchAsync(q, country)));

app.MapGet("/api/products/by-barcode/{barcode}", async (ProductCatalog catalog, string barcode, string? country) =>
    !ProductCatalog.ValidCountry(country) ? Results.BadRequest("İki harfli ülke kodu kullanın (TR, DE gibi).") :
    Results.Ok(await catalog.SearchAsync(barcode, country, exactBarcode: true)));

app.MapPost("/api/ingredients/assess", (IngredientInput input) =>
    input.Ingredients?.Length > 12000 ? Results.BadRequest("İçerik en fazla 12.000 karakter olabilir.") :
    Results.Ok(IngredientEvaluator.Evaluate(input.Ingredients, input.Language)));

app.MapGet("/api/off/products/{barcode}", async (OpenFoodFactsCatalog catalog, string barcode, CancellationToken cancellationToken) =>
{
    var result = await catalog.FindAsync(barcode, cancellationToken);
    return Results.Json(result, statusCode: result.Status switch
    {
        "found" => 200, "invalid_barcode" => 400, "not_found" => 404,
        "rate_limited" => 429, _ => 502
    });
});

app.MapGet("/api/products/{id:int}", async (ProductCatalog catalog, int id) =>
    await catalog.GetDetailAsync(id) is { } detail ? Results.Ok(detail) : Results.NotFound());
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
