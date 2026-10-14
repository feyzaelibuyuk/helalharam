using System.Net;
using System.Text.Json;
using BlazorApp1.Services;
using Microsoft.Extensions.Caching.Memory;

const string barcode = "3017620422003";
const string fixture = """
{"status":"success","product":{"code":"3017620422003","product_name":"Test ürünü","brands":"Test markası","ingredients_text":"砂糖, lait, cacao","lang":"ja","countries_tags":["en:japan","en:france"],"labels_tags":["en:halal"],"image_front_url":"https://images.openfoodfacts.org/test.jpg","last_modified_t":1700000000}}
""";
using var parsed = JsonDocument.Parse(fixture);
var mapped = OpenFoodFactsCatalog.Parse(parsed.RootElement, barcode, DateTimeOffset.UtcNow);
Check(mapped.Product is { Name: "Test ürünü", Ingredients: "砂糖, lait, cacao", IngredientsLanguage: "ja" }, "Özgün dil/metin korunmalı");
var mappedProduct = mapped.Product ?? throw new Exception("Ürün eşlenmedi");
Check(mappedProduct.Countries.Length == 2, "Birden çok satış ülkesi korunmalı");
Check(mappedProduct.Assessment == "Yetersiz bilgi" && !mappedProduct.IngredientAssessment.LanguageSupported, "Helal etiketi kesin uygunluğa dönüşmemeli");
Check(mappedProduct.SourceUpdatedUtc != mappedProduct.FetchedAtUtc, "Veri alınma tarihi kaynak doğrulama tarihi değildir");
using var empty = JsonDocument.Parse("""{"product":{"code":"3017620422003","product_name":"İçeriksiz","last_modified_t":"bozuk"}}""");
Check(OpenFoodFactsCatalog.Parse(empty.RootElement, barcode, DateTimeOffset.UtcNow).Product?.Assessment == "Yetersiz bilgi", "Eksik içerik ve bozuk tarih güvenle ele alınmalı");
using var mismatch = JsonDocument.Parse("""{"product":{"code":"9999999999999","ingredients_text":"test"}}""");
Check(OpenFoodFactsCatalog.Parse(mismatch.RootElement, barcode, DateTimeOffset.UtcNow).Status == "unavailable", "Yanlış barkodlu yanıt kullanılmamalı");
using var malicious = JsonDocument.Parse("""{"product":{"code":"3017620422003","image_front_url":"https://tracking.example/test.jpg"}}""");
Check(OpenFoodFactsCatalog.Parse(malicious.RootElement, barcode, DateTimeOffset.UtcNow).Product?.ImageUrl == "", "Resimler yalnız OFF kaynağından gelmeli");
using var upc = JsonDocument.Parse("""{"product":{"code":"0034000470693","product_name":"UPC fixture"}}""");
Check(OpenFoodFactsCatalog.Parse(upc.RootElement, "034000470693", DateTimeOffset.UtcNow).Product?.Barcode == "0034000470693", "UPC/EAN baştaki sıfır varyantı aynı ürünü bulmalı");
using var absentCode = JsonDocument.Parse("""{"product":{"product_name":"Eksik barkod"}}""");
Check(OpenFoodFactsCatalog.Parse(absentCode.RootElement, barcode, DateTimeOffset.UtcNow).Status == "unavailable", "Kimliği olmayan ürün yanıtı kullanılmamalı");
var handler = new Handler(_ => new(HttpStatusCode.OK) { Content = new StringContent(fixture) });
using var cache = new MemoryCache(new MemoryCacheOptions());
var catalog = new OpenFoodFactsCatalog(new Factory(handler), cache);
Check((await catalog.FindAsync("bad/code")).Status == "invalid_barcode" && handler.Count == 0, "Geçersiz barkod dış isteğe dönüşmemeli");
var results = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => catalog.FindAsync(barcode)));
Check(handler.Count == 1 && results.Count(r => r.FromCache) == 4, "Aynı barkod için tek istek ve önbellek");
var missingHandler = new Handler(_ => new(HttpStatusCode.NotFound));
using var missingCache = new MemoryCache(new MemoryCacheOptions());
var missingCatalog = new OpenFoodFactsCatalog(new Factory(missingHandler), missingCache);
Check((await missingCatalog.FindAsync(barcode)).Status == "not_found" && (await missingCatalog.FindAsync(barcode)).FromCache && missingHandler.Count == 1, "Bulunamayan ürün kısa önbelleğe alınmalı");
using var rateCache = new MemoryCache(new MemoryCacheOptions());
var rateHandler = new Handler(_ => new(HttpStatusCode.ServiceUnavailable));
var rateCatalog = new OpenFoodFactsCatalog(new Factory(rateHandler), rateCache);
for (var i = 0; i < 15; i++) Check((await rateCatalog.FindAsync((3000000000000L + i).ToString())).Status == "unavailable", "Servis hatası ayrılmalı");
Check((await rateCatalog.FindAsync("3000000000015")).Status == "rate_limited" && rateHandler.Count == 15, "Dakikada en fazla 15 dış ürün isteği");
var upstreamLimit = new Handler(_ => new(HttpStatusCode.TooManyRequests));
using var limitCache = new MemoryCache(new MemoryCacheOptions());
Check((await new OpenFoodFactsCatalog(new Factory(upstreamLimit), limitCache).FindAsync(barcode)).Status == "rate_limited", "Dış servis 429'u doğru aktarılmalı");
Console.WriteLine("PASS: OFF eşleme, özgün dil, eksik içerik, etiket/sertifika ayrımı, yanlış barkod, önbellek, eşzamanlılık, 404/429/503 ve istek sınırı.");
static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public int Count { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    { Count++; return Task.FromResult(respond(request)); }
}
sealed class Factory(Handler handler) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(handler, disposeHandler: false) { BaseAddress = new("https://world.openfoodfacts.org/") };
}
