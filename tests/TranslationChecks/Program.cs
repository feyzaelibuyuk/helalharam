using BlazorApp1.Services;
using HelalRehber.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;
void Check(bool value,string reason){if(!value)throw new Exception(reason);}
TranslationInput Input(string target="tr")=>new(){Text="Ingredients: gelatin, E471.\nAllergens: MILK. May contain SOY.\nNutrition: 250 kcal. <script>example</script>",SourceLanguage="en",TargetLanguage=target,ReviewedSource=true,CloudConsent=true};
var handler=new Handler();var factory=new Clients(handler);
IConfiguration Config(bool enabled=true,int limit=50000)=>new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Translation:Enabled",enabled.ToString()},{"Translation:Key",enabled?"test-key-only":""},{"Translation:DailyCharacterLimit",limit.ToString()}}).Build();
TranslationService Service(bool enabled=true,int limit=50000)=>new(factory,Config(enabled,limit),new MemoryCache(new MemoryCacheOptions()));
var service=Service(false);Check((await service.TranslateAsync(Input())).HttpStatus==503&&handler.Posts==0,"Yapılandırılmamış servis dışarı gönderdi");
service=Service();var input=Input();input.CloudConsent=false;Check((await service.TranslateAsync(input)).HttpStatus==400&&handler.Posts==0,"Rıza");
input=Input();input.Text=new string('a',12001);Check((await service.TranslateAsync(input)).HttpStatus==400,"Metin kısaltıldı");
input=Input();input.TargetLanguage="bad&to=xx";Check((await service.TranslateAsync(input)).HttpStatus==400,"Geçersiz dil");
input=Input();var result=await service.TranslateAsync(input);Check(result.HttpStatus==200&&result.Document!.OriginalText==input.Text&&handler.LastText==input.Text,"Tam metin/özgün metin korunması");
Check(result.Document!.TranslatedText.Contains("SÜT")&&result.Document.TranslatedText.Contains("250"),"Alerjen/besin metni kayıp");
Check(result.Document.TranslationPrecheck!.Findings.Any(x=>x.RuleId=="gelatin"),"Çeviri ön kontrol");
input=Input("ja");result=await service.TranslateAsync(input);Check(result.HttpStatus==200&&result.Document!.PrecheckLanguage=="en"&&handler.LastUri!.Query.Contains("to=en"),"Hedef dil + açık İngilizce ön kontrol");
input=Input("ar");result=await service.TranslateAsync(input);Check(result.Document!.Direction=="rtl","RTL hedef");
handler.OmitEnglish=true;Check((await service.TranslateAsync(Input("ja"))).HttpStatus==502,"Eksik ön kontrol kabul edildi");handler.OmitEnglish=false;
handler.DropCode=true;Check((await service.TranslateAsync(Input())).Document!.Warnings.Any(x=>x.Contains("E kodları")),"E kodu kaybı uyarılmadı");handler.DropCode=false;
input=Input();input.Mode="menu";result=await service.TranslateAsync(input);Check(result.Document!.TranslationPrecheck is null&&result.Document.OriginalAssessment is null,"Menü ürün gibi sınıflandı");
input=Input();input.SourceLanguage="auto";handler.LowConfidence=true;result=await service.TranslateAsync(input);Check(result.Document!.OriginalAssessment is null&&result.Document.Warnings.Any(x=>x.Contains("algılama belirsiz")),"Düşük dil güveni");handler.LowConfidence=false;
foreach(var pair in new[]{(HttpStatusCode.Unauthorized,503),(HttpStatusCode.TooManyRequests,429),(HttpStatusCode.ServiceUnavailable,502)}){handler.Status=pair.Item1;Check((await service.TranslateAsync(Input())).HttpStatus==pair.Item2,"Servis hata eşleme");}handler.Status=HttpStatusCode.OK;
var before=handler.Posts;Check((await Service(limit:10).TranslateAsync(Input())).HttpStatus==429&&handler.Posts==before,"Karakter sınırı dış çağrıyı durdurmadı");
Console.WriteLine("PASS: tam metin/alerjen/besin korunması, rıza, yapılandırma, güncel dil doğrulama, ön kontrol ayrımı, RTL, eksik yanıt, E kodu, düşük güven, servis/kota hataları. Çeviriler kontrollü test yanıtlarıdır; gerçek Azure çağrısı değildir.");
sealed class Clients(Handler handler):IHttpClientFactory {public HttpClient CreateClient(string name)=>new(handler,false){BaseAddress=new Uri("https://api.cognitive.microsofttranslator.com/")};}
sealed class Handler:HttpMessageHandler
{
 public int Posts;public string? LastText;public Uri? LastUri;public bool OmitEnglish,DropCode,LowConfidence;public HttpStatusCode Status=HttpStatusCode.OK;
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
 {
  if(request.Method==HttpMethod.Get)return Json(new{translation=new Dictionary<string,object>{{"tr",new{name="Turkish",nativeName="Türkçe",dir="ltr"}},{"en",new{name="English",nativeName="English",dir="ltr"}},{"ja",new{name="Japanese",nativeName="日本語",dir="ltr"}},{"ar",new{name="Arabic",nativeName="العربية",dir="rtl"}}}});
  Posts++;LastUri=request.RequestUri;LastText=(await JsonDocument.ParseAsync(await request.Content!.ReadAsStreamAsync(token),cancellationToken:token)).RootElement[0].GetProperty("text").GetString();
  if(Status!=HttpStatusCode.OK)return new(Status);
  var targets=request.RequestUri!.Query.Split('&').Where(x=>x.StartsWith("to=")).Select(x=>Uri.UnescapeDataString(x[3..]));
  var translations=targets.Where(x=>!(OmitEnglish&&x=="en")).Select(x=>new{to=x,text=x=="en"?"Ingredients: gelatin, E471. Allergens: MILK. May contain SOY. Nutrition: 250 kcal.":(DropCode?"İçindekiler: jelatin. Alerjen: SÜT. SOYA içerebilir. Besin: 250 kcal.":"İçindekiler: jelatin, E471. Alerjen: SÜT. SOYA içerebilir. Besin: 250 kcal.")});
  return Json(new[]{new{detectedLanguage=new{language="en",score=LowConfidence?0.2:0.99},translations}});
 }
 private static HttpResponseMessage Json(object value)=>new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(value),System.Text.Encoding.UTF8,"application/json")};
}
