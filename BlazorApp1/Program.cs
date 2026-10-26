using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using HelalRehber.Shared;
using Microsoft.AspNetCore.Antiforgery;
using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Keep mutable data outside the deployment directory in production.
var configuredDataPath = builder.Configuration["Storage:DataDirectory"];
if (!builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(configuredDataPath))
    throw new InvalidOperationException("Production için Storage:DataDirectory kalıcı klasörü ayarlanmalıdır.");
var dataDirectory = string.IsNullOrWhiteSpace(configuredDataPath)
    ? builder.Environment.ContentRootPath
    : Path.GetFullPath(configuredDataPath, builder.Environment.ContentRootPath);
Directory.CreateDirectory(dataDirectory);
var databasePath = Path.Combine(dataDirectory, "helalharam.db");
var keysDirectory = Path.Combine(dataDirectory, "keys");
Directory.CreateDirectory(keysDirectory);
builder.Services.AddDataProtection().SetApplicationName("HelalRehber.Web")
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString()));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ProductCatalog>();
builder.Services.AddScoped<RestaurantCatalog>();
builder.Services.AddScoped<CorrectionService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<WebUi>();
builder.Services.AddSingleton<TranslationService>();
builder.Services.AddSingleton<TravelCatalog>();
builder.Services.AddHttpClient("Overpass", client => { client.BaseAddress=new Uri(builder.Configuration["Maps:OverpassEndpoint"]??"https://overpass-api.de/api/interpreter");client.Timeout=TimeSpan.FromSeconds(30);client.DefaultRequestHeaders.UserAgent.ParseAdd("HelalRehber-StudentDemo/0.1"); }).ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler {AllowAutoRedirect=false});
builder.Services.AddHttpClient("Translator", client => { client.BaseAddress = new Uri("http://127.0.0.1:5000/"); client.Timeout = TimeSpan.FromSeconds(120); }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false });
builder.Services.AddSingleton<AdminAccess>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddAuthentication(AdminAccess.Scheme).AddCookie(AdminAccess.Scheme, options => {
    options.Cookie.Name = "helal-admin"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); options.SlidingExpiration = false;
    options.LoginPath = "/admin/login"; options.AccessDeniedPath = "/admin/login";
    options.Events.OnValidatePrincipal = async context => {
        if (!context.HttpContext.RequestServices.GetRequiredService<AdminAccess>().IsCurrent(context.Principal))
        { context.RejectPrincipal(); await context.HttpContext.SignOutAsync(AdminAccess.Scheme); }
    };
    options.Events.OnRedirectToLogin = async context => {
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        { context.Response.StatusCode = 401; await context.Response.WriteAsJsonAsync(new { Message = "Yönetici girişi gerekiyor." }); }
        else context.Response.Redirect(context.RedirectUri);
    };
    options.Events.OnRedirectToAccessDenied = async context => {
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        { context.Response.StatusCode = 403; await context.Response.WriteAsJsonAsync(new { Message = "Yetkiniz yok." }); }
        else context.Response.Redirect("/admin/login");
    };
});
builder.Services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser().RequireRole(AdminAccess.Role)));
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext,string>(context =>
        context.Request.Method=="POST" && (context.Request.Path.StartsWithSegments("/translate") || context.Request.Path.StartsWithSegments("/travel"))
        ? RateLimitPartition.GetFixedWindowLimiter("translate-travel-web:"+(context.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
            _=>new FixedWindowRateLimiterOptions { PermitLimit=5, Window=TimeSpan.FromMinutes(1), QueueLimit=0 })
        : RateLimitPartition.GetNoLimiter("other-global"));
    options.OnRejected = async (context, token) => {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        if ((context.HttpContext.Request.Path.StartsWithSegments("/translate") || context.HttpContext.Request.Path.StartsWithSegments("/feedback") || context.HttpContext.Request.Path.StartsWithSegments("/corrections") || context.HttpContext.Request.Path.StartsWithSegments("/admin")))
        {
            var retryPath = context.HttpContext.Request.Path.StartsWithSegments("/admin") ? "/admin/login" : context.HttpContext.Request.Path.ToString();
            var path = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(retryPath);
            context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
            await context.HttpContext.Response.WriteAsync("<!doctype html><html lang='tr'><meta charset='utf-8'><title>Gönderim sınırı</title><main style='max-width:600px;margin:60px auto;font-family:system-ui'><h1>Biraz bekleyelim</h1><p>Gönderim sınırına ulaşıldı. Bir dakika sonra tekrar deneyin. Önceki forma dönmek için tarayıcınızın geri düğmesini kullanabilirsiniz.</p><a href=\"" + path + "\">Yeni form aç</a></main></html>", token);
            return;
        }
        await context.HttpContext.Response.WriteAsJsonAsync(new { Message = "Gönderim sınırına ulaşıldı. Bir dakika sonra aynı gönderimle tekrar deneyin.", Receipt = (CorrectionReceipt?)null }, cancellationToken: token);
    };
    options.AddPolicy("translation", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("admin-login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("correction-posts", context =>
        context.Request.Method == "POST" && (context.Request.Path == "/feedback" || context.Request.Path == "/api/feedback" || context.Request.Path.StartsWithSegments("/corrections") || context.Request.Path == "/api/corrections")
        ? RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })
        : RateLimitPartition.GetNoLimiter("other-requests"));
});
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
    await RestaurantCatalog.InitializeAsync(db);
    await CorrectionService.InitializeAsync(db);
    await ReviewService.InitializeAsync(db);
    await FeedbackService.InitializeAsync(db);
}

