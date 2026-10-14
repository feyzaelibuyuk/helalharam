using System.Text.Json;

namespace BlazorApp1.Services;

public sealed record CertificateInfo(string Status, string Message, bool HasHalalLabel,
    string Issuer = "", string Number = "", string Factory = "", string VerificationUrl = "",
    DateOnly? ValidFrom = null, DateOnly? ValidUntil = null, DateOnly? CheckedOn = null);
public sealed class CertificateRecord
{
    public string Barcode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Number { get; set; } = "";
    public string Factory { get; set; } = "";
    public string VerificationUrl { get; set; } = "";
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public DateOnly? CheckedOn { get; set; }
    public bool ScopeConfirmed { get; set; }
    public bool Revoked { get; set; }
}
public sealed class CertificateRegistry(IWebHostEnvironment environment)
{
    public CertificateInfo Evaluate(OffProduct product)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).DateTime);
        try
        {
            var path = Path.Combine(environment.ContentRootPath, "Data", "certificates.json");
            var records = File.Exists(path) ? JsonSerializer.Deserialize<List<CertificateRecord>>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [] : [];
            return Check(product.Barcode, product.Name, product.Labels, records, today);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        { return new("unavailable", "Sertifika kayıtlarına erişilemiyor. Doğrulanmış sertifika sonucu verilemedi.", HasLabel(product.Labels)); }
    }
    public static bool HasLabel(string[] labels) => labels.Any(tag =>
        tag.Split(':').Last().Equals("halal", StringComparison.OrdinalIgnoreCase) ||
        tag.Split(':').Last().Equals("halal-certified", StringComparison.OrdinalIgnoreCase));
    public static CertificateInfo Check(string barcode, string productName, string[] labels, IEnumerable<CertificateRecord> records, DateOnly today)
    {
        var hasLabel = HasLabel(labels);
        var candidates = records.Where(r => r is not null && r.Barcode == barcode && string.Equals(r.ProductName, productName, StringComparison.OrdinalIgnoreCase)).ToList();
        // Birden fazla kapsam kaydı otomatik olarak tek ürün/üretim yerine genellenmez.
        if (candidates.Count > 1) return new("scope_uncertain", "Birden fazla sertifika/üretim yeri kaydı var. Elinizdeki ambalajın kapsamı ayrıca eşleştirilmeli.", hasLabel);
        var record = candidates.SingleOrDefault();
        if (record is null) return new(hasLabel ? "label_only" : "not_found", hasLabel
            ? "Kaynakta helal etiketi var; sertifika numarası ve ürün kapsamı bağımsız doğrulanmadı."
            : "Sertifika bilgisi bulunamadı. Bu, ürünün haram olduğu anlamına gelmez.", hasLabel);
        var official = Uri.TryCreate(record.VerificationUrl, UriKind.Absolute, out var url) && url.Scheme == "https" &&
            ((record.Issuer?.Trim().ToUpperInvariant() == "DHCE" && url.Host == "halalcertification.ie") || (record.Issuer?.Trim().ToUpperInvariant() == "IFANCA" && (url.Host == "ifanca.org" || url.Host == "www.ifanca.org"))) &&
            url.AbsolutePath.Length > 1 && url.AbsolutePath.TrimEnd('/') != "/verify-halal-certificates";
        if (!record.ScopeConfirmed || !official || string.IsNullOrWhiteSpace(record.Factory) || string.IsNullOrWhiteSpace(record.Issuer) ||
            string.IsNullOrWhiteSpace(record.Number) || record.ValidFrom is null || record.ValidUntil is null ||
            record.CheckedOn is null || record.ValidFrom > record.ValidUntil || record.CheckedOn > today)
            return new("unverified", "Belge veya ürün/üretim yeri kapsamı eksik. Sertifika doğrulanmış olarak gösterilmez.", hasLabel);
        CertificateInfo Result(string status, string message) => new(status, message, hasLabel,
            record.Issuer, record.Number, record.Factory, record.VerificationUrl, record.ValidFrom, record.ValidUntil, record.CheckedOn);
        if (record.Revoked) return Result("revoked", "Kayıtlı sertifika iptal edilmiş. Geçerli sertifika olarak kullanılamaz.");
        if (record.ValidUntil < today) return Result("expired", "Kayıtlı belgenin süresi dolmuş. Güncel sertifika doğrulanmadı.");
        if (record.ValidFrom > today) return Result("not_yet_valid", "Belgenin geçerlilik dönemi henüz başlamamış.");
        if (record.CheckedOn < today.AddDays(-30)) return Result("recheck_required", "Resmî kayıt kontrolü 30 günden eski; güncel durumu tekrar doğrulanmalı.");
        return Result("verified", "Resmî kaydı kontrol edilmiş, kayıtlı ürün ve üretim yeri kapsamında geçerli sertifika. Elinizdeki ambalajın üretim yerini karşılaştırın.");
    }
}
