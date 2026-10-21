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
    public bool Configured => bool.TryParse(config["Translation:Enabled"],out var enabled) && enabled && !string.IsNullOrWhiteSpace(config["Translation:Key"]);
    private static bool RuleLanguage(string language) => language.Split('-')[0] is "tr" or "en" or "fr" or "de";
    public async Task<TranslationOptions> OptionsAsync(CancellationToken token = default)
    {
        if(cache.TryGetValue<List<TranslationLanguage>>("translator-languages-v1",out var known)) return new(Configured,"Microsoft Translator",known!,Configured ? "" : "Çeviri servisi henüz yapılandırılmadı. Azure Translator anahtarı gerekiyor.");
        await languageGate.WaitAsync(token);
        try
        {
            if(!cache.TryGetValue<List<TranslationLanguage>>("translator-languages-v1",out known))
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,"languages?api-version=3.0&scope=translation");
                using var response = await clients.CreateClient("Translator").SendAsync(request,token); response.EnsureSuccessStatusCode();
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
                known = json.RootElement.GetProperty("translation").EnumerateObject().Select(x => new TranslationLanguage(x.Name,x.Value.GetProperty("name").GetString() ?? x.Name,x.Value.GetProperty("nativeName").GetString() ?? x.Name,x.Value.GetProperty("dir").GetString() == "rtl" ? "rtl" : "ltr")).OrderBy(x=>x.Name).ToList();
                if(known.Count == 0 || !known.Any(x=>x.Code=="en")) throw new JsonException();
                cache.Set("translator-languages-v1",known,TimeSpan.FromHours(24));
            }
            return new(Configured,"Microsoft Translator",known!,Configured ? "" : "Çeviri servisi henüz yapılandırılmadı. Azure Translator anahtarı gerekiyor.");
        }
        catch(Exception ex) when(ex is HttpRequestException or JsonException or InvalidOperationException or KeyNotFoundException or TaskCanceledException)
        { if(token.IsCancellationRequested) throw; return new(Configured,"Microsoft Translator",[],"Dil listesi alınamadı. Bağlantıyı kontrol edip tekrar deneyin."); }
        finally { languageGate.Release(); }
    }
    public async Task<TranslationResult> TranslateAsync(TranslationInput input, CancellationToken token = default)
    {
        var errors=TranslationRules.Validate(input); if(errors.Count>0) return new(400,string.Join(" ",errors));
        if(!Configured) return new(503,"Çeviri servisi henüz yapılandırılmadı. Özgün metniniz korunuyor; çeviri yapılmadı.");
        var options=await OptionsAsync(token); if(options.Languages.Count==0) return new(502,options.Message);
        var source=input.SourceLanguage=="zh" ? "zh-Hans" : input.SourceLanguage;
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
            var query="translate?api-version=3.0&textType=plain&profanityAction=NoAction&to="+Uri.EscapeDataString(target.Code);
            if(useEnglish) query+="&to=en";
            if(source!="auto") query+="&from="+Uri.EscapeDataString(source);
            using var request=new HttpRequestMessage(HttpMethod.Post,query);
            request.Headers.Add("Ocp-Apim-Subscription-Key",config["Translation:Key"]);
            var region=config["Translation:Region"];
            if(!string.IsNullOrWhiteSpace(region))
            {
                if(!Regex.IsMatch(region,"^[a-z0-9-]{1,40}$")) return new(503,"Çeviri bölge ayarı geçersiz. Yönetici yapılandırmayı kontrol etmeli.");
                request.Headers.Add("Ocp-Apim-Subscription-Region",region);
            }
            request.Content=JsonContent.Create(new[]{new { Text=input.Text }});
            using var response=await clients.CreateClient("Translator").SendAsync(request,token);
            if(response.StatusCode==HttpStatusCode.TooManyRequests) return new(429,"Çeviri servisinin kullanım sınırına ulaşıldı. Bir süre sonra tekrar deneyin.");
            if(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) return new(503,"Çeviri servisi anahtarı, bölgesi veya kotası doğrulanamadı. Yönetici ayarları kontrol etmeli.");
            if(!response.IsSuccessStatusCode) return new(502,"Çeviri servisi isteği tamamlayamadı. Özgün metin korunuyor.");
            var entries=await response.Content.ReadFromJsonAsync<AzureResult[]>(cancellationToken:token);
            if(entries is not { Length:1 } || entries[0] is null || entries[0].Translations is null) return new(502,"Çeviri servisi eksik yanıt verdi. Çeviri gösterilemiyor.");
            var result=entries[0];
            string? Find(string code)=>result.Translations!.FirstOrDefault(x=>x is not null && string.Equals(x.To,code,StringComparison.OrdinalIgnoreCase))?.Text;
            var translated=Find(target.Code); var checkText=useEnglish ? Find("en") : translated; var checkLanguage=useEnglish ? "en" : target.Code;
            if(string.IsNullOrWhiteSpace(translated) || translated.Length>120000 || (useEnglish && string.IsNullOrWhiteSpace(checkText))) return new(502,"Çeviri servisi eksik yanıt verdi. Özgün metni kullanın.");
            var actualSource=source=="auto" ? result.DetectedLanguage?.Language ?? "unknown" : source;
            var warnings=new List<string>{"Makine çevirisi hatalı veya eksik olabilir. İçindekilerin tamamını, alerjen ve eser miktar uyarılarını özgün etiketle karşılaştırın. Bu çıktı helal onayı veya alerjen güvenliği garantisi değildir.","Yalnız girdiğiniz/okuttuğunuz metin çevrildi; fotoğrafta okunmayan ya da kadraj dışında kalan bölüm çevrilmiş sayılmaz."};
            var detectedLow=source=="auto" && (result.DetectedLanguage is null || result.DetectedLanguage.Score<0.8);
            if(detectedLow) warnings.Add("Otomatik dil algılama belirsiz. Kaynak dili elle doğrulayın; özgün metin için değerlendirme yapılmadı.");
            var codePattern=@"(?<![A-Za-z0-9])E[ -]?\d{3,4}[a-z]?(?![A-Za-z0-9])";
            var codes=Regex.Matches(input.Text,codePattern,RegexOptions.IgnoreCase).Select(x=>Regex.Replace(x.Value,"[ -]","").ToUpperInvariant()).Distinct();
            var translatedCodes=Regex.Matches(translated,codePattern,RegexOptions.IgnoreCase).Select(x=>Regex.Replace(x.Value,"[ -]","").ToUpperInvariant()).ToHashSet();
            if(codes.Any(x=>!translatedCodes.Contains(x))) warnings.Add("Özgün metindeki bazı E kodları çeviride eşleşmedi. Kodları etiketten ayrıca doğrulayın.");
            var originalAssessment=input.Mode=="label" && !detectedLow ? IngredientEvaluator.Evaluate(input.Text,actualSource) : null;
            var precheck=input.Mode=="label" ? IngredientEvaluator.Evaluate(checkText,checkLanguage) : null;
            if(precheck is not null) warnings.Add("Çeviri üzerinde ön kontrol: bulgular makine çevirisine dayanır, gerçek bileşen/kaynak özgün etiketten doğrulanmalıdır. Bulgu olmaması uygunluk veya alerjen yokluğu değildir.");
            if(useEnglish) warnings.Add("Seçilen hedef dil için içerik kuralları olmadığından ek İngilizce çeviri ön kontrolde kullanıldı. Bu ikinci metin ayrıca gösterilir ve karakter kotasına dahildir.");
            return new(200,"Metin çevrildi; özgün metinle karşılaştırın.",new(input.Text,translated,actualSource,target.Code,target.Direction,"Microsoft Translator",input.Mode,warnings,originalAssessment,checkText ?? "",checkLanguage,precheck));
        }
        catch(OperationCanceledException) when(!token.IsCancellationRequested) { return new(504,"Çeviri isteği zaman aşımına uğradı. Metniniz korunuyor; tekrar deneyin."); }
        catch(Exception ex) when(ex is HttpRequestException or JsonException or InvalidOperationException or FormatException)
        { return new(502,"Çeviri alınamadı. Özgün metin korunuyor; daha sonra tekrar deneyin."); }
    }
    private sealed class AzureResult { public Detected? DetectedLanguage { get; set; } public List<AzureText>? Translations { get; set; } }
    private sealed class Detected { public string Language { get; set; }=""; public double Score { get; set; } }
    private sealed class AzureText { public string To { get; set; }=""; public string Text { get; set; }=""; }
}