app.MapPost("/language", async (HttpContext context, IAntiforgery antiforgery) => {
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest("Form süresi doldu. Sayfayı yenileyin."); }
    var form = await context.Request.ReadFormAsync(); var language = form["language"].ToString();
    if (!UiCatalog.Languages.Contains(language)) return Results.BadRequest("Geçersiz arayüz dili.");
    context.Response.Cookies.Append(WebUi.CookieName, language, new CookieOptions { HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Lax, IsEssential = true, Path = "/", MaxAge = TimeSpan.FromDays(365) });
    return Results.Redirect("/");
});

app.MapPost("/admin/signin", async (HttpContext context, IAntiforgery antiforgery, AdminAccess access) => {
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest("Form süresi doldu. Giriş sayfasını yenileyin."); }
    var form = await context.Request.ReadFormAsync();
    if (!access.Verify(form["username"].ToString(), form["password"].ToString())) return Results.Redirect("/admin/login?error=1");
    await context.SignInAsync(AdminAccess.Scheme, access.Principal(), new AuthenticationProperties { IsPersistent = false, AllowRefresh = false });
    return Results.Redirect("/admin");
}).RequireRateLimiting("admin-login");
app.MapPost("/admin/signout", async (HttpContext context, IAntiforgery antiforgery) => {
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest("Form süresi doldu. Sayfayı yenileyin."); }
    await context.SignOutAsync(AdminAccess.Scheme); return Results.Redirect("/admin/login");
}).RequireAuthorization("Admin");
app.MapGet("/api/admin/corrections", async (HttpContext context, ReviewService service, string? status, string? kind, int? page) =>
    Results.Ok(await service.QueueAsync(context.User, status, kind, page ?? 1))).RequireAuthorization("Admin");

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

app.MapGet("/api/travel/cities", (TravelCatalog catalog,string? q)=>Results.Ok(catalog.Cities(q)));
app.MapPost("/api/travel/search", async (TravelSearch input,TravelCatalog catalog,HttpContext context,CancellationToken token)=>{
    context.Response.Headers.CacheControl="no-store";var result=await catalog.SearchAsync(input,token);return Results.Json(result,statusCode:result.HttpStatus);
}).RequireRateLimiting("translation");
app.MapGet("/api/translation/options", async (TranslationService service, CancellationToken token) => Results.Ok(await service.OptionsAsync(token)));
app.MapPost("/api/translation", async (TranslationInput input, TranslationService service, HttpContext context, CancellationToken token) => {
    context.Response.Headers.CacheControl = "no-store";
    var result = await service.TranslateAsync(input, token); return Results.Json(result, statusCode: result.HttpStatus);
}).RequireRateLimiting("translation");

app.MapPost("/api/feedback", async (FeedbackInput input, FeedbackService service, CancellationToken token) => {
    var result = await service.SubmitAsync(input, token);
    return Results.Json(new { result.Message, result.Receipt }, statusCode: result.HttpStatus);
}).RequireRateLimiting("correction-posts");
app.MapGet("/api/admin/feedback", async (HttpContext context, FeedbackService service, int? page) =>
    Results.Ok(await service.QueueAsync(context.User, page ?? 1))).RequireAuthorization("Admin");

app.MapPost("/api/corrections", async (CorrectionInput input, CorrectionService service, CancellationToken token) => {
    var result = await service.SubmitAsync(input, token);
    return Results.Json(new { result.Message, result.Receipt }, statusCode: result.HttpStatus);
}).RequireRateLimiting("correction-posts");

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
app.MapGet("/api/restaurants", async (RestaurantCatalog catalog, string? q, string? city) =>
    Results.Ok(await catalog.SearchAsync(q, city)));
app.MapGet("/api/restaurants/{id:int}", async (RestaurantCatalog catalog, int id) =>
    await catalog.GetAsync(id) is { } detail ? Results.Ok(detail) : Results.NotFound());

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

app.Use(async (context, next) => {
    if(context.Request.Path.StartsWithSegments("/translate")) context.Response.Headers.CacheControl = "no-store";
    if (context.Request.Path.StartsWithSegments("/travel") || context.Request.Path.StartsWithSegments("/api/travel")) context.Response.Headers.CacheControl="no-store";
    if (context.Request.Method == "POST" && (context.Request.Path == "/travel" || context.Request.Path.StartsWithSegments("/api/travel") || context.Request.Path == "/translate" || context.Request.Path.StartsWithSegments("/api/translation") || context.Request.Path == "/language" || context.Request.Path == "/feedback" || context.Request.Path == "/api/feedback" || context.Request.Path.StartsWithSegments("/api/corrections") || context.Request.Path.StartsWithSegments("/corrections") || context.Request.Path.StartsWithSegments("/admin")))
    {
        var limit = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if (limit is { IsReadOnly: false }) limit.MaxRequestBodySize = (context.Request.Path.StartsWithSegments("/translate") || context.Request.Path.StartsWithSegments("/api/translation")) ? 256 * 1024 : 64 * 1024;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().RequireRateLimiting("correction-posts");

app.Run();
