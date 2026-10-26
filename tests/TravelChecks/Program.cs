using System.Net;
using System.Text.Json;
using BlazorApp1.Services;
using HelalRehber.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
void Check(bool value,string why){if(!value)throw new Exception(why);}
if(args.Contains("--live"))
{
    var real=new TravelCatalog(new Env(),new LiveClients(args.FirstOrDefault(x=>x.StartsWith("--endpoint="))?[11..]??"https://overpass-api.de/api/interpreter"),new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Maps:Enabled","true"}}).Build(),new MemoryCache(new MemoryCacheOptions()));
    var live=await real.SearchAsync(new TravelSearch{UseCoordinates=true,Latitude=41.01,Longitude=28.97,Consent=true});
    Console.WriteLine($"LIVE: HTTP {live.HttpStatus}, {live.Places.Count} records. {live.Message}");return;
}
var handler=new Handler();
TravelCatalog Service(bool enabled=true)=>new(new Env(),new Clients(handler),new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Maps:Enabled",enabled.ToString()}}).Build(),new MemoryCache(new MemoryCacheOptions()));
var service=Service();
Check(service.Cities("istanbul").Any(x=>x.Country=="TR"),"Türkçe şehir/ASCII araması");
Check(service.Cities("Tokyo").Any(x=>x.Country=="JP"),"Japonya şehir araması");
Check(service.Cities("Roma").Any(x=>x.Country=="IT"),"İtalya şehir araması");
Check(service.Cities("東京").Any(x=>x.Country=="JP"),"Yerel alfabe şehir araması");
Check(service.Cities("a").Count==0&&service.Cities(new string('a',81)).Count==0,"Şehir girdi sınırı");
TravelSearch Input()=>new(){UseCoordinates=true,Latitude=41.01499,Longitude=28.97499,Consent=true};
var invalid=Input();invalid.Consent=false;Check((await service.SearchAsync(invalid)).HttpStatus==400&&handler.Posts==0,"Onaysız dış çağrı");
invalid=Input();invalid.Latitude=double.NaN;Check((await service.SearchAsync(invalid)).HttpStatus==400&&handler.Posts==0,"NaN koordinat");
Check((await Service(false).SearchAsync(Input())).HttpStatus==503,"Üretimde kapalı servis");
var result=await service.SearchAsync(Input());Check(result.HttpStatus==200&&result.Latitude==41.01&&result.Longitude==28.97,"Yaklaşık koordinat");
Check(handler.Body.Contains("around:3000,41.01,28.97")&&!handler.Body.Contains("41.01499"),"Konum hassasiyeti/sorgu biçimi");
Check(result.Places.Count==2&&result.Places[0].OsmId==1,"Filtre, tekrar kayıt ve sıralama");
Check(result.Places.All(x=>x.HalalTag is "yes" or "only")&&result.Places.All(x=>x.SourceUrl.StartsWith("https://www.openstreetmap.org/")),"Kaynak doğrulama");
Check(result.Places.Any(x=>x.Name=="<script>alert(1)</script>"),"Kaynak metni bozuldu (UI metin olarak kaçışlamalı)");
var before=handler.Posts;Check((await service.SearchAsync(Input())).Places.Count==2&&handler.Posts==before,"Önbellek");
invalid=Input();invalid.Latitude=42;Check((await service.SearchAsync(invalid)).HttpStatus==429&&handler.Posts==before,"Global demo hız sınırı");
handler.Status=HttpStatusCode.TooManyRequests;var unavailable=await Service().SearchAsync(Input());Check(unavailable.HttpStatus==429&&unavailable.HasCenter&&unavailable.Places.Count==0,"Kaynak hatasında harita bölgesi korunmalı, sonuç uydurulmamalı");
handler.Status=HttpStatusCode.OK;handler.Partial=true;Check((await Service().SearchAsync(Input())).HttpStatus==502,"Eksik kaynak başarılı sayıldı");
Check(TravelRules.Distance(0,0,0,0)==0&&TravelRules.Distance(0,0,0,1)>111&&TravelRules.Distance(0,0,0,1)<112,"Kuş uçuşu mesafe");
Check(!TravelRules.Valid(86,0)&&!TravelRules.Valid(0,181),"Harita koordinat sınırları");
Console.WriteLine("PASS: dünya şehirleri/alfabeler, onay, koordinat hassasiyeti, filtre/mesafe/sıralama/tekrar, kaynak bağlantısı, önbellek, kota ve eksik yanıt. Mock yanıt gerçek sertifika doğrulaması değildir.");
sealed class Env:IWebHostEnvironment
{
 public string WebRootPath{get;set;}="";public IFileProvider WebRootFileProvider{get;set;}=new NullFileProvider();public string ApplicationName{get;set;}="TravelChecks";
 public string EnvironmentName{get;set;}="Development";public string ContentRootPath{get;set;}=Path.GetFullPath("BlazorApp1");public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();
}
sealed class Clients(Handler handler):IHttpClientFactory{public HttpClient CreateClient(string name)=>new(handler,false){BaseAddress=new Uri("https://overpass-api.de/api/interpreter")};}
sealed class Handler:HttpMessageHandler
{
 public int Posts;public string Body="";public HttpStatusCode Status=HttpStatusCode.OK;public bool Partial;
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
 {
  Posts++;Body=Uri.UnescapeDataString(await request.Content!.ReadAsStringAsync(token));
  object Row(long id,double lat,string halal="yes",string type="node")=>new{type,id,lat,lon=28.97,tags=new Dictionary<string,string>{{"name",id==2?"<script>alert(1)</script>":"Test"},{"amenity","restaurant"},{"diet:halal",halal}}};
  object data=Partial?new{remark="timeout",elements=new object[0]}:new{elements=new[]{Row(2,41.02),Row(1,41.01),Row(1,41.01),Row(3,41.01,"no"),Row(4,50),Row(5,41.01,"yes","unknown")}};
  return new(Status){Content=new StringContent(JsonSerializer.Serialize(data),System.Text.Encoding.UTF8,"application/json")};
 }
}

sealed class LiveClients(string endpoint):IHttpClientFactory
{
 public HttpClient CreateClient(string name){var client=new HttpClient(new ProbeHandler()){BaseAddress=new Uri(endpoint),Timeout=TimeSpan.FromSeconds(30)};client.DefaultRequestHeaders.UserAgent.ParseAdd("HelalRehber-StudentDemo/0.1");return client;}
}
sealed class ProbeHandler:DelegatingHandler
{
 public ProbeHandler():base(new HttpClientHandler{AllowAutoRedirect=false}){}
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
 {
  var response=await base.SendAsync(request,token);
  if(!response.IsSuccessStatusCode){var body=await response.Content.ReadAsStringAsync(token);Console.WriteLine($"Upstream {(int)response.StatusCode}: {body[..Math.Min(body.Length,200)]}");}
  return response;
 }
}
