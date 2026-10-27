using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace HelalRehber.Shared;
public sealed class FoodPreferences
{
    public string[] Allergens {get;set;}=[];
    public string Diet {get;set;}="none";
    public bool AvoidGluten {get;set;}
    public bool AvoidLactose {get;set;}
    public string Custom {get;set;}="";
    public bool Active=>Allergens.Length>0||Diet!="none"||AvoidGluten||AvoidLactose||Custom.Length>0;
}
public sealed record FoodAllergen(string Id,string Name,string[] Words,string[] Tags);
public sealed record FoodFinding(string Item,string Context,string Match);
public sealed record FoodPreferenceResult(string Summary,string Coverage,List<FoodFinding> Findings);
public static class FoodPreferenceRules
{
    public const string Limits="Eşleşme bulunmaması alerjen yokluğu, çapraz temas yokluğu veya tüketim güvenliği onayı değildir. Tam etiket ve eser miktar uyarılarını kontrol edin. Menü gerçek tarif/ortak ekipmanı doğrulamaz. Laktozsuz beyanı süt alerjisi değerlendirmesinin yerine geçmez.";
    public const string Sources="https://www.gov.uk/understanding-food-labelling/allergen-labelling";
    public static readonly FoodAllergen[] All=[
        new("milk","Süt",["sut","sutu","sutlu","sutun","suttozu","peynir alti suyu","peyniralti suyu","kazein","kazeinat","kazeinati","lactoserum","lactalbumin","lactoglobulin","peynir","tereyagi","yogurt","kaymak","milk","whey","casein","caseinate","butter","cream","cheese","yogurt","lait","beurre","fromage","creme","milch","molke","kase","sahne"],["milk"]),
        new("eggs","Yumurta",["yumurta","yumurtasi","egg","eggs","albumen","albumin","oeuf","oeufs","ei","eier"],["eggs","egg"]),
        new("gluten","Gluten içeren tahıllar",["gluten","bugday","arpa","cavdar","yulaf","bulgur","irmik","semolina","wheat","barley","rye","oat","oats","ble","orge","seigle","avoine","weizen","gerste","roggen","hafer"],["gluten","wheat","barley","rye","oats"]),
        new("peanuts","Yer fıstığı",["yer fistigi","peanut","peanuts","arachide","arachides","erdnuss","erdnusse"],["peanuts"]),
        new("nuts","Sert kabuklu yemişler",["findik","badem","ceviz","kaju","antep fistigi","nuts","hazelnut","hazelnuts","almond","almonds","walnut","walnuts","cashew","cashews","pistachio","pistachios","pecan","macadamia","noisette","noisettes","amande","amandes","noix","haselnuss","haselnusse","mandel","mandeln","walnuss","walnusse"],["nuts","almonds","hazelnuts","walnuts","cashew-nuts","pecan-nuts","brazil-nuts","pistachio-nuts","macadamia-nuts"]),
        new("soybeans","Soya",["soya","soy","soybean","soybeans","soja"],["soybeans","soy","soya"]),
        new("sesame-seeds","Susam",["susam","tahin","sesame","tahini","sesam"],["sesame-seeds","sesame"]),
        new("fish","Balık",["balik","somon","ton baligi","hamsi","fish","salmon","tuna","anchovy","anchovies","poisson","saumon","thon","fisch","lachs"],["fish"]),
        new("crustaceans","Kabuklular (karides vb.)",["karides","yengec","istakoz","shrimp","prawn","prawns","crab","lobster","crevette","crevettes","crabe","garnelen","krabben"],["crustaceans"]),
        new("molluscs","Yumuşakçalar (midye vb.)",["midye","kalamar","istiridye","mussel","mussels","squid","oyster","oysters","mollusc","molluscs","moule","moules","calmar","muschel","muscheln"],["molluscs"]),
        new("celery","Kereviz",["kereviz","celery","celeri","sellerie"],["celery"]),
        new("mustard","Hardal",["hardal","mustard","moutarde","senf"],["mustard"]),
        new("lupin","Acı bakla (lupin)",["aci bakla","lupin","lupine","lupinen"],["lupin"]),
        new("sulphites","Sülfitler / kükürt dioksit",["sulfit","sulfitler","kukurt dioksit","sulphite","sulphites","sulfite","sulfites","sulphur dioxide","sulfur dioxide","sulfites","schwefeldioxid","e220","e221","e222","e223","e224","e225","e226","e227","e228"],["sulphur-dioxide-and-sulphites","sulphites"])
    ];
    private static readonly string[] Meat=["tavuk","domuz","sigir","kuzu","jelatin","meat","chicken","beef","pork","lamb","gelatin","gelatine","viande","poulet","boeuf","porc","fleisch","huhn","schwein","rennet","renin","hayvansal peynir mayasi"];
    public static List<string> Validate(FoodPreferences p)
    {
        List<string> errors=[];p.Allergens??=[];p.Custom=(p.Custom??"").Trim();
        if(p.Allergens.Length>All.Length||p.Allergens.Any(x=>!All.Any(a=>a.Id==x)))errors.Add("Geçersiz alerjen seçimi.");
        if(p.Diet is not ("none" or "vegetarian" or "vegan"))errors.Add("Diyet seçimi geçersiz.");
        var words=p.Custom.Split([',',';','\n'],StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
        if(p.Custom.Length>500||words.Length>8||words.Any(x=>x.Length is <2 or >60))errors.Add("Ek içerikleri virgülle ayırın: en fazla 8 içerik, her biri 2–60 karakter, toplam en fazla 500 karakter.");
        p.Allergens=p.Allergens.Distinct().ToArray();return errors;
    }
    public static FoodPreferences Read(string? json){try{var p=JsonSerializer.Deserialize<FoodPreferences>(json??"null")??new();return Validate(p).Count==0?p:new();}catch(JsonException){return new();}}
    public static string Normalize(string text)
    {
        var b=new StringBuilder();foreach(var c in text.ToLowerInvariant().Replace('ı','i').Replace("œ","oe").Replace("ß","ss").Normalize(NormalizationForm.FormD))if(CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)b.Append(c);return b.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
    private static IEnumerable<FoodFinding> TextFindings(string title,IEnumerable<string> words,string text)
    {
        foreach(var segment in Regex.Split(text,@"[,;\r\n.!]")){
            var normalized=Normalize(segment);
            foreach(var word in words){var pattern=@"(?<![\p{L}\p{N}])"+Regex.Escape(Normalize(word))+@"(?![\p{L}\p{N}])";
                foreach(Match m in Regex.Matches(normalized,pattern,RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(100))){
                    var before=normalized[..m.Index];var after=normalized[(m.Index+m.Length)..];
                    var negative=Regex.IsMatch(before,@"(?:^|\s)(?:no|without|sans|ohne)\s*$")||Regex.IsMatch(after,@"^\s*(?:-?free|icermez|yok|frei)(?!\p{L})");
                    var possible=Regex.IsMatch(before,@"(?:may contain(?: traces of)?|peut contenir|kann.*enthalten|eser miktarda|iz miktarda)\s*$")||Regex.IsMatch(after,@"^\s*(?:icerebilir)(?!\p{L})");
                    yield return new(title,negative?"İçermez beyanında eşleşme — doğrulanmadı":possible?"Olası içerik / eser miktar beyanında eşleşme":"Metinde eşleşme — içerik ve uyarı bağlamını kontrol edin",segment.Trim());
                }
            }
        }
    }
    public static FoodPreferenceResult Evaluate(FoodPreferences preferences,string? text,string? language,string[]? allergens=null,string[]? traces=null)
    {
        const string coverage="Başlangıç listesi 14 alerjen grubudur; tüm alerjileri kapsamaz. Metin sözlüğü sınırlı TR/EN/FR/DE; diğer dillerde OFF etiketleri ve ek içeriklerin birebir eşleşmesi kullanılabilir. Miktar/eşik veya çapraz temas ölçülmez. Her başlıkta en fazla 8 eşleşme örneği gösterilir.";
        if(Validate(preferences).Count>0)return new("Tercihler geçersiz; yeniden kaydedin.",coverage,[]);
        if(!preferences.Active)return new("Kişisel filtre seçilmedi.",coverage,[]);
        var original=text??"";if(original.Length>12000)return new("Metin 12.000 karakteri aşıyor; kişisel kontrol yapılmadı.",coverage,[]);
        var supported=(language??"").ToLowerInvariant().Split('-','_')[0] is "tr" or "en" or "fr" or "de";
        List<FoodFinding> findings=[];
        var selected=preferences.Allergens.ToHashSet();if(preferences.AvoidGluten)selected.Add("gluten");if(preferences.AvoidLactose)selected.Add("milk");
        if(preferences.Diet!="none"){selected.UnionWith(["fish","crustaceans","molluscs"]);if(preferences.Diet=="vegan")selected.UnionWith(["milk","eggs"]);}
        string Tag(string value)=>value.Split(':').Last().ToLowerInvariant();
        foreach(var a in All.Where(x=>selected.Contains(x.Id))){
            var title=preferences.AvoidLactose&&a.Id=="milk"&&!preferences.Allergens.Contains("milk")?"Laktozdan kaçınma: süt türevi / laktoz miktarı bilinmiyor":a.Name;
            if(!preferences.Allergens.Contains(a.Id)&&!(a.Id=="milk"&&preferences.AvoidLactose)&&!(a.Id=="gluten"&&preferences.AvoidGluten))title=(preferences.Diet=="vegan"?"Vegan tercihi · ":"Vejetaryen tercihi · ")+title;
            foreach(var t in (allergens??[]).Where(x=>a.Tags.Contains(Tag(x))))findings.Add(new(title,"Kaynak alerjen etiketi (OFF beyanı)",t));
            foreach(var t in (traces??[]).Where(x=>a.Tags.Contains(Tag(x))))findings.Add(new(title,"Kaynak eser miktar / çapraz temas etiketi (OFF beyanı)",t));
            if(supported)findings.AddRange(TextFindings(title,a.Words,original));
        }
        if(preferences.Diet!="none"&&supported){
            var avoid=Meat.Concat((language??"").StartsWith("tr",StringComparison.OrdinalIgnoreCase)?new[]{"et"}:Array.Empty<string>());
            if(preferences.Diet=="vegan")avoid=avoid.Concat(["bal","honey","miel","honig"]);
            findings.AddRange(TextFindings(preferences.Diet=="vegan"?"Vegan tercihi":"Vejetaryen tercihi",avoid,original));
        }
        if(preferences.AvoidLactose&&supported)findings.AddRange(TextFindings("Laktozdan kaçınma",["laktoz","lactose","laktose"],original));
        foreach(var custom in preferences.Custom.Split([',',';','\n'],StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries))findings.AddRange(TextFindings("Ek kaçınılan içerik: "+custom,[custom],original));
        var unique=findings.Distinct().GroupBy(x=>x.Item).SelectMany(g=>g.OrderBy(x=>x.Context.StartsWith("İçermez")?2:x.Context.StartsWith("Metinde")?1:0).Take(8)).ToList();
        var summary=unique.Count>0?"Seçtiğiniz tercihlerle ilgili eşleşmeler var; özgün etiketi kontrol edin.":"Seçili sınırlı kurallarda eşleşme bulunamadı; güvenli/alerjensiz/uygun sonucu verilmedi.";
        if(original.Length==0)summary+=" İçindekiler metni eksik.";if(!supported)summary+=" Metnin dili bu sözlükte desteklenmiyor; metin kontrolü kısmi.";
        if(preferences.AvoidLactose)summary+=" Süt türevi eşleşmesi laktoz miktarını belirlemez; laktozsuz ürün süt proteini içerebilir.";
        return new(summary,coverage,unique);
    }
}
