using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
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
    Console.WriteLine("PASS: anonim erişim engeli, giriş/çıkış, CSRF, çerez bayrakları, XSS kaçışlama, inceleme/tekrar/eski sürüm, kuyruk, katalog değişmezliği ve giriş sınırı.");
}
finally{if(!process.HasExited){process.Kill(entireProcessTree:true);await process.WaitForExitAsync();}await logs;await errors;}
