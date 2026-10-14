using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorApp1.Services;

public sealed record IngredientFinding(string RuleId, string Ingredient, string MatchedText,
    string Severity, string Reason, string SourceTitle, string SourceUrl);
public sealed record IngredientAssessment(string Status, string Summary, string RuleVersion,
    string Profile, bool LanguageSupported, string Coverage, List<IngredientFinding> Findings);
public sealed record IngredientInput(string? Ingredients, string? Language);

public static class IngredientEvaluator
{
    public const string Version = "helal-source-check/1.0";
    public const string Profile = "Helal · DHCE/IFANCA kaynak kontrolü yaklaşımı";
    private const string Dhce = "https://halalcertification.ie/certification-requirements/";
    private const string Ifanca = "https://ifancahalal.pk/faqs/?include_category=general-halal";
    private const string Coverage = "Sınırlı içerik kuralları: TR/EN/FR/DE. Üretim, kesim ve sertifika doğrulaması yapılmaz. Bulgu olmaması helal onayı değildir.";
    private sealed record Rule(string Id, string Name, string Pattern, string Severity, string Reason, string SourceTitle, string SourceUrl);
    private static readonly Rule[] Rules =
    [
        new("pork", "Domuz türevi", "domuz(?: eti| yagi| jelatini)?|pork(?: fat| gelatin)?|porcine|porc|saindoux|schweinefleisch|schweinefett|schweinegelatine", "restricted",
            "Bu profilde domuz ve domuz türevleri kısıtlıdır. Eşleşme etiket metnine dayanır; gerçek kayıt ayrıca kontrol edilmelidir.", "DHCE — yasaklı hayvansal kaynaklar", Dhce),
        new("gelatin", "Jelatin", "jelatin(?:i)?|gelatin(?:e)?|gelatine|gelatines", "uncertain",
            "Jelatinin hayvansal kaynağı ve gerekli üretim/kesim bilgisi kayıttan doğrulanmadı. Jelatin tek başına kesin haram sınıflaması değildir.", "DHCE — belirsiz kaynaklı içerikler", Dhce),
        new("e471", "E471 / mono ve digliseritler", "e[ -]?471|mono[ -]*(?:(?:and|ve|et|und)[ -]*)?di[ -]*glycerides?|diglycerides?|monoglycerides?|mono(?:-| )?(?:ve )?digliseritler|digliseritler", "uncertain",
            "Mono/digliseritler bitkisel veya hayvansal kaynaklı olabilir. Kaynak belirtilmeden bu katkı kesin sınıflandırılmaz.", "IFANCA — mono/digliseritler", Ifanca),
        new("lecithin", "Lesitin", "lesitin(?:i)?|lecithin(?:s|e|es)?|lezithin|e[ -]?322", "uncertain",
            "Lesitinin kaynağı açıkça belirtilmemiş. Soya veya bitkisel kaynak beyanı, hayvansal kaynak belirsizliğini azaltır; ürün sertifikasını doğrulamaz.", "IFANCA — lesitin kaynakları", Ifanca),
        new("flavour", "Aroma", "aroma verici(?:ler)?|aromalar|aromes?|flavou?r(?:ing)?s?|aromastoffe", "uncertain",
            "Aromanın kaynak ve taşıyıcı bilgisi bu metinden doğrulanamıyor. Bu bir kaynak belirsizliği uyarısıdır.", "DHCE — aroma ve katkı kaynakları", Dhce),
        new("ethanol", "Etil alkol / etanol", "etil alkol|ethyl alcohol|ethanol|etanol|alcool ethylique", "uncertain",
            "Etanol ifadesi bulundu; miktar, kullanım ve işlem bilgisi bilinmiyor. Seçilen profil için ek kaynak kontrolü gerekir.", "DHCE — alkol ve üretim kaynakları", Dhce)
    ];

