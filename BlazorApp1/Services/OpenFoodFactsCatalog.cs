using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorApp1.Services;

public sealed record OffProduct(string Barcode, string Name, string Brand, string Ingredients,
    string IngredientsLanguage, string[] Countries, string[] Categories, string[] Labels,
    string ImageUrl, string SourceUrl, DateTimeOffset FetchedAtUtc, DateTimeOffset? SourceUpdatedUtc,
    string Assessment, string AssessmentNote, IngredientAssessment IngredientAssessment, CertificateInfo Certificate);
public sealed record OffResult(string Status, OffProduct? Product, string Message, bool FromCache = false);

public sealed class OpenFoodFactsCatalog(IHttpClientFactory clients, IMemoryCache cache, CertificateRegistry? certificates = null)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Queue<DateTimeOffset> requests = new();
    public static bool ValidBarcode(string? code) => code is not null &&
        Regex.IsMatch(code, "\\A(?:[0-9]{8}|[0-9]{12}|[0-9]{13}|[0-9]{14})\\z");

    public async Task<OffResult> FindAsync(string? barcode, CancellationToken cancellationToken = default)
    {
        var code = barcode?.Trim() ?? "";
        if (!ValidBarcode(code)) return new("invalid_barcode", null, "8, 12, 13 veya 14 rakamlı barkod girin.");
        code = NormalizeBarcode(code);
        var key = "off-v3:" + IngredientEvaluator.Version + ":" + code;
        if (cache.TryGetValue<OffResult>(key, out var cached) && cached is not null) return WithCertificate(cached with { FromCache = true });
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue<OffResult>(key, out cached) && cached is not null) return WithCertificate(cached with { FromCache = true });
            var now = DateTimeOffset.UtcNow;
            while (requests.TryPeek(out var time) && time <= now.AddMinutes(-1)) requests.Dequeue();
            if (requests.Count >= 15) return new("rate_limited", null, "Sorgu sınırına ulaşıldı. Bir dakika sonra tekrar deneyin.");
            requests.Enqueue(now);
            var client = clients.CreateClient("OpenFoodFacts");
            const string fields = "code,product_name,brands,ingredients_text,lang,countries_tags,categories_tags,labels_tags,image_front_url,last_modified_t";
            using var response = await client.GetAsync($"api/v3.6/product/{code}.json?fields={fields}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                return new("rate_limited", null, "Open Food Facts sorgu sınırına ulaşıldı. Daha sonra tekrar deneyin.");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return CacheMissing(key);
            if (!response.IsSuccessStatusCode) return Unavailable();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var result = Parse(document.RootElement, code, DateTimeOffset.UtcNow);
            if (result.Status is "found" or "not_found")
                cache.Set(key, result, result.Status == "found" ? TimeSpan.FromHours(24) : TimeSpan.FromMinutes(10));
            return WithCertificate(result);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return Unavailable(); }
        catch (Exception ex) when (ex is HttpRequestException or JsonException) { return Unavailable(); }
        finally { gate.Release(); }
    }

    private OffResult WithCertificate(OffResult result) => result.Product is { } product && certificates is not null
        ? result with { Product = product with { Certificate = certificates.Evaluate(product) } } : result;

    private OffResult CacheMissing(string key)
    {
        var result = Missing(); cache.Set(key, result, TimeSpan.FromMinutes(10)); return result;
    }
    // OFF canonicalizes UPC/EAN aliases with different numbers of leading zeros.
    private static string NormalizeBarcode(string code)
    {
        var significant = code.TrimStart('0');
        return significant.Length <= 8 ? significant.PadLeft(8, '0') :
            significant.Length <= 12 ? significant.PadLeft(13, '0') : significant;
    }
    private static OffResult Missing() => new("not_found", null, "Bu barkod Open Food Facts'te bulunamadı. İçindekiler metnini elle girebilir veya mobil uygulamada etiket fotoğrafını okutabilirsiniz.");
    private static OffResult Unavailable() => new("unavailable", null, "Ürün servisine şu anda ulaşılamıyor. İnternet bağlantısını kontrol edip tekrar deneyin.");

    public static OffResult Parse(JsonElement root, string code, DateTimeOffset fetchedAt)
    {
        if (root.ValueKind != JsonValueKind.Object) return Unavailable();
        if (root.TryGetProperty("result", out var result) && Text(result, "id") == "product_not_found") return Missing();
        if (!root.TryGetProperty("product", out var product) || product.ValueKind != JsonValueKind.Object) return Unavailable();
        var returnedCode = Text(product, "code");
        if (!ValidBarcode(returnedCode) || NormalizeBarcode(returnedCode) != NormalizeBarcode(code)) return Unavailable();
        code = NormalizeBarcode(returnedCode);
        var ingredients = Text(product, "ingredients_text");
        var evaluation = IngredientEvaluator.Evaluate(ingredients, Text(product, "lang"));
        var image = Text(product, "image_front_url");
        if (!Uri.TryCreate(image, UriKind.Absolute, out var imageUri) || imageUri.Scheme != "https" || imageUri.Host != "images.openfoodfacts.org") image = "";
        DateTimeOffset? updated = null;
        if (product.TryGetProperty("last_modified_t", out var timestamp) && timestamp.ValueKind == JsonValueKind.Number && timestamp.TryGetInt64(out var unix) && unix is >= 0 and <= 253402300799)
            updated = DateTimeOffset.FromUnixTimeSeconds(unix);
        return new("found", new OffProduct(code, Text(product, "product_name") is { Length: > 0 } name ? name : "Ürün adı belirtilmemiş",
            Text(product, "brands"), ingredients, Text(product, "lang"), Tags(product, "countries_tags"),
            Tags(product, "categories_tags"), Tags(product, "labels_tags"), image,
            $"https://world.openfoodfacts.org/product/{code}", fetchedAt, updated, evaluation.Status, evaluation.Summary, evaluation, CertificateRegistry.Check(code, Text(product, "product_name"), Tags(product, "labels_tags"), [], DateOnly.FromDateTime(fetchedAt.Date))),
            "Gerçek ürün kaydı: Open Food Facts. Topluluk verileri eksik veya eski olabilir.");
    }
    private static string Text(JsonElement root, string property) => root.ValueKind == JsonValueKind.Object &&
        root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
    private static string[] Tags(JsonElement root, string property) => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
        ? value.EnumerateArray().Where(t => t.ValueKind == JsonValueKind.String).Select(t => t.GetString()!).ToArray() : [];
}
