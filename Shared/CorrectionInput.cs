namespace HelalRehber.Shared;

public sealed class CorrectionInput
{
    public Guid SubmissionId { get; set; }
    public string Kind { get; set; } = "";
    public string TargetId { get; set; } = "";
    public string Field { get; set; } = "";
    public string Suggestion { get; set; } = "";
    public string SourceUrl { get; set; } = "";
}
public sealed record CorrectionField(string Id, string Label);
public sealed record CorrectionReceipt(Guid Id, string Status, string Message);
public static class CorrectionRules
{
    public static string StatusLabel(string status) => status switch {
        "pending" => "İnceleme bekliyor", "needs_info" => "Ek bilgi gerekiyor",
        "accepted" => "Öneri uygun bulundu — kataloğa uygulanmadı", "rejected" => "Öneri reddedildi", _ => "Durum bilinmiyor"
    };
    public static bool ValidTarget(string kind, string id) => kind == "restaurant"
        ? FavoriteList.Valid("catalog", id) : FavoriteList.Valid(kind, id);
    public static CorrectionField[] Fields(string kind) => kind == "restaurant"
        ? [new("name", "Restoran / yemek adı"), new("ingredients", "İçerik bilgisi"), new("location", "Şehir / şube"), new("source", "Bilgi kaynağı"), new("other", "Diğer")]
        : [new("name", "Ürün adı / marka"), new("ingredients", "İçindekiler"), new("assessment", "Değerlendirme / sertifika bilgisi"), new("source", "Bilgi kaynağı"), new("other", "Diğer")];
    public static List<string> Validate(CorrectionInput input)
    {
        var errors = new List<string>();
        if (input.SubmissionId == Guid.Empty) errors.Add("Gönderim kimliği eksik. Formu tekrar açın.");
        if (!ValidTarget(input.Kind ?? "", input.TargetId ?? "")) errors.Add("Geçersiz ürün veya yemek bağlantısı.");
        if (!Fields(input.Kind ?? "").Any(x => x.Id == input.Field)) errors.Add("Düzeltilecek alanı seçin.");
        var text = input.Suggestion?.Trim() ?? "";
        if (text.Length is < 10 or > 2000) errors.Add("Önerinizi 10–2.000 karakterle açıklayın.");
        var source = input.SourceUrl?.Trim() ?? "";
        if (source.Length > 1000 || (source.Length > 0 && (!Uri.TryCreate(source, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.UserInfo.Length > 0)))
            errors.Add("Kaynak için kullanıcı adı/şifre içermeyen, en fazla 1.000 karakterlik https bağlantısı girin.");
        return errors;
    }
}
