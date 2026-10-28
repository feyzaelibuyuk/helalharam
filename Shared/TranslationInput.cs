namespace HelalRehber.Shared;
public sealed class TranslationInput
{
    public string Text { get; set; } = "";
    public string SourceLanguage { get; set; } = "auto";
    public string TargetLanguage { get; set; } = "tr";
    public string Mode { get; set; } = "label";
    public bool ReviewedSource { get; set; }
    public bool CloudConsent { get; set; }
}
public sealed record TranslationLanguage(string Code, string Name, string NativeName, string Direction);
public sealed record TranslationOptions(bool Configured, string Provider, List<TranslationLanguage> Languages, string Message);
public static class TranslationRules
{
    public const int MaxLength = 12000;
    public static List<string> Validate(TranslationInput input)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(input.Text) || input.Text.Length > MaxLength) errors.Add("1–12.000 karakter metin girin. Metin otomatik kısaltılmaz.");
        if (!input.ReviewedSource) errors.Add("OCR metnini fotoğrafla karşılaştırıp kontrol edin.");
        if (!input.CloudConsent) errors.Add("Metnin sunucunun bilgisayarındaki yerel çeviri servisine gönderilmesini onaylayın.");
        if (input.Mode is not ("label" or "menu")) errors.Add("Etiket veya menü türünü seçin.");
        if (string.IsNullOrWhiteSpace(input.SourceLanguage) || input.SourceLanguage.Length > 20 || string.IsNullOrWhiteSpace(input.TargetLanguage) || input.TargetLanguage.Length > 20) errors.Add("Kaynak ve hedef dili seçin.");
        return errors;
    }
}