    public static IngredientAssessment Evaluate(string? ingredients, string? language)
    {
        var text = ingredients?.Trim() ?? "";
        var lang = (language ?? "").Trim().ToLowerInvariant().Split('-', '_')[0];
        var supported = lang is "tr" or "en" or "fr" or "de";
        var findings = new List<IngredientFinding>();
        IngredientAssessment Result(string status, string summary) => new(status, summary, Version, Profile, supported, Coverage, findings);
        if (text.Length == 0) return Result("Yetersiz bilgi", "İçindekiler metni yok; değerlendirme yapılamadı.");
        if (text.Length > 12000) return Result("Yetersiz bilgi", "İçerik metni 12.000 karakter sınırını aşıyor.");
        if (!supported) return Result("Yetersiz bilgi", "Bu içerik dili henüz desteklenmiyor veya belirtilmemiş. Özgün metin korunur; çeviri olmadan uygunluk çıkarımı yapılmaz.");
        foreach (var segment in Regex.Split(text, @"[,;\r\n]"))
        {
            var normalized = Normalize(segment);
            foreach (var rule in Rules)
            {
                foreach (Match match in Regex.Matches(normalized, $@"(?<![\p{{L}}\p{{N}}])(?:{rule.Pattern})(?![\p{{L}}\p{{N}}])", RegexOptions.CultureInvariant))
                {
                var severity = rule.Severity;
                var reason = rule.Reason;
                // Bir 'içermez' beyanı diğer malzeme veya bulguları geçersiz kılamaz.
                var before = normalized[..match.Index];
                var after = normalized[(match.Index + match.Length)..];
                var negated = Regex.IsMatch(before, @"(?:^|[\s:(])(?:no|without|sans|ohne|kein|keine)\s+(?:(?!\b(?:but|mais|aber|ancak|except|sauf)\b).)*$") ||
                    Regex.IsMatch(after, @"^\s*(?:-?free|icermez|yok|frei|enthalten nicht)(?!\p{L})");
                if (negated)
                { severity = "declaration"; reason = "İçermez beyanı bulundu. Bu beyan kaynakta yazılıdır; sertifika veya bağımsız doğrulama değildir."; }
                else if (Regex.IsMatch(before, @"(?:may contain|peut contenir|eser miktarda)\s*$"))
                { severity = "uncertain"; reason = "Olası içerik/iz beyanı bulundu; kesin mevcut bileşen gibi sınıflandırılmadı."; }
                else if (rule.Id is "e471" or "lecithin")
                {
                    const string plant = "(?:bitkisel|vegetable|vegetal(?:e|es)?|pflanzlich(?:e|en|er)?)";
                    const string soy = "(?:soya|soy|soja|sunflower|aycicegi|tournesol|sonnenblumen)";
                    var allowed = rule.Id == "lecithin" ? $"(?:{plant}|{soy})" : plant;
                    var qualified = Regex.IsMatch(before, $@"(?<!\p{{L}}){allowed}\s*(?:kaynakli\s*)?$") ||
                        Regex.IsMatch(after, $@"^\s*[\(\[]?\s*(?:(?:from|of|de|aus)\s+)?{allowed}(?!\p{{L}})");
                    if (qualified)
                    { severity = "declaration"; reason = "Bu bileşenin aynı metin parçasında bitkisel/soya kaynak beyanı var. Beyan kayıtlıdır; bağımsız ürün uygunluğu doğrulaması değildir."; }
                }
                if (findings.Any(f => f.RuleId == rule.Id && f.Severity == severity && f.MatchedText == segment.Trim())) continue;
                findings.Add(new(rule.Id, rule.Name, segment.Trim(), severity, reason, rule.SourceTitle, rule.SourceUrl));
                }
            }
        }
        if (findings.Any(f => f.Severity == "restricted")) return Result("Seçilen profile göre uygun değil", "Etiket metninde bu profilin kısıtladığı bir içerik eşleşti. Gerekçe ve kaynakları inceleyin.");
        if (findings.Any(f => f.Severity == "uncertain")) return Result("Şüpheli gıda", "Kaynağı veya kullanım bilgisi doğrulanmamış içerikler bulundu. Paketli ürün için belirsizlik ve gerekçe gösterilir.");
        return Result("İncelenen kurallarda risk bulunmadı", "Bu sınırlı kural listesinde kısıtlı veya belirsiz içerik eşleşmedi. Bu sonuç helal onayı veya sertifika değildir.");
    }
    private static string Normalize(string value)
    {
        var result = new StringBuilder();
        foreach (var c in value.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                result.Append(c == 'ı' ? 'i' : char.ToLowerInvariant(c));
        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
