using BlazorApp1.Services;

void Status(string text, string lang, string expected)
{
    var evaluation = IngredientEvaluator.Evaluate(text, lang);
    if (evaluation.Status != expected) throw new Exception($"{text}: {evaluation.Status}, beklenen {expected}");
    if (evaluation.Findings.Any(f => !Uri.IsWellFormedUriString(f.SourceUrl, UriKind.Absolute))) throw new Exception("Kaynak eksik");
}
Status("Şeker, domuz jelatini", "tr", "Seçilen profile göre uygun değil");
Status("Sugar, pork fat", "en", "Seçilen profile göre uygun değil");
Status("Sucre, gélatine", "fr", "Şüpheli gıda");
Status("Zucker, Gelatine", "de", "Şüpheli gıda");
Status("Şeker, E 471", "tr", "Şüpheli gıda");
Status("sugar, E4710", "en", "İncelenen kurallarda risk bulunmadı");
Status("bitkisel yağ, E471", "tr", "Şüpheli gıda");
Status("vegetable oil and E471", "en", "Şüpheli gıda");
Status("vegetable E471", "en", "İncelenen kurallarda risk bulunmadı");
Status("soya lesitini", "tr", "İncelenen kurallarda risk bulunmadı");
Status("lécithines [SOJA)", "fr", "İncelenen kurallarda risk bulunmadı");
Status("vegetable mono- and diglycerides", "en", "İncelenen kurallarda risk bulunmadı");
Status("mono- and diglycerides", "en", "Şüpheli gıda");
Status("lecithin", "en", "Şüpheli gıda");
Status("vegetable E471 and animal E471", "en", "Şüpheli gıda");
Status("soy lecithin and lecithin", "en", "Şüpheli gıda");
Status("natural flavours", "en", "Şüpheli gıda");
Status("sugar alcohol, chocolate liquor", "en", "İncelenen kurallarda risk bulunmadı");
Status("ethyl alcohol", "en", "Şüpheli gıda");
Status("pork-free", "en", "İncelenen kurallarda risk bulunmadı");
Status("sans gélatine", "fr", "İncelenen kurallarda risk bulunmadı");
Status("no pork and gelatin", "en", "İncelenen kurallarda risk bulunmadı");
Status("no pork but gelatin", "en", "Şüpheli gıda");
Status("pork-free; pork fat", "en", "Seçilen profile göre uygun değil");
Status("may contain pork", "en", "Şüpheli gıda");
Status("porcini mushrooms", "en", "İncelenen kurallarda risk bulunmadı");
Status("", "tr", "Yetersiz bilgi");
Status("砂糖, ゼラチン", "ja", "Yetersiz bilgi");
Status("牛奶, 糖", "zh", "Yetersiz bilgi");
Status("우유, 설탕", "ko", "Yetersiz bilgi");
Status("दूध, नमक", "hi", "Yetersiz bilgi");
Status("حليب, سكر", "ar", "Yetersiz bilgi");
Status("zucchero, gelatina", "it", "Yetersiz bilgi");
Status("sugar", "", "Yetersiz bilgi");
Status(new string('a',12001), "tr", "Yetersiz bilgi");
var benign = IngredientEvaluator.Evaluate("un, su, tuz", "tr");
if (benign.Status == "Uygun" || !benign.Summary.Contains("sertifika değildir")) throw new Exception("Olumsuz eşleşme yok diye sertifika üretildi");
Console.WriteLine("PASS: dil kapsamı, kaynaklı bulgular, belirsizlik, olumsuz beyan, yerel kaynak bağlamı, E-numarası sınırları ve yanlış helal onayının engellenmesi.");
