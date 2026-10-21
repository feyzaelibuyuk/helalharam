using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
foreach(var key in HelalRehber.Shared.UiCatalog.Keys)
foreach(var code in HelalRehber.Shared.UiCatalog.Languages)
    if(string.IsNullOrWhiteSpace(HelalRehber.Shared.UiCatalog.Text(key,code)))throw new Exception("Eksik UI çevirisi: "+key+"/"+code);
if(HelalRehber.Shared.UiCatalog.Text("search","unknown")!="Ara" || !HelalRehber.Shared.UiCatalog.IsRtl("ar") || HelalRehber.Shared.UiCatalog.IsRtl("en"))throw new Exception("Dil geri dönüş/yön kuralı");
var root = Path.GetFullPath(args[0]);
var fixture = Path.Combine(root, ".verification", "admin-http-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(fixture);
var password = Guid.NewGuid().ToString("N") + "!";
var start = new ProcessStartInfo("dotnet") {UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=root};
foreach(var value in new[]{Path.Combine(root,"BlazorApp1","bin","Debug","net10.0","BlazorApp1.dll"),"--urls","http://localhost:5091","--contentRoot",fixture})start.ArgumentList.Add(value);
start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";
start.Environment["Admin__PasswordHash"]=new PasswordHasher<string>().HashPassword("admin",password);
start.Environment["Logging__LogLevel__Default"]="Warning";
using var process=Process.Start(start)!;var logs=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();
void Check(bool ok,string why){if(!ok)throw new Exception(why);}
string Value(string html,string name){var match=Regex.Match(html,"name=\""+Regex.Escape(name)+"\" value=\"([^\"]*)\"");Check(match.Success,"Form alanı eksik: "+name);return WebUtility.HtmlDecode(match.Groups[1].Value);}
try
{
    using var handler=new HttpClientHandler{AllowAutoRedirect=false,CookieContainer=new CookieContainer()};
    using var client=new HttpClient(handler){BaseAddress=new Uri("http://localhost:5091")};
    var ready=false; for(var i=0;i<40;i++){try{using var response=await client.GetAsync("/admin/login");if(response.IsSuccessStatusCode){ready=true;break;}}catch(HttpRequestException){}await Task.Delay(250);}
    Check(ready,"Test sunucusu başlamadı");
    using(var response=await client.GetAsync("/api/admin/corrections"))Check(response.StatusCode==HttpStatusCode.Unauthorized,"Anonim kuyruk erişimi açık");
    using(var response=await client.GetAsync("/admin"))Check(response.StatusCode==HttpStatusCode.Redirect && response.Headers.Location!.ToString().Contains("/admin/login"),"Panel girişe yönlenmedi");
    var original=await client.GetStringAsync("/api/products/1");
    var submission=await client.PostAsJsonAsync("/api/corrections",new {SubmissionId=Guid.NewGuid(),Kind="catalog",TargetId="1",Field="ingredients",Suggestion="TEST <script>alert(1)</script> içerik kaynağı eksik.",SourceUrl="https://example.com/test"});
    Check(submission.StatusCode==HttpStatusCode.Created,"Fixture önerisi oluşturulamadı");
    var json=JsonDocument.Parse(await submission.Content.ReadAsStringAsync());var id=json.RootElement.GetProperty("receipt").GetProperty("id").GetString()!;
    using(var response=await client.GetAsync("/admin/corrections/"+id))Check(response.StatusCode==HttpStatusCode.Redirect,"Anonim detay açıldı");
    var login=await client.GetStringAsync("/admin/login");var token=Value(login,"__RequestVerificationToken");
    using(var response=await client.PostAsync("/admin/signin",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","admin"},{"password",password}})))Check(response.StatusCode==HttpStatusCode.BadRequest,"Giriş CSRF koruması yok");
    using(var response=await client.PostAsync("/admin/signin",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","admin"},{"password","wrong-test-password"},{"__RequestVerificationToken",token}})))Check(response.Headers.Location!.ToString().Contains("error=1"),"Yanlış parola kabul edildi");
    using(var response=await client.PostAsync("/admin/signin",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","admin"},{"password",password},{"__RequestVerificationToken",token}})))
    {Check(response.StatusCode==HttpStatusCode.Redirect && response.Headers.Location!.ToString()=="/admin","Yönetici girişi başarısız");Check(response.Headers.GetValues("Set-Cookie").Any(x=>x.Contains("httponly",StringComparison.OrdinalIgnoreCase)&&x.Contains("samesite=strict",StringComparison.OrdinalIgnoreCase)),"Oturum çerezi bayrakları eksik");}
    using(var response=await new HttpClient().GetAsync("http://localhost:5091/api/admin/feedback"))Check(response.StatusCode==HttpStatusCode.Unauthorized,"Anonim geri bildirim listesi açıldı");
    var feedbackInput=new HelalRehber.Shared.FeedbackInput { SubmissionId=Guid.NewGuid(), Category="bug", Message="TEST <script>alert(1)</script> arama sorunu", Client="android", Version="0.1.0" };
    using(var response=await client.PostAsJsonAsync("/api/feedback",feedbackInput))Check(response.StatusCode==HttpStatusCode.Created,"Geri bildirim API kayıt");
    using(var response=await client.PostAsJsonAsync("/api/feedback",feedbackInput))Check(response.StatusCode==HttpStatusCode.OK,"Geri bildirim tekrar");
    feedbackInput.Message="TEST başka geri bildirim mesajı";
    using(var response=await client.PostAsJsonAsync("/api/feedback",feedbackInput))Check(response.StatusCode==HttpStatusCode.Conflict,"Geri bildirim kimlik çakışması");
    var feedbackForm=await client.GetStringAsync("/feedback");
    var feedbackBody=new Dictionary<string,string>{{"_handler","feedback"},{"Input.SubmissionId",Value(feedbackForm,"Input.SubmissionId")},{"Input.Category","idea"},{"Input.Message","TEST kullanım önerisi açıklaması"},{"__RequestVerificationToken",Value(feedbackForm,"__RequestVerificationToken")}};
    var feedbackMissing=new Dictionary<string,string>(feedbackBody);feedbackMissing.Remove("__RequestVerificationToken");
    using(var response=await client.PostAsync("/feedback",new FormUrlEncodedContent(feedbackMissing)))Check(response.StatusCode==HttpStatusCode.BadRequest,"Geri bildirim CSRF");
    using(var response=await client.PostAsync("/feedback",new FormUrlEncodedContent(feedbackBody)))Check(WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("Geri bildiriminiz kaydedildi"),"Geri bildirim web formu");
    var feedbackList=await client.GetStringAsync("/admin/feedback");Check(feedbackList.Contains("arama sorunu")&&!feedbackList.Contains("<script>alert(1)</script>"),"Geri bildirim listesi/XSS");
    var feedbackQueue=await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/admin/feedback");Check(feedbackQueue.GetProperty("total").GetInt32()==2,"Geri bildirim kayıt sayısı");
    var dashboard=await client.GetStringAsync("/admin");Check(dashboard.Contains("İnceleme kuyruğu"),"Panel açılmadı");
    var detail=await client.GetStringAsync("/admin/corrections/"+id);Check(!detail.Contains("<script>alert(1)</script>"),"Öneri HTML olarak çalıştırılabilir");
    var body=new Dictionary<string,string>{{"_handler","review"},{"Input.ActionId",Value(detail,"Input.ActionId")},{"Input.ExpectedRevision",Value(detail,"Input.ExpectedRevision")},{"Input.Status","needs_info"},{"Input.Note","TEST kaynak açıklaması tamamlanmalı."},{"__RequestVerificationToken",Value(detail,"__RequestVerificationToken")}};
    var missing=new Dictionary<string,string>(body);missing.Remove("__RequestVerificationToken");
    using(var response=await client.PostAsync("/admin/corrections/"+id,new FormUrlEncodedContent(missing)))Check(response.StatusCode==HttpStatusCode.BadRequest,"İnceleme CSRF koruması yok");
    using(var response=await client.PostAsync("/admin/corrections/"+id,new FormUrlEncodedContent(body)))Check(WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("İnceleme kaydedildi"),"Web inceleme kaydedilmedi");
    using(var response=await client.PostAsync("/admin/corrections/"+id,new FormUrlEncodedContent(body)))Check(WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("daha önce kaydedildi"),"Web tekrar işlem koruması");
    body["Input.ActionId"]=Guid.NewGuid().ToString();
    using(var response=await client.PostAsync("/admin/corrections/"+id,new FormUrlEncodedContent(body)))Check(WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("başka bir sekmede"),"Eski form sürümü kabul edildi");
    body["Input.ExpectedRevision"]="1";body["Input.Status"]="accepted";body["Input.Note"]="TEST öneri kontrol edildi; katalog değiştirilmedi.";
    using(var response=await client.PostAsync("/admin/corrections/"+id,new FormUrlEncodedContent(body)))Check(WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("İnceleme kaydedildi"),"İkinci karar kaydedilmedi");
    var queue=JsonDocument.Parse(await client.GetStringAsync("/api/admin/corrections?status=accepted"));Check(queue.RootElement.GetProperty("accepted").GetInt32()==1,"Kuyruk sayacı güncellenmedi");
    using(var repeat = await client.PostAsJsonAsync("/api/corrections",new {SubmissionId=Guid.Parse(id),Kind="catalog",TargetId="1",Field="ingredients",Suggestion="TEST <script>alert(1)</script> içerik kaynağı eksik.",SourceUrl="https://example.com/test"}))
    {
        var repeated=JsonDocument.Parse(await repeat.Content.ReadAsStringAsync());
        Check(repeated.RootElement.GetProperty("receipt").GetProperty("status").GetString()=="accepted","Tekrar gönderim inceleme sonucunu kaybetti");
    }
    Check(await client.GetStringAsync("/api/products/1")==original,"Katalog otomatik değişti");
    dashboard=await client.GetStringAsync("/admin");
    using(var response=await client.PostAsync("/admin/signout",new FormUrlEncodedContent(new Dictionary<string,string>{{"__RequestVerificationToken",Value(dashboard,"__RequestVerificationToken")}})))Check(response.StatusCode==HttpStatusCode.Redirect,"Çıkış başarısız");
    using(var response=await client.GetAsync("/api/admin/corrections"))Check(response.StatusCode==HttpStatusCode.Unauthorized,"Çıkıştan sonra erişim açık");
    login=await client.GetStringAsync("/admin/login");token=Value(login,"__RequestVerificationToken");var limited=false;
    for(var i=0;i<6;i++){using var response=await client.PostAsync("/admin/signin",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","admin"},{"password","wrong-test-password"},{"__RequestVerificationToken",token}}));if(response.StatusCode==HttpStatusCode.TooManyRequests)limited=true;}
    Check(limited,"Giriş deneme sınırı yok");
    limited=false;
    for(var i=0;i<11;i++){using var response=await client.PostAsJsonAsync("/api/feedback",new HelalRehber.Shared.FeedbackInput());if(response.StatusCode==HttpStatusCode.TooManyRequests){limited=true;Check(response.Content.Headers.ContentType?.MediaType=="application/json","Geri bildirim 429 JSON değil");break;}}
    Check(limited,"Geri bildirim gönderim sınırı yok");
    using(var response=await client.PostAsync("/feedback",new FormUrlEncodedContent(feedbackBody)))Check(response.StatusCode==HttpStatusCode.TooManyRequests,"Web geri bildirim hız sınırı yok");


    var languageHtml=await client.GetStringAsync("/");
    var languageBody=new Dictionary<string,string>{{"language","en"},{"__RequestVerificationToken",Value(languageHtml,"__RequestVerificationToken")}};
    using(var response=await client.PostAsync("/language",new FormUrlEncodedContent(new Dictionary<string,string>{{"language","en"}})))Check(response.StatusCode==HttpStatusCode.BadRequest,"Dil CSRF");
    using(var response=await client.PostAsync("/language",new FormUrlEncodedContent(languageBody))){Check(response.StatusCode==HttpStatusCode.Redirect&&response.Headers.Location!.ToString()=="/","Dil kaydetme");Check(response.Headers.GetValues("Set-Cookie").Any(x=>x.Contains("helal-ui-language=en")&&x.Contains("httponly",StringComparison.OrdinalIgnoreCase)),"Dil çerezi");}
    languageHtml=await client.GetStringAsync("/");Check(languageHtml.Contains("lang=\"en\"")&&languageHtml.Contains("dir=\"ltr\"")&&languageHtml.Contains("Interface language")&&languageHtml.Contains("Packaged products"),"İngilizce ekran");
    Check(await client.GetStringAsync("/api/products/1")==original,"Dil değişimi kaynak ürünü değiştirdi");
    languageBody["language"]="ar";languageBody["__RequestVerificationToken"]=Value(languageHtml,"__RequestVerificationToken");
    using(var response=await client.PostAsync("/language",new FormUrlEncodedContent(languageBody)))Check(response.StatusCode==HttpStatusCode.Redirect,"Arapça dil kaydetme");
    languageHtml=await client.GetStringAsync("/");Check(languageHtml.Contains("lang=\"ar\"")&&languageHtml.Contains("dir=\"rtl\""),"Arapça RTL");
    languageBody["language"]="invalid";
    using(var response=await client.PostAsync("/language",new FormUrlEncodedContent(languageBody)))Check(response.StatusCode==HttpStatusCode.BadRequest,"Geçersiz dil kabul edildi");
    using(var fresh=new HttpClient())Check((await fresh.GetStringAsync("http://localhost:5091/")).Contains("lang=\"tr\""),"Dil başka oturuma sızdı");
    Console.WriteLine("PASS: anonim erişim engeli, giriş/çıkış, CSRF, çerez bayrakları, XSS kaçışlama, inceleme/tekrar/eski sürüm, kuyruk, katalog değişmezliği ve giriş sınırı; geri bildirim API/web/CSRF/XSS/özel liste; TR/EN/AR dil kalıcılığı, RTL, CSRF ve oturum ayrımı.");
}
finally{if(!process.HasExited){process.Kill(entireProcessTree:true);await process.WaitForExitAsync();}await logs;await errors;}
