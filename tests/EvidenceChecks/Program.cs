using BlazorApp1.Data;
using BlazorApp1.Services;
using HelalRehber.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
var root=Path.GetFullPath(args[0]);
var fixture=Path.Combine(root,".verification","evidence-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(fixture);
void Check(bool ok,string why){if(!ok)throw new Exception(why);}
var password=Guid.NewGuid().ToString("N")+"!";
var hash=new PasswordHasher<string>().HashPassword("admin",password);
var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Admin:PasswordHash",hash}}).Build();
var access=new AdminAccess(config);var admin=access.Principal();
using var connection=new SqliteConnection("Data Source=:memory:");await connection.OpenAsync();
var options=new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;var factory=new Factory(options);
await using var db=new AppDbContext(options);await db.Database.EnsureCreatedAsync();
await CertificateEvidenceService.InitializeAsync(db);await CertificateEvidenceService.InitializeAsync(db);
var env=new Host {ContentRootPath=fixture,WebRootPath=Path.Combine(fixture,"wwwroot")};
var service=new CertificateEvidenceService(factory,access,env,config);
var png=Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jD1sAAAAASUVORK5CYII=");
IFormFile FileFixture(string name="test-only.png",byte[]? bytes=null){var data=bytes??png;return new FormFile(new MemoryStream(data),0,data.Length,"File",name);}
CertificateEvidenceInput Input()=>new(){SubmissionId=Guid.NewGuid(),Name="SYNTHETIC TEST <script>alert(1)</script>",Issuer="DHCE",Number="TEST-ONLY",Scope="Synthetic test plant; not a real certificate",Consent=true};
Check(!CertificateAuthorities.Matches("HAK","https://www.hak.gov.tr/record/test"),"HAK is not a product certificate issuer");
foreach(var bad in new[]{"https://halalcertification.ie.evil.example/record","https://user@halalcertification.ie/record","http://halalcertification.ie/record","https://halalcertification.ie:8443/record","https://halalcertification.ie/verify-halal-certificates","https://halalcertification.ie/verify-halal-certificates/","https://halalcertification.ie/record#fragment"})Check(!CertificateAuthorities.IsRecordUrl("DHCE",bad),"Unsafe/generic URL accepted: "+bad);
var input=Input();input.Consent=false;Check((await service.SubmitAsync(input,FileFixture())).HttpStatus==400,"Consent required");input.Consent=true;
Check((await service.SubmitAsync(input,null)).HttpStatus==400,"Evidence required");
Check((await service.SubmitAsync(input,FileFixture("test.pdf"))).HttpStatus==400,"Extension mismatch");
Check((await service.SubmitAsync(input,FileFixture("test.png","<html>bad</html>"u8.ToArray()))).HttpStatus==400,"HTML upload accepted");
Check((await service.SubmitAsync(input,new FormFile(Stream.Null,0,CertificateEvidenceRules.MaxFileBytes+1,"File","big.pdf"))).HttpStatus==400,"Size bound");
Check((await service.SubmitAsync(input,FileFixture("../../test-only.png"))).HttpStatus==201,"Valid upload");
Check((await service.SubmitAsync(input,FileFixture("../../test-only.png"))).HttpStatus==200,"Idempotent submission");
input.Notes="Changed";Check((await service.SubmitAsync(input,FileFixture())).HttpStatus==409,"Conflicting payload");input.Notes="";
Check(Directory.GetFiles(Path.Combine(fixture,"certificate-evidence")).Length==1,"Duplicate/orphan file");
try{await service.QueueAsync(new ClaimsPrincipal());throw new Exception("Anonymous queue access");}catch(UnauthorizedAccessException){}
try{await service.FileAsync(new ClaimsPrincipal(),input.SubmissionId,default);throw new Exception("Anonymous file access");}catch(UnauthorizedAccessException){}
var file=await service.FileAsync(admin,input.SubmissionId,default);Check(file?.Name=="test-only.png"&&file.Value.Bytes.SequenceEqual(png),"Private file contents/name");
var decision=new EvidenceDecision {ActionId=Guid.NewGuid(),ExpectedRevision=0,Status="reviewed",Note="Synthetic test only",CheckedUrl="https://halalcertification.ie/register/test-only"};
Check((await service.ReviewAsync(admin,input.SubmissionId,decision)).HttpStatus==400,"Checklist mandatory");
decision.SourceChecked=decision.NumberMatched=decision.ScopeMatched=decision.DatesChecked=decision.RevocationChecked=true;
decision.CheckedUrl="https://halalcertification.ie/verify-halal-certificates/";Check((await service.ReviewAsync(admin,input.SubmissionId,decision)).HttpStatus==400,"Directory is not evidence");
decision.CheckedUrl="https://halalcertification.ie/register/test-only";Check((await service.ReviewAsync(admin,input.SubmissionId,decision)).HttpStatus==200,"Review save");
Check((await service.ReviewAsync(admin,input.SubmissionId,decision)).HttpStatus==200,"Idempotent review");
decision.ActionId=Guid.NewGuid();Check((await service.ReviewAsync(admin,input.SubmissionId,decision)).HttpStatus==409,"Stale review");
var detail=await service.GetAsync(admin,input.SubmissionId);Check(detail is {Row.Revision:1,History.Count:1},"Audit history");Check(await db.Products.CountAsync()==0,"Catalog changed");
var stored=Directory.GetFiles(Path.Combine(fixture,"certificate-evidence")).Single();await System.IO.File.WriteAllBytesAsync(stored,"tampered"u8.ToArray());
try{await service.FileAsync(admin,input.SubmissionId,default);throw new Exception("Tampering undetected");}catch(InvalidDataException){}
config["Storage:DataDirectory"]=env.WebRootPath;var unsafeInput=Input();Check((await service.SubmitAsync(unsafeInput,FileFixture())).HttpStatus==503,"Public upload storage accepted");config["Storage:DataDirectory"]=null;
Console.WriteLine("PASS service: URL allowlist, consent, size/type, private storage, checksum, authorization, idempotency, checklist, revisions, catalog separation.");
// Real HTTP server: isolated database, generated test credentials, no real User Secrets read.
var start=new ProcessStartInfo("dotnet"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=root};
foreach(var arg in new[]{Path.Combine(root,"BlazorApp1/bin/Debug/net10.0/BlazorApp1.dll"),"--urls","http://127.0.0.1:5096","--contentRoot",Path.Combine(root,"BlazorApp1")})start.ArgumentList.Add(arg);
start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";start.Environment["Admin__PasswordHash"]=hash;start.Environment["Storage__DataDirectory"]=Path.Combine(fixture,"http-data");start.Environment["Logging__LogLevel__Default"]="Warning";
using var process=Process.Start(start)!;var logs=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();
string Value(string html,string name){var match=Regex.Match(html,"name=\""+Regex.Escape(name)+"\" value=\"([^\"]*)\"");Check(match.Success,"Missing form field: "+name);return WebUtility.HtmlDecode(match.Groups[1].Value);}
try {
using var handler=new HttpClientHandler {AllowAutoRedirect=false,CookieContainer=new CookieContainer()};using var client=new HttpClient(handler){BaseAddress=new Uri("http://127.0.0.1:5096")};
var ready=false;for(var i=0;i<50;i++){try{using var r=await client.GetAsync("/certificates");if(r.IsSuccessStatusCode){ready=true;break;}}catch(HttpRequestException){}await Task.Delay(200);}Check(ready,"Server failed to start");
var formHtml=await client.GetStringAsync("/certificates");Check(CertificateAuthorities.All.All(a=>formHtml.Contains(a.Url)),"Official directory missing");
var af=await client.GetFromJsonAsync<JsonElement>("/api/certificates/form-token");var csrf=af.GetProperty("token").GetString()!;var header=af.GetProperty("header").GetString()!;
var httpId=Guid.NewGuid();
MultipartFormDataContent Form(){var f=new MultipartFormDataContent();foreach(var pair in new Dictionary<string,string>{{"SubmissionId",httpId.ToString()},{"Kind","product"},{"Name","SYNTHETIC <script>alert(1)</script>"},{"Issuer","DHCE"},{"Number","TEST-ONLY"},{"Scope","Synthetic test only"},{"Consent","true"}})f.Add(new StringContent(pair.Value),pair.Key);f.Add(new ByteArrayContent(png),"File","test-only.png");return f;}
using(var r=await client.PostAsync("/api/certificates/submissions",Form()))Check(r.StatusCode==HttpStatusCode.BadRequest,"Submission CSRF");
foreach(var expected in new[]{HttpStatusCode.Created,HttpStatusCode.OK}){using var req=new HttpRequestMessage(HttpMethod.Post,"/api/certificates/submissions"){Content=Form()};req.Headers.Add(header,csrf);using var r=await client.SendAsync(req);Check(r.StatusCode==expected,"HTTP upload/idempotency: "+await r.Content.ReadAsStringAsync());}
using(var r=await client.GetAsync($"/api/admin/certificates/{httpId}/file"))Check(r.StatusCode==HttpStatusCode.Unauthorized,"Anonymous download");
using(var r=await client.GetAsync($"/admin/certificates/{httpId}"))Check(r.StatusCode==HttpStatusCode.Redirect,"Anonymous private metadata");
var login=await client.GetStringAsync("/admin/login");using(var r=await client.PostAsync("/admin/signin",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","admin"},{"password",password},{"__RequestVerificationToken",Value(login,"__RequestVerificationToken")}})))Check(r.StatusCode==HttpStatusCode.Redirect,"Test login");
var review=await client.GetStringAsync($"/admin/certificates/{httpId}");Check(!review.Contains("<script>alert(1)</script>")&&review.Contains("test-only.png"),"Private detail/XSS");
using(var r=await client.GetAsync($"/api/admin/certificates/{httpId}/file")){Check(r.IsSuccessStatusCode&&r.Content.Headers.ContentDisposition?.DispositionType=="attachment"&&r.Headers.GetValues("X-Content-Type-Options").Contains("nosniff")&&r.Headers.CacheControl?.NoStore==true,"Download headers");Check((await r.Content.ReadAsByteArrayAsync()).SequenceEqual(png),"Download changed");}
var body=new Dictionary<string,string>{{"_handler","certificate-review"},{"Input.ActionId",Value(review,"Input.ActionId")},{"Input.ExpectedRevision",Value(review,"Input.ExpectedRevision")},{"Input.Status","needs_info"},{"Input.Note","Synthetic test only; more information needed"}};
using(var r=await client.PostAsync($"/admin/certificates/{httpId}",new FormUrlEncodedContent(body)))Check(r.StatusCode==HttpStatusCode.BadRequest,"Review CSRF");
body["__RequestVerificationToken"]=Value(review,"__RequestVerificationToken");
using(var r=await client.PostAsync($"/admin/certificates/{httpId}",new FormUrlEncodedContent(body)))Check(WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync()).Contains("İnceleme kaydedildi"),"SSR review save");
using(var r=await client.PostAsync($"/admin/certificates/{httpId}",new FormUrlEncodedContent(body)))Check(WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync()).Contains("daha önce kaydedildi"),"SSR duplicate review");
body["Input.ActionId"]=Guid.NewGuid().ToString();using(var r=await client.PostAsync($"/admin/certificates/{httpId}",new FormUrlEncodedContent(body)))Check(WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync()).Contains("Başka sekmede"),"SSR stale revision");
Console.WriteLine("PASS HTTP: directory, antiforgery cookie/header, upload/retry, private metadata/file access, attachment/nosniff/no-store, XSS encoding, SSR review/CSRF/retry/stale revision.");
}finally{if(!process.HasExited){process.Kill(entireProcessTree:true);await process.WaitForExitAsync();}await logs;await errors;}
sealed class Factory(DbContextOptions<AppDbContext> options):IDbContextFactory<AppDbContext>{public AppDbContext CreateDbContext()=>new(options);public Task<AppDbContext> CreateDbContextAsync(CancellationToken token=default)=>Task.FromResult(new AppDbContext(options));}
sealed class Host:IWebHostEnvironment {public string ApplicationName {get;set;}="EvidenceChecks";public string EnvironmentName {get;set;}="Development";public string ContentRootPath {get;set;}="";public string WebRootPath {get;set;}="";public IFileProvider ContentRootFileProvider {get;set;}=new NullFileProvider();public IFileProvider WebRootFileProvider {get;set;}=new NullFileProvider();}
