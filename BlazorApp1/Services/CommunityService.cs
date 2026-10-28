using System.Security.Claims;
using System.Text.Json;
using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;
using HelalRehber.Shared;
namespace BlazorApp1.Services;
public sealed class CommunityInput
{
    public Guid SubmissionId {get;set;}
    public string Place {get;set;}="";
    public string City {get;set;}="";
    public string Country {get;set;}="";
    public string Food {get;set;}="";
    public string Question {get;set;}="";
    public string Answer {get;set;}="";
    public string SourceUrl {get;set;}="";
    public DateOnly? VisitedOn {get;set;}
    public bool ConsentPublic {get;set;}
}
public sealed record CommunityResult(int HttpStatus,string Message);
public sealed record ContributorStats(int Published,int Sourced,int Helpful)
{
    public string Badge=>Published>=3?"Düzenli katkı":Published>0?"İlk paylaşım":"Yeni katılımcı";
}
public sealed record CommunityCard(Guid Id,CommunityInput Input,string Author,ContributorStats Stats,int Helpful,int Outdated,string MyVote,bool Own,string Status,int Revision);
public sealed record CommunityList(List<CommunityCard> Items,int Page,int Total);
public sealed record CommunityDetail(CommunityPost Row,CommunityInput Input,List<CommunityDecision> History);
public sealed class CommunityService(IDbContextFactory<AppDbContext> factory,AdminAccess access)
{
    public static string StatusLabel(string value)=>value switch {"pending"=>"İnceleme bekliyor","published"=>"Paylaşım yayında — içerik doğrulanmış değildir","hidden"=>"Yönetici tarafından gizlendi","withdrawn"=>"Paylaşım sahibi geri çekti",_=>"Durum bilinmiyor"};
    public static List<string> Validate(CommunityInput x)
    {
        x.Place=(x.Place??"").Trim();x.City=(x.City??"").Trim();x.Country=(x.Country??"").Trim();x.Food=(x.Food??"").Trim();x.Question=(x.Question??"").Trim();x.Answer=(x.Answer??"").Trim();x.SourceUrl=(x.SourceUrl??"").Trim();
        List<string> errors=[];
        if(x.SubmissionId==Guid.Empty)errors.Add("Gönderim kimliği eksik; yeni form açın.");
        if(x.Place.Length is <2 or >160||x.Food.Length is <2 or >160)errors.Add("İşletme/şube ve yemek adı 2–160 karakter olmalı.");
        if(x.City.Length is <2 or >100||x.Country.Length is <2 or >80)errors.Add("Şehir (2–100) ve ülke (2–80) bilgilerini girin.");
        if(x.Question.Length is <5 or >500||x.Answer.Length is <5 or >1200)errors.Add("Soruyu 5–500, alınan yanıtı/deneyimi 5–1.200 karakterle yazın.");
        if(x.VisitedOn is null||x.VisitedOn>DateOnly.FromDateTime(DateTime.UtcNow.AddHours(14))||x.VisitedOn<new DateOnly(2000,1,1))errors.Add("Geçmişteki ziyaret tarihini girin (2000 ve sonrası).");
        if(x.SourceUrl.Length>1000||(x.SourceUrl.Length>0&&!CertificateAuthorities.SafeHttps(x.SourceUrl)))errors.Add("Kaynak bağlantısı en fazla 1.000 karakter geçerli https adresi olmalı.");
        if(!x.ConsentPublic)errors.Add("İnceleme sonrası herkesle paylaşım onayını verin.");
        return errors;
    }
    private static bool Owner(string? key)=>key is not null&&System.Text.RegularExpressions.Regex.IsMatch(key,"^[A-F0-9]{64}$");
    private void Authorize(ClaimsPrincipal user){if(!access.IsCurrent(user))throw new UnauthorizedAccessException();}
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS CommunityPosts (Id TEXT NOT NULL PRIMARY KEY,OwnerKey TEXT NOT NULL,Payload TEXT NOT NULL,Status TEXT NOT NULL,Revision INTEGER NOT NULL,CreatedUtc TEXT NOT NULL)");
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CommunityPosts_Status_CreatedUtc ON CommunityPosts(Status,CreatedUtc)");
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS CommunityVotes (PostId TEXT NOT NULL,OwnerKey TEXT NOT NULL,Value TEXT NOT NULL,PRIMARY KEY(PostId,OwnerKey),FOREIGN KEY(PostId) REFERENCES CommunityPosts(Id) ON DELETE RESTRICT)");
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS CommunityDecisions (Id TEXT NOT NULL PRIMARY KEY,PostId TEXT NOT NULL,Revision INTEGER NOT NULL,Status TEXT NOT NULL,Note TEXT NOT NULL,Reviewer TEXT NOT NULL,CreatedUtc TEXT NOT NULL,FOREIGN KEY(PostId) REFERENCES CommunityPosts(Id) ON DELETE RESTRICT)");
        await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_CommunityDecisions_PostId_Revision ON CommunityDecisions(PostId,Revision)");
    }
    public async Task<CommunityResult> SubmitAsync(string? owner,CommunityInput input)
    {
        if(!Owner(owner))return new(400,"Tarayıcı kimliği yok. Girdilerinizi kopyalayıp yeni form açın; çerezler etkin olmalı.");
        var errors=Validate(input);if(errors.Count>0)return new(400,string.Join(" ",errors));
        var payload=JsonSerializer.Serialize(input);await using var db=await factory.CreateDbContextAsync();
        var old=await db.CommunityPosts.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==input.SubmissionId);
        if(old is not null)return old.OwnerKey==owner&&old.Payload==payload?new(200,"Paylaşım daha önce kaydedildi; durumunu Paylaşımlarım ekranından görebilirsiniz."):new(409,"Gönderim kimliği farklı içerikle kullanılmış. Yeni form açın.");
        var since=DateTime.UtcNow.AddDays(-1);
        if(await db.CommunityPosts.CountAsync()>=500||await db.CommunityPosts.CountAsync(x=>x.OwnerKey==owner&&x.CreatedUtc>=since)>=10)return new(429,"Geliştirme sürümü paylaşım sınırına ulaşıldı. Daha sonra deneyin.");
        db.CommunityPosts.Add(new(){Id=input.SubmissionId,OwnerKey=owner!,Payload=payload,Status="pending",Revision=0,CreatedUtc=DateTime.UtcNow});
        try{await db.SaveChangesAsync();return new(201,"Paylaşım incelemeye alındı. Şimdilik yalnız siz ve yönetici görebilirsiniz.");}catch(DbUpdateException){return new(409,"Gönderim eşzamanlı değişti. Paylaşımlarım ekranını kontrol edin.");}
    }
    public sealed record StatusNotice(Guid PostId, int Revision, string Status, DateTime CreatedUtc);
    public async Task<List<StatusNotice>> NoticesAsync(string? owner)
    {
        if (!Owner(owner)) return [];
        await using var db = await factory.CreateDbContextAsync();
        // Ownership comes only from the protected cookie. Internal reviewer notes never leave this query.
        return await (from decision in db.CommunityDecisions.AsNoTracking()
                      join post in db.CommunityPosts.AsNoTracking() on decision.PostId equals post.Id
                      where post.OwnerKey == owner
                      orderby decision.CreatedUtc descending, decision.Id descending
                      select new StatusNotice(post.Id, decision.Revision, decision.Status, decision.CreatedUtc))
                      .Take(100).ToListAsync();
    }
    public async Task<CommunityList> ListAsync(string? owner,int page=1,bool mine=false)
    {
        await using var db=await factory.CreateDbContextAsync();
        var q=db.CommunityPosts.AsNoTracking().Where(x=>mine?x.OwnerKey==owner&&owner!=null:x.Status=="published");
        var total=await q.CountAsync();page=Math.Clamp(page,1,Math.Max(1,(total+19)/20));
        var rows=await q.OrderByDescending(x=>x.CreatedUtc).ThenBy(x=>x.Id).Skip((page-1)*20).Take(20).ToListAsync();
        var ids=rows.Select(x=>x.Id).ToArray();var authors=rows.Select(x=>x.OwnerKey).Distinct().ToArray();
        var votes=await db.CommunityVotes.AsNoTracking().Where(x=>ids.Contains(x.PostId)).ToListAsync();
        var published=await db.CommunityPosts.AsNoTracking().Where(x=>x.Status=="published"&&authors.Contains(x.OwnerKey)).ToListAsync();
        var authorPostIds=published.Select(x=>x.Id).ToArray();
        var helpful=await db.CommunityVotes.AsNoTracking().Where(x=>authorPostIds.Contains(x.PostId)&&x.Value=="helpful").Select(x=>x.PostId).ToListAsync();
        return new(rows.Select(row=>{
            var posts=published.Where(x=>x.OwnerKey==row.OwnerKey).ToList();var postIds=posts.Select(x=>x.Id).ToHashSet();
            return new CommunityCard(row.Id,JsonSerializer.Deserialize<CommunityInput>(row.Payload)!,"Katılımcı "+row.OwnerKey[..10],new(posts.Count,posts.Count(x=>JsonSerializer.Deserialize<CommunityInput>(x.Payload)!.SourceUrl.Length>0),helpful.Count(postIds.Contains)),row.Status=="published"?votes.Count(x=>x.PostId==row.Id&&x.Value=="helpful"):0,row.Status=="published"?votes.Count(x=>x.PostId==row.Id&&x.Value=="outdated"):0,votes.FirstOrDefault(x=>x.PostId==row.Id&&x.OwnerKey==owner)?.Value??"",row.OwnerKey==owner,row.Status,row.Revision);
        }).ToList(),page,total);
    }
    public async Task<CommunityResult> VoteAsync(string? owner,Guid id,string value)
    {
        if(!Owner(owner)||value is not ("helpful" or "outdated" or "clear"))return new(400,"Oy/kimlik geçersiz. Topluluk ekranını yeniden açın.");
        await using var db=await factory.CreateDbContextAsync();
        // Transaction serializes the read/status check with a concurrent hide/withdraw operation in SQLite.
        await using var transaction=await db.Database.BeginTransactionAsync();
        var post=await db.CommunityPosts.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&x.Status=="published");
        if(post is null)return new(404,"Paylaşım yayında değil.");if(post.OwnerKey==owner)return new(400,"Kendi paylaşımınıza oy veremezsiniz.");
        var vote=await db.CommunityVotes.SingleOrDefaultAsync(x=>x.PostId==id&&x.OwnerKey==owner);
        if(value=="clear"){if(vote is not null)db.CommunityVotes.Remove(vote);}
        else if(vote is null)db.CommunityVotes.Add(new(){PostId=id,OwnerKey=owner!,Value=value});else vote.Value=value;
        try{await db.SaveChangesAsync();await transaction.CommitAsync();return new(200,value=="clear"?"Oyunuz kaldırıldı.":"Oyunuz kaydedildi. Bu oy uygunluk veya sertifika onayı değildir.");}catch(DbUpdateException){return new(409,"Oy eşzamanlı değişti. Sayfayı yenileyin.");}
    }
    public async Task<CommunityResult> WithdrawAsync(string? owner,Guid id,int revision)
    {
        if(!Owner(owner))return new(400,"Tarayıcı kimliği gerekli.");await using var db=await factory.CreateDbContextAsync();
        var row=await db.CommunityPosts.SingleOrDefaultAsync(x=>x.Id==id&&x.OwnerKey==owner);if(row is null)return new(404,"Paylaşım bulunamadı.");
        if(row.Status=="withdrawn")return new(200,"Paylaşım zaten geri çekildi.");if(row.Revision!=revision)return new(409,"Paylaşım başka sekmede değişti. Sayfayı yenileyin.");
        row.Status="withdrawn";row.Revision++;db.CommunityDecisions.Add(new(){Id=Guid.NewGuid(),PostId=id,Revision=row.Revision,Status="withdrawn",Note="Paylaşım sahibi geri çekti.",Reviewer="owner",CreatedUtc=DateTime.UtcNow});
        try{await db.SaveChangesAsync();return new(200,"Paylaşım yayından kaldırıldı. İnceleme geçmişi sunucuda korunur.");}catch(DbUpdateException){return new(409,"Paylaşım eşzamanlı değişti. Sayfayı yenileyin.");}
    }
    public async Task<List<CommunityPost>> QueueAsync(ClaimsPrincipal user){Authorize(user);await using var db=await factory.CreateDbContextAsync();return await db.CommunityPosts.AsNoTracking().OrderByDescending(x=>x.CreatedUtc).ThenBy(x=>x.Id).Take(500).ToListAsync();}
    public async Task<CommunityDetail?> GetAsync(ClaimsPrincipal user,Guid id){Authorize(user);await using var db=await factory.CreateDbContextAsync();var row=await db.CommunityPosts.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id);return row is null?null:new(row,JsonSerializer.Deserialize<CommunityInput>(row.Payload)!,await db.CommunityDecisions.AsNoTracking().Where(x=>x.PostId==id).OrderByDescending(x=>x.Revision).ToListAsync());}
    public async Task<CommunityResult> ReviewAsync(ClaimsPrincipal user,Guid id,ReviewInput input)
    {
        Authorize(user);if(input.ActionId==Guid.Empty||input.ExpectedRevision<0||input.Status is not ("pending" or "published" or "hidden")||(input.Note?.Trim().Length??0) is <5 or >2000)return new(400,"Durum ve 5–2.000 karakter gerekçe gerekli.");
        await using var db=await factory.CreateDbContextAsync();var old=await db.CommunityDecisions.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==input.ActionId);
        if(old is not null)return old.PostId==id&&old.Status==input.Status&&old.Note==input.Note!.Trim()?new(200,"İnceleme daha önce kaydedildi."):new(409,"İşlem kimliği farklı içerikle kullanıldı.");
        var row=await db.CommunityPosts.SingleOrDefaultAsync(x=>x.Id==id);if(row is null)return new(404,"Paylaşım bulunamadı.");
        if(row.Status=="withdrawn")return new(409,"Sahibi geri çekti; yeniden yayınlanamaz.");if(row.Revision!=input.ExpectedRevision)return new(409,"Paylaşım başka sekmede değişti. Formu yenileyin.");
        row.Status=input.Status;row.Revision++;db.CommunityDecisions.Add(new(){Id=input.ActionId,PostId=id,Revision=row.Revision,Status=input.Status,Note=input.Note!.Trim(),Reviewer=user.Identity?.Name??"admin",CreatedUtc=DateTime.UtcNow});
        try{await db.SaveChangesAsync();return new(200,"İnceleme kaydedildi. Yayınlama içerik/helal/sertifika doğrulaması değildir.");}catch(DbUpdateException){return new(409,"İnceleme eşzamanlı değişti. Formu yenileyin.");}
    }
}
