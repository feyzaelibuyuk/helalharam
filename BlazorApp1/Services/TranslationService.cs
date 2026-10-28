using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using HelalRehber.Shared;
using Microsoft.Extensions.Caching.Memory;
namespace BlazorApp1.Services;
public sealed record TranslationResult(int HttpStatus, string Message, TranslationDocument? Document = null);
public sealed record TranslationDocument(string OriginalText, string TranslatedText, string SourceLanguage, string TargetLanguage,
    string Direction, string Provider, string Mode, List<string> Warnings, IngredientAssessment? OriginalAssessment,
    string PrecheckText, string PrecheckLanguage, IngredientAssessment? TranslationPrecheck);
public sealed class TranslationService(IHttpClientFactory clients, IConfiguration config, IMemoryCache cache)
{
    private readonly SemaphoreSlim languageGate = new(1,1);
    private readonly object quotaGate = new();
    private DateOnly budgetDay = DateOnly.FromDateTime(DateTime.UtcNow);
    private int consumed;
    public bool Configured => bool.TryParse(config["Translation:Enabled"],out var enabled) && enabled;
    private static bool RuleLanguage(string language) => language.Split('-')[0] is "tr" or "en" or "fr" or "de";
    public async Task<TranslationOptions> OptionsAsync(CancellationToken token = default)
    {
        if(cache.TryGetValue<List<TranslationLanguage>>("libre-languages-v1",out var known)) return new(Configured,"LibreTranslate (yerel)",known!,Configured ? "" : "Çeviri servisi henüz yapılandırılmadı. Yerel LibreTranslate servisini başlatın.");
        await languageGate.WaitAsync(token);
        try
        {
            if(!cache.TryGetValue<List<TranslationLanguage>>("libre-languages-v1",out known))
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,"languages");
                using var response = await clients.CreateClient("Translator").SendAsync(request,token); response.EnsureSuccessStatusCode();
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
                known = json.RootElement.EnumerateArray().Select(x => new TranslationLanguage(x.GetProperty("code").GetString()!, x.GetProperty("name").GetString()!, x.GetProperty("name").GetString()!, x.GetProperty("code").GetString() is "ar" or "he" or "fa" or "ur" ? "rtl" : "ltr")).OrderBy(x=>x.Name).ToList();
                var routes = json.RootElement.EnumerateArray().ToDictionary(x=>x.GetProperty("code").GetString()!, x=>x.GetProperty("targets").EnumerateArray().Select(t=>t.GetString()!).ToHashSet());
                cache.Set("libre-routes-v1", routes, TimeSpan.FromHours(24));
                if(known.Count == 0 || !known.Any(x=>x.Code=="en")) throw new JsonException();
                cache.Set("libre-languages-v1",known,TimeSpan.FromHours(24));
            }
            return new(Configured,"LibreTranslate (yerel)",known!,Configured ? "" : "Çeviri servisi henüz yapılandırılmadı. Yerel LibreTranslate servisini başlatın.");
        }
        catch(Exception ex) when(ex is HttpRequestException or JsonException or InvalidOperationException or KeyNotFoundException or TaskCanceledException)
        { if(token.IsCancellationRequested) throw; return new(Configured,"LibreTranslate (yerel)",[],"Dil listesi alınamadı. Bilgisayardaki LibreTranslate servisini başlatıp tekrar deneyin."); }
        finally { languageGate.Release(); }
    }
    public async Task<TranslationResult> TranslateAsync(TranslationInput input, CancellationToken token = default)
    {
        var errors=TranslationRules.Validate(input); if(errors.Count>0) return new(400,string.Join(" ",errors));
        if(!Configured) return new(503,"Çeviri servisi henüz yapılandırılmadı. Özgün metniniz korunuyor; çeviri yapılmadı.");
        var options=await OptionsAsync(token); if(options.Languages.Count==0) return new(502,options.Message);
        var source=input.SourceLanguage=="zh-Hans" ? "zh" : input.SourceLanguage;
        var target=options.Languages.FirstOrDefault(x=>x.Code.Equals(input.TargetLanguage,StringComparison.OrdinalIgnoreCase));
        var originalSource=source=="auto" ? null : options.Languages.FirstOrDefault(x=>x.Code.Equals(source,StringComparison.OrdinalIgnoreCase));
        if(target is null || (source!="auto" && originalSource is null)) return new(400,"Seçilen dil çeviri servisi tarafından desteklenmiyor. Güncel dil listesinden seçin.");
        source=originalSource?.Code ?? "auto";
        // Only label mode needs an additional transparent English precheck when target rules do not exist.
        var useEnglish=input.Mode=="label" && !RuleLanguage(target.Code);
        var multiplier=useEnglish ? 2 : 1;
        lock(quotaGate)
        {
            var today=DateOnly.FromDateTime(DateTime.UtcNow); if(today!=budgetDay){budgetDay=today;consumed=0;}
            var limit=Math.Clamp(int.TryParse(config["Translation:DailyCharacterLimit"],out var configuredLimit) ? configuredLimit : 50000,1,1000000);
            if(consumed+input.Text.Length*multiplier>limit) return new(429,"Uygulamanın günlük çeviri karakter sınırına ulaşıldı. Daha sonra tekrar deneyin.");
            consumed+=input.Text.Length*multiplier;
        }
        try
        {
            // Automatic detection is resolved first so unsupported model routes never become partial success.
            var actualSource=source;
            var detectedLow=false;
            if(source=="auto")
            {
                using var detection=await clients.CreateClient("Translator").PostAsJsonAsync("detect",new { q=input.Text },token);
                if(!detection.IsSuccessStatusCode) return new(502,"Kaynak dil algılanamadı. Dili elle seçin.");
                using var detected=JsonDocument.Parse(await detection.Content.ReadAsStringAsync(token));
                var best=detected.RootElement.EnumerateArray().OrderByDescending(x=>x.GetProperty("confidence").GetDouble()).FirstOrDefault();
                if(best.ValueKind!=JsonValueKind.Object) return new(502,"Kaynak dil algılanamadı. Dili elle seçin.");
                actualSource=best.GetProperty("language").GetString() ?? "unknown";
                detectedLow=best.GetProperty("confidence").GetDouble()<80;
                if(detectedLow) return new(400,"Otomatik dil algılama belirsiz. Kaynak dili elle seçip tekrar deneyin.");
            }
            if(!cache.TryGetValue<Dictionary<string,HashSet<string>>>("libre-routes-v1",out var routes) || routes is null || !routes.TryGetValue(actualSource,out var targets) ||
                (actualSource!=target.Code && !targets.Contains(target.Code)) || (useEnglish && actualSource!="en" && !targets.Contains("en")))
                return new(400,"Bu dil çifti için yerel çeviri modeli yüklü değil. Başka dil seçin veya gerekli modeli yükleyin.");
            async Task<string> TranslateTo(string language)
            {
                if(actualSource==language) return input.Text;
                using var response=await clients.CreateClient("Translator").PostAsJsonAsync("translate",new { q=input.Text, source=actualSource, target=language, format="text" },token);
                if(response.StatusCode==HttpStatusCode.TooManyRequests) throw new TranslationFailure(429);
                if(!response.IsSuccessStatusCode) throw new TranslationFailure(502);
                using var result=JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
                var value=result.RootElement.GetProperty("translatedText").GetString();
                if(string.IsNullOrWhiteSpace(value) || value.Length>120000) throw new TranslationFailure(502);
                return value;
            }
            var translated=await TranslateTo(target.Code);
            var checkText=useEnglish ? await TranslateTo("en") : translated;
            var checkLanguage=useEnglish ? "en" : target.Code;
            var warnings=new List<string>{"Makine çevirisi hatalı veya eksik olabilir. İçindekilerin tamamını, alerjen ve eser miktar uyarılarını özgün etiketle karşılaştırın. Bu çıktı helal onayı veya alerjen güvenliği garantisi değildir.","Yalnız girdiğiniz/okuttuğunuz metin çevrildi; fotoğrafta okunmayan ya da kadraj dışında kalan bölüm çevrilmiş sayılmaz."};

            // A deliberately limited signal, never a proof that all other words were translated.
            if(actualSource == "en" && target.Code == "tr")
            {
                string[] terms = ["milk", "wheat", "soy", "soya", "peanut", "peanuts", "egg", "eggs", "sesame", "fish", "shellfish", "mustard", "celery", "lupin", "sulphites", "sulfites", "gluten", "lactose", "whey", "casein", "sugar", "salt", "fat"];
                bool ContainsWord(string text, string word) => Regex.IsMatch(text,
                    @"(?<![\p{L}\p{N}])" + Regex.Escape(word) + @"(?![\p{L}\p{N}])",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
                var retained = terms.Where(word => ContainsWord(input.Text, word) && ContainsWord(translated, word)).ToArray();
                if(retained.Length > 0) warnings.Insert(0,
                    "Eksik çeviri olasılığı: " + string.Join(", ", retained) +
                    ". Bu sözcükler İngilizce kaynakta ve Türkçe çıktıda aynı biçimde bulundu. Ortak kullanılan terimler de eşleşebilir. Çıktı düzeltilmedi; özgün metni ayrıca kontrol edin. Bu sınırlı kontrol diğer çeviri hatalarını bulmaz.");
            }

            if(detectedLow) warnings.Add("Otomatik dil algılama belirsiz. Kaynak dili elle doğrulayın; özgün metin için değerlendirme yapılmadı.");
            var codePattern=@"(?<![A-Za-z0-9])E[ -]?\d{3,4}[a-z]?(?![A-Za-z0-9])";
            var codes=Regex.Matches(input.Text,codePattern,RegexOptions.IgnoreCase).Select(x=>Regex.Replace(x.Value,"[ -]","").ToUpperInvariant()).Distinct();
            var translatedCodes=Regex.Matches(translated,codePattern,RegexOptions.IgnoreCase).Select(x=>Regex.Replace(x.Value,"[ -]","").ToUpperInvariant()).ToHashSet();
            if(codes.Any(x=>!translatedCodes.Contains(x))) warnings.Add("Özgün metindeki bazı E kodları çeviride eşleşmedi. Kodları etiketten ayrıca doğrulayın.");
            var originalAssessment=input.Mode=="label" && !detectedLow ? IngredientEvaluator.Evaluate(input.Text,actualSource) : null;
            var precheck=input.Mode=="label" ? IngredientEvaluator.Evaluate(checkText,checkLanguage) : null;
            if(precheck is not null) warnings.Add("Çeviri üzerinde ön kontrol: bulgular makine çevirisine dayanır, gerçek bileşen/kaynak özgün etiketten doğrulanmalıdır. Bulgu olmaması uygunluk veya alerjen yokluğu değildir.");
            if(useEnglish) warnings.Add("Seçilen hedef dil için içerik kuralları olmadığından ek İngilizce çeviri ön kontrolde kullanıldı. Bu ikinci metin ayrıca gösterilir ve karakter kotasına dahildir.");
            return new(200,"Metin çevrildi; özgün metinle karşılaştırın.",new(input.Text,translated,actualSource,target.Code,target.Direction,"LibreTranslate (yerel)",input.Mode,warnings,originalAssessment,checkText ?? "",checkLanguage,precheck));
        }
        catch(TranslationFailure ex) { return new(ex.Status,"Yerel çeviri servisi isteği tamamlayamadı. Özgün metin korunuyor."); }
        catch(OperationCanceledException) when(!token.IsCancellationRequested) { return new(504,"Çeviri isteği zaman aşımına uğradı. Metniniz korunuyor; tekrar deneyin."); }
        catch(Exception ex) when(ex is HttpRequestException or JsonException or InvalidOperationException or FormatException or KeyNotFoundException)
        { return new(502,"Çeviri alınamadı. Özgün metin korunuyor; daha sonra tekrar deneyin."); }
    }
    private sealed class TranslationFailure(int status) : Exception { public int Status { get; } = status; }
}
