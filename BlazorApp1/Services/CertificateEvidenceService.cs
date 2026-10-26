using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlazorApp1.Data;
using BlazorApp1.Models;
using HelalRehber.Shared;
using Microsoft.EntityFrameworkCore;
namespace BlazorApp1.Services;
public sealed class EvidenceDecision
{
    public Guid ActionId { get; set; } public int ExpectedRevision { get; set; }
    public string Status { get; set; }="needs_info"; public string Note { get; set; }="";
    public string CheckedUrl { get; set; }="";
    public bool SourceChecked { get; set; } public bool NumberMatched { get; set; } public bool ScopeMatched { get; set; }
    public bool DatesChecked { get; set; } public bool RevocationChecked { get; set; }
}
public sealed record EvidenceResult(int HttpStatus,string Message,Guid? Id=null);
public sealed record EvidenceDetail(CertificateEvidence Row,CertificateEvidenceInput Input,List<CertificateEvidenceReview> History);
public sealed class CertificateEvidenceService(IDbContextFactory<AppDbContext> factory,AdminAccess access,IWebHostEnvironment env,IConfiguration config)
{
    private readonly SemaphoreSlim uploads=new(1,1);
    private string UnsafeRoot=>Path.Combine(string.IsNullOrWhiteSpace(config["Storage:DataDirectory"])?env.ContentRootPath:Path.GetFullPath(config["Storage:DataDirectory"]!,env.ContentRootPath),"certificate-evidence");
    private string Root { get { var path=Path.GetFullPath(UnsafeRoot);var web=Path.GetFullPath(env.WebRootPath??Path.Combine(env.ContentRootPath,"wwwroot")).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;if((path+Path.DirectorySeparatorChar).StartsWith(web,StringComparison.OrdinalIgnoreCase))throw new IOException("Private storage required.");return path; } }
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS CertificateEvidence (
        Id TEXT NOT NULL PRIMARY KEY, Payload TEXT NOT NULL,Fingerprint TEXT NOT NULL,FileKey TEXT NOT NULL,FileName TEXT NOT NULL,Mime TEXT NOT NULL,
        FileBytes INTEGER NOT NULL,Sha256 TEXT NOT NULL,Status TEXT NOT NULL,Revision INTEGER NOT NULL,CreatedUtc TEXT NOT NULL);
        """);
        await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS CertificateEvidenceReviews (
        Id TEXT NOT NULL PRIMARY KEY,SubmissionId TEXT NOT NULL,Revision INTEGER NOT NULL,Status TEXT NOT NULL,Note TEXT NOT NULL,ChecksJson TEXT NOT NULL,Reviewer TEXT NOT NULL,CreatedUtc TEXT NOT NULL,
        FOREIGN KEY (SubmissionId) REFERENCES CertificateEvidence(Id) ON DELETE RESTRICT);
        """);
        await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_CertificateEvidenceReviews_SubmissionId_Revision ON CertificateEvidenceReviews(SubmissionId,Revision)");
    }
    public async Task<EvidenceResult> SubmitAsync(CertificateEvidenceInput input,IFormFile? file,CancellationToken token=default)
    {
        var errors=CertificateEvidenceRules.Validate(input);if(errors.Count>0)return new(400,string.Join(" ",errors));
        if(file is null&&input.SourceUrl.Length==0)return new(400,"Belge dosyası veya kaynak bağlantısı ekleyin.");
        if(file is not null&&(file.Length is <=0 or >CertificateEvidenceRules.MaxFileBytes))return new(400,"Dosya en fazla 5 MiB PDF/JPG/PNG olabilir.");
        if(!await uploads.WaitAsync(0,token))return new(429,"Başka belge yüklemesi işleniyor. Biraz sonra tekrar deneyin.");
        string? ownedPath=null;var saved=false;
        try
        {
            byte[] bytes=[];var mime="";var name="";
            if(file is not null)
            {
                await using var stream=file.OpenReadStream();using var buffer=new MemoryStream();var chunk=new byte[65536];int read;
                while((read=await stream.ReadAsync(chunk,token))>0){if(buffer.Length+read>CertificateEvidenceRules.MaxFileBytes)return new(400,"Dosya 5 MiB sınırını aşıyor.");buffer.Write(chunk,0,read);}
                bytes=buffer.ToArray();mime=CertificateEvidenceRules.Mime(bytes)??"";
                var extension=Path.GetExtension(file.FileName).ToLowerInvariant();
                if(mime.Length==0||!((mime=="application/pdf"&&extension==".pdf")||(mime=="image/png"&&extension==".png")||(mime=="image/jpeg"&&extension is ".jpg" or ".jpeg")))return new(400,"Dosyanın uzantısı ve içeriği PDF/JPG/PNG ile eşleşmeli. SVG/HTML/çalıştırılabilir dosya alınmaz.");
                name=Path.GetFileName(file.FileName.Replace('\\','/'));if(name.Length>160)name=name[^160..];
            }
            var payload=JsonSerializer.Serialize(input);var sha=bytes.Length==0?"":Convert.ToHexString(SHA256.HashData(bytes));
            var fingerprint=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload+"|"+sha+"|"+name)));
            await using var db=await factory.CreateDbContextAsync(token);
            var existing=await db.CertificateEvidence.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==input.SubmissionId,token);
            if(existing is not null)return existing.Fingerprint==fingerprint?new(200,"Belge talebi daha önce kaydedildi.",existing.Id):new(409,"Gönderim kimliği farklı içerikte kullanılmış. Yeni form açın.");
            if(await db.CertificateEvidence.CountAsync(token)>=100||(await db.CertificateEvidence.Select(x=>(long?)x.FileBytes).SumAsync(token)??0)+bytes.Length>100*1024*1024)return new(429,"Geliştirme sürümünün belge depolama sınırına ulaşıldı. Yönetici kontrol etmeli.");
            var key="";
            if(bytes.Length>0){Directory.CreateDirectory(Root);key=Guid.NewGuid().ToString("N")+".bin";ownedPath=Path.Combine(Root,key);await File.WriteAllBytesAsync(ownedPath,bytes,token);}
            db.CertificateEvidence.Add(new(){Id=input.SubmissionId,Payload=payload,Fingerprint=fingerprint,FileKey=key,FileName=name,Mime=mime,FileBytes=bytes.Length,Sha256=sha,Status="pending",Revision=0,CreatedUtc=DateTime.UtcNow});
            await db.SaveChangesAsync(token);saved=true;return new(201,"Belge inceleme talebi kaydedildi. Sertifika veya katalog onayı verilmedi.",input.SubmissionId);
        }
        catch(DbUpdateException){return new(409,"Gönderim eşzamanlı değişti. Aynı gönderimi tekrar kontrol edin.");}
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException){return new(503,"Belge saklanamadı. Daha sonra tekrar deneyin.");}
        finally{try{if(!saved&&ownedPath is not null)File.Delete(ownedPath);}catch(IOException){}catch(UnauthorizedAccessException){}finally{uploads.Release();}}
    }
    private void Authorize(ClaimsPrincipal user){if(!access.IsCurrent(user))throw new UnauthorizedAccessException();}
    public async Task<List<CertificateEvidence>> QueueAsync(ClaimsPrincipal user)
    {Authorize(user);await using var db=await factory.CreateDbContextAsync();return await db.CertificateEvidence.AsNoTracking().OrderByDescending(x=>x.CreatedUtc).Take(100).ToListAsync();}
    public async Task<EvidenceDetail?> GetAsync(ClaimsPrincipal user,Guid id)
    {
        Authorize(user);await using var db=await factory.CreateDbContextAsync();var row=await db.CertificateEvidence.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id);if(row is null)return null;
        return new(row,JsonSerializer.Deserialize<CertificateEvidenceInput>(row.Payload)!,await db.CertificateEvidenceReviews.AsNoTracking().Where(x=>x.SubmissionId==id).OrderByDescending(x=>x.Revision).ToListAsync());
    }
    public async Task<(byte[] Bytes,string Mime,string Name)?> FileAsync(ClaimsPrincipal user,Guid id,CancellationToken token)
    {
        var detail=await GetAsync(user,id);if(detail is null||detail.Row.FileKey.Length==0)return null;
        var row=detail.Row;if(!System.Text.RegularExpressions.Regex.IsMatch(row.FileKey,"^[a-f0-9]{32}\\.bin$"))return null;
        var path=Path.Combine(Root,row.FileKey);if(!File.Exists(path))return null;var bytes=await File.ReadAllBytesAsync(path,token);
        if(bytes.Length!=row.FileBytes||Convert.ToHexString(SHA256.HashData(bytes))!=row.Sha256)throw new InvalidDataException("Belge özeti eşleşmedi.");
        return (bytes,row.Mime,row.FileName);
    }
    public async Task<EvidenceResult> ReviewAsync(ClaimsPrincipal user,Guid id,EvidenceDecision decision)
    {
        Authorize(user);decision.Note??="";decision.CheckedUrl??="";
        if(decision.ActionId==Guid.Empty||decision.ExpectedRevision<0||decision.Status is not ("pending" or "needs_info" or "reviewed" or "rejected")||decision.Note.Trim().Length is <5 or >2000||decision.CheckedUrl.Length>1000)return new(400,"Durum ve 5–2.000 karakter inceleme notu gerekli.");
        await using var db=await factory.CreateDbContextAsync();var existing=await db.CertificateEvidenceReviews.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==decision.ActionId);
        var checks=JsonSerializer.Serialize(decision);
        if(existing is not null)return existing.SubmissionId==id&&existing.ChecksJson==checks?new(200,"İnceleme daha önce kaydedildi.",id):new(409,"İşlem kimliği farklı incelemede kullanıldı.");
        var row=await db.CertificateEvidence.SingleOrDefaultAsync(x=>x.Id==id);if(row is null)return new(404,"Belge talebi bulunamadı.");
        if(row.Revision!=decision.ExpectedRevision)return new(409,"Başka sekmede değişti. Notu koruyup formu yenileyin.");
        var input=JsonSerializer.Deserialize<CertificateEvidenceInput>(row.Payload)!;
        if(decision.Status=="reviewed"&&(!decision.SourceChecked||!decision.NumberMatched||!decision.ScopeMatched||!decision.DatesChecked||!decision.RevocationChecked||!CertificateAuthorities.IsRecordUrl(input.Issuer,decision.CheckedUrl)))return new(400,"İncelemeyi tamamlamak için beş kontrolü ve belgeye özgü resmî kaynak bağlantısını doğrulayın. Genel kaynak dizini tek başına kanıt değildir.");
        if(decision.CheckedUrl.Length>0&&!CertificateAuthorities.SafeHttps(decision.CheckedUrl))return new(400,"Kontrol bağlantısı geçerli https adresi olmalı.");
        row.Revision++;row.Status=decision.Status;
        db.CertificateEvidenceReviews.Add(new(){Id=decision.ActionId,SubmissionId=id,Revision=row.Revision,Status=decision.Status,Note=decision.Note.Trim(),ChecksJson=checks,Reviewer=user.Identity?.Name??"admin",CreatedUtc=DateTime.UtcNow});
        try{await db.SaveChangesAsync();return new(200,"İnceleme kaydedildi. Katalog/sertifika uygunluğu otomatik değiştirilmedi.",id);}catch(DbUpdateException){return new(409,"İnceleme eşzamanlı değişti. Formu yenileyin.");}
    }
}
