namespace helalharam;

public static class MenuQuestions
{
    public static string Prepare(string text, string language)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Önce menü metnini gir.";
        if (text.Length > 12000) return "Menü en fazla 12.000 karakter olabilir.";
        var questions = new List<string>
        {
            "• Bu yemeklerin tam içerik listesini paylaşabilir misiniz?",
            "• Sos, aroma ve katkı maddelerinin içerikleri nedir?",
            "• Aynı ekipmanla başka ürünler hazırlanıyor mu?"
        };
        string[] meat = language switch
        {
            "tr" => ["et", "tavuk", "köfte", "kebap", "domuz"],
            "en" => ["meat", "chicken", "beef", "pork", "lamb"],
            "fr" => ["viande", "poulet", "boeuf", "bœuf", "porc", "agneau"],
            "de" => ["fleisch", "hähnchen", "huhn", "rind", "schwein", "lamm"],
            _ => []
        };
        if (Has(text, meat)) questions.Add("• Etin tedarikçisi ve varsa helal sertifikasının kapsamı nedir?");
        if (Has(text, ["çorba", "pilav", "sos", "soup", "broth", "sauce", "soupe", "bouillon", "suppe", "brühe"]))
            questions.Add("• Et suyu, bulyon veya hazır karışım kullanılıyor mu; kaynağı nedir?");
        if (Has(text, ["tatlı", "jelatin", "pasta", "dessert", "gelatin", "cake", "gâteau", "kuchen"]))
            questions.Add("• Jelatin veya benzeri bileşenler var mı; varsa kaynakları nedir?");
        if (Has(text, ["şarap", "alkol", "wine", "alcohol", "vin", "alcool", "wein", "alkohol"]))
            questions.Add("• Tarif veya soslarda alkol kullanılıyor mu; içeriği açık bir alternatif hazırlanabilir mi?");
        questions.Add("• İçeriği belirsiz bileşen çıkarılabilir mi veya içeriği açık başka bir yemek seçebilir miyim?");
        var coverage = language is "tr" or "en" or "fr" or "de"
            ? "Sorular sınırlı TR/EN/FR/DE anahtar kelimelerine göre hazırlanır; yemek adından tam içerik çıkarılmaz."
            : "Bu dil için özel yemek eşleştirmesi yok; genel sorular gösterilir.";
        return string.Join("\n\n", questions) + "\n\n" + coverage +
            "\nİşletmenin yanıtı olmadan uygunluk veya doğrulanmış alternatif sonucu verilmez.";
    }
    private static bool Has(string text, string[] words) => words.Any(word =>
        System.Text.RegularExpressions.Regex.IsMatch(text, @"(?<!\p{L})" +
            System.Text.RegularExpressions.Regex.Escape(word) + @"(?!\p{L})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant));
}
