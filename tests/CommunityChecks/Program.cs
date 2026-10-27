using BlazorApp1.Data;
using BlazorApp1.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Claims;
var root=Path.GetFullPath(args[0]);var fixture=Path.Combine(root,".verification","community-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(fixture);
void Check(bool ok,string why){if(!ok)throw new Exception(why);}
var password=Guid.NewGuid().ToString("N")+"!";var hash=new PasswordHasher<string>().HashPassword("admin",password);
var settings=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Admin:PasswordHash",hash}}).Build();var access=new AdminAccess(settings);var admin=access.Principal();
var owner=new string('A',64);var voter=new string('B',64);var stranger=new string('C',64);
using var connection=new SqliteConnection("Data Source=:memory:");await connection.OpenAsync();var options=new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;var factory=new Factory(options);
await using var db=new AppDbContext(options);await db.Database.EnsureCreatedAsync();await CommunityService.InitializeAsync(db);await CommunityService.InitializeAsync(db);var service=new CommunityService(factory,access);
CommunityInput Input()=>new(){SubmissionId=Guid.NewGuid(),Place="TEST ONLY <script>alert(1)</script>",City="Test city",Country="Test country",Food="Test meal",Question="TEST what ingredients?",Answer="TEST user answer; no actual restaurant",VisitedOn=DateOnly.FromDateTime(DateTime.UtcNow),SourceUrl="https://example.com/test-only",ConsentPublic=true};
var input=Input();input.ConsentPublic=false;Check((await service.SubmitAsync(owner,input)).HttpStatus==400,"Public consent missing");input.ConsentPublic=true;
Check((await service.SubmitAsync(null,input)).HttpStatus==400,"Identity missing");input.VisitedOn=DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2);Check((await service.SubmitAsync(owner,input)).HttpStatus==400,"Future visit");input.VisitedOn=DateOnly.FromDateTime(DateTime.UtcNow);
Check((await service.SubmitAsync(owner,input)).HttpStatus==201,"Submit");Check((await service.SubmitAsync(owner,input)).HttpStatus==200,"Duplicate submission");Check((await service.SubmitAsync(stranger,input)).HttpStatus==409,"Other identity overwrote post");input.Answer+=" changed";Check((await service.SubmitAsync(owner,input)).HttpStatus==409,"Payload conflict");input.Answer=input.Answer.Replace(" changed","");
Check((await service.ListAsync(voter)).Total==0,"Pending post public");Check((await service.ListAsync(stranger,1,true)).Total==0,"Other owner private list");Check((await service.ListAsync(owner,1,true)).Total==1,"Own pending missing");
Check((await service.VoteAsync(voter,input.SubmissionId,"helpful")).HttpStatus==404,"Pending vote accepted");
try{await service.QueueAsync(new ClaimsPrincipal());throw new Exception("Anonymous moderation");}catch(UnauthorizedAccessException){}
var decision=new ReviewInput {ActionId=Guid.NewGuid(),ExpectedRevision=0,Status="published",Note="TEST public moderation only"};
Check((await service.ReviewAsync(admin,input.SubmissionId,decision)).HttpStatus==200,"Publish");Check((await service.ReviewAsync(admin,input.SubmissionId,decision)).HttpStatus==200,"Review retry");
var stale=new ReviewInput {ActionId=Guid.NewGuid(),ExpectedRevision=0,Status="hidden",Note="TEST old revision"};Check((await service.ReviewAsync(admin,input.SubmissionId,stale)).HttpStatus==409,"Stale review");
Check((await service.VoteAsync(owner,input.SubmissionId,"helpful")).HttpStatus==400,"Self vote");
Check((await service.VoteAsync(voter,input.SubmissionId,"helpful")).HttpStatus==200,"Vote");await service.VoteAsync(voter,input.SubmissionId,"helpful");
var card=(await service.ListAsync(voter)).Items.Single();Check(card.Helpful==1&&card.MyVote=="helpful"&&card.Stats.Badge=="İlk paylaşım"&&card.Stats.Helpful==1,"Duplicate vote or initial badge");
await service.VoteAsync(voter,input.SubmissionId,"outdated");card=(await service.ListAsync(voter)).Items.Single();Check(card.Helpful==0&&card.Outdated==1&&card.Stats.Helpful==0,"Vote replacement");
await service.VoteAsync(voter,input.SubmissionId,"clear");card=(await service.ListAsync(voter)).Items.Single();Check(card.Outdated==0&&card.MyVote=="","Vote removal");
for(var i=0;i<2;i++){var extra=Input();await service.SubmitAsync(owner,extra);await service.ReviewAsync(admin,extra.SubmissionId,new(){ActionId=Guid.NewGuid(),ExpectedRevision=0,Status="published",Note="TEST contribution count only"});}
Check((await service.ListAsync(voter)).Items.All(x=>x.Stats is {Published:3,Sourced:3,Badge:"Düzenli katkı"}),"Contribution badge");
await service.VoteAsync(voter,input.SubmissionId,"helpful");await service.ReviewAsync(admin,input.SubmissionId,new(){ActionId=Guid.NewGuid(),ExpectedRevision=1,Status="hidden",Note="TEST remove public listing"});Check((await service.ListAsync(voter)).Items.All(x=>x.Stats.Published==2&&x.Stats.Helpful==0),"Hidden contribution affects public stats");Check((await service.VoteAsync(voter,input.SubmissionId,"helpful")).HttpStatus==404,"Hidden vote");
Check((await service.WithdrawAsync(stranger,input.SubmissionId,2)).HttpStatus==404,"Unauthorized withdrawal");Check((await service.WithdrawAsync(owner,input.SubmissionId,0)).HttpStatus==409,"Stale withdrawal");Check((await service.WithdrawAsync(owner,input.SubmissionId,2)).HttpStatus==200,"Withdrawal");
Check((await service.ReviewAsync(admin,input.SubmissionId,new(){ActionId=Guid.NewGuid(),ExpectedRevision=3,Status="published",Note="TEST cannot restore owner withdrawal"})).HttpStatus==409,"Withdrawn post republished");Check((await service.GetAsync(admin,input.SubmissionId))?.History.Count==3,"Audit history");Check(await db.Products.CountAsync()==0&&await db.CertificateEvidence.CountAsync()==0,"Catalog/certificate side effect");
var identities=new CommunityIdentity(DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(fixture,"keys"))));var ctx=new DefaultHttpContext();identities.Ensure(ctx);var cookie=ctx.Response.Headers.SetCookie.ToString().Split(';')[0];var read=new DefaultHttpContext();read.Request.Headers.Cookie=cookie;Check(identities.Read(read)?.Length==64,"Identity cookie");var tampered=new DefaultHttpContext();tampered.Request.Headers.Cookie="helal-community-v1=tampered";Check(identities.Read(tampered) is null,"Forged identity");
Console.WriteLine("PASS service: consent/identity, pending privacy, idempotency, owner isolation, self/duplicate/replace/remove vote, badges, hidden stats, withdrawal, stale revision/audit, catalog/certificate separation, cookie tampering.");
var start=new ProcessStartInfo("dotnet"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=root};foreach(var arg in new[]{Path.Combine(root,"BlazorApp1/bin/Debug/net10.0/BlazorApp1.dll"),"--urls","http://127.0.0.1:5097","--contentRoot",Path.Combine(root,"BlazorApp1")})start.ArgumentList.Add(arg);
start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";start.Environment["Admin__PasswordHash"]=hash;start.Environment["Storage__DataDirectory"]=Path.Combine(fixture,"http-data");start.Environment["Logging__LogLevel__Default"]="Warning";
using var process=Process.Start(start)!;var logs=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();
string Value(string html,string name){var m=Regex.Match(html,"name=\""+Regex.Escape(name)+"\" value=\"([^\"]*)\"");Check(m.Success,"Missing field: "+name);return WebUtility.HtmlDecode(m.Groups[1].Value);}
HttpClient Client()=>new(new HttpClientHandler {AllowAutoRedirect=false,CookieContainer=new CookieContainer()}){BaseAddress=new Uri("http://127.0.0.1:5097")};
try {
using var author=Client();using var other=Client();using var moderator=Client();var ready=false;for(var i=0;i<50;i++){try{using var r=await author.GetAsync("/community");if(r.IsSuccessStatusCode){ready=true;break;}}catch(HttpRequestException){}await Task.Delay(200);}Check(ready,"Server not ready");
var html=await author.GetStringAsync("/community/new");var httpId=Value(html,"Input.SubmissionId");
var post=new Dictionary<string,string>{{"_handler","community-new"},{"Input.SubmissionId",httpId},{"Input.Place","TEST <script>alert(1)</script>"},{"Input.City","Test city"},{"Input.Country","Test country"},{"Input.Food","Test meal"},{"Input.Question","TEST ingredients question"},{"Input.Answer","TEST user experience; no real business"},{"Input.VisitedOn",DateTime.UtcNow.ToString("yyyy-MM-dd")},{"Input.ConsentPublic","true"}};
using(var r=await author.PostAsync("/community/new",new FormUrlEncodedContent(post)))Check(r.StatusCode==HttpStatusCode.BadRequest,"Submit CSRF");
post["__RequestVerificationToken"]=Value(html,"__RequestVerificationToken");using(var r=await author.PostAsync("/community/new",new FormUrlEncodedContent(post))){var result=WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());Check(result.Contains("Paylaşım incelemeye alındı"),"SSR submit: "+result[..Math.Min(200,result.Length)]);}
Check(!(await other.GetStringAsync("/community")).Contains("TEST ingredients question"),"Pending publicly visible");Check((await author.GetStringAsync("/community/mine")).Contains("TEST ingredients question"),"Own pending missing");Check(!(await other.GetStringAsync("/community/mine")).Contains("TEST ingredients question"),"Owner isolation HTTP");
using(var r=await other.GetAsync("/admin/community/"+httpId))Check(r.StatusCode==HttpStatusCode.Redirect,"Anonymous admin");
var login=await moderator.GetStringAsync("/admin/login");using(var r=await moderator.PostAsync("/admin/signin",new FormUrlEncodedContent(new Dictionary<string,string>{{"username","admin"},{"password",password},{"__RequestVerificationToken",Value(login,"__RequestVerificationToken")}})))Check(r.StatusCode==HttpStatusCode.Redirect,"Test login");
var detail=await moderator.GetStringAsync("/admin/community/"+httpId);var review=new Dictionary<string,string>{{"_handler","community-review"},{"Input.ActionId",Value(detail,"Input.ActionId")},{"Input.ExpectedRevision",Value(detail,"Input.ExpectedRevision")},{"Input.Status","published"},{"Input.Note","TEST moderation; not real business"},{"__RequestVerificationToken",Value(detail,"__RequestVerificationToken")}};
using(var r=await moderator.PostAsync("/admin/community/"+httpId,new FormUrlEncodedContent(review)))Check(WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync()).Contains("İnceleme kaydedildi"),"SSR publish");
html=await other.GetStringAsync("/community");Check(html.Contains("TEST ingredients question")&&!html.Contains("<script>alert(1)</script>"),"Published rendering/XSS");Check(!html.Contains("TEST moderation; not real business"),"Private moderator note public");
var vote=new Dictionary<string,string>{{"id",httpId},{"value","helpful"}};
using(var r=await other.PostAsync("/community/vote",new FormUrlEncodedContent(vote)))Check(r.StatusCode==HttpStatusCode.BadRequest,"Vote CSRF");vote["__RequestVerificationToken"]=Value(html,"__RequestVerificationToken");
using(var r=await other.PostAsync("/community/vote",new FormUrlEncodedContent(vote)))Check(r.StatusCode==HttpStatusCode.Redirect,"HTTP vote");
html=WebUtility.HtmlDecode(await other.GetStringAsync("/community"));Check(html.Contains("Yararlı: 1")&&html.Contains("Oyunuz: Yararlı"),"Vote count/render");
var mine=await author.GetStringAsync("/community/mine");var withdraw=new Dictionary<string,string>{{"id",httpId},{"revision",Value(mine,"revision")},{"__RequestVerificationToken",Value(mine,"__RequestVerificationToken")}};
using(var r=await author.PostAsync("/community/withdraw",new FormUrlEncodedContent(withdraw)))Check(r.StatusCode==HttpStatusCode.Redirect,"HTTP withdrawal");
Check(!(await other.GetStringAsync("/community")).Contains("TEST ingredients question"),"Withdrawn still public");
Console.WriteLine("PASS HTTP: SSR submit/CSRF, private own list, anonymous admin denial, moderated publication, XSS encoding, private review notes, vote CSRF/counts, owner withdrawal/public removal.");
}finally{if(!process.HasExited){process.Kill(entireProcessTree:true);await process.WaitForExitAsync();}await logs;await errors;}
sealed class Factory(DbContextOptions<AppDbContext> options):IDbContextFactory<AppDbContext>{public AppDbContext CreateDbContext()=>new(options);public Task<AppDbContext> CreateDbContextAsync(CancellationToken token=default)=>Task.FromResult(new AppDbContext(options));}
