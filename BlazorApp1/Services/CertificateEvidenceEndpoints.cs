using System.Globalization;
using HelalRehber.Shared;
using Microsoft.AspNetCore.Antiforgery;
namespace BlazorApp1.Services;
public static class CertificateEvidenceEndpoints
{
    public static void MapCertificateEvidence(this WebApplication app)
    {
        app.MapGet("/api/certificates/form-token",(HttpContext ctx,IAntiforgery af)=>{ctx.Response.Headers.CacheControl="no-store";var tokens=af.GetAndStoreTokens(ctx);return Results.Ok(new {Token=tokens.RequestToken,Header=tokens.HeaderName});});
        app.MapPost("/api/certificates/submissions",async (HttpContext ctx,IAntiforgery af,CertificateEvidenceService service,CancellationToken token)=>{
            try {
                await af.ValidateRequestAsync(ctx);
                if(!ctx.Request.HasFormContentType)return Results.BadRequest(new {Message="Form verisi gerekli."});
                var f=await ctx.Request.ReadFormAsync(new Microsoft.AspNetCore.Http.Features.FormOptions {MultipartBodyLengthLimit=6*1024*1024,ValueLengthLimit=12000,ValueCountLimit=32},token);
                if(f.Files.Count>1)return Results.BadRequest(new {Message="Tek belge yükleyin."});
                if(!Guid.TryParse(f["SubmissionId"],out var id))return Results.BadRequest(new {Message="Gönderim kimliği geçersiz."});
                DateOnly? from=null,until=null;
                if(f["ValidFrom"].ToString().Length>0){if(!DateOnly.TryParseExact(f["ValidFrom"],"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date))return Results.BadRequest(new {Message="Başlangıç tarihi geçersiz."});from=date;}
                if(f["ValidUntil"].ToString().Length>0){if(!DateOnly.TryParseExact(f["ValidUntil"],"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date))return Results.BadRequest(new {Message="Bitiş tarihi geçersiz."});until=date;}
                var input=new CertificateEvidenceInput {SubmissionId=id,Kind=f["Kind"].ToString(),Reference=f["Reference"].ToString(),Name=f["Name"].ToString(),Issuer=f["Issuer"].ToString(),Number=f["Number"].ToString(),Scope=f["Scope"].ToString(),SourceUrl=f["SourceUrl"].ToString(),Notes=f["Notes"].ToString(),Consent=f["Consent"]=="true",ValidFrom=from,ValidUntil=until};
                var result=await service.SubmitAsync(input,f.Files.FirstOrDefault(),token);
                return Results.Json(new {result.Message,result.Id},statusCode:result.HttpStatus);
            } catch(AntiforgeryValidationException){return Results.BadRequest(new {Message="Form süresi dolmuş. Girdilerinizi kopyalayıp sayfayı yenileyin."});}
            catch(Exception ex) when(ex is InvalidDataException or BadHttpRequestException){return Results.BadRequest(new {Message="Form veya dosya sınırı aşıldı. En fazla 5 MiB PDF/JPG/PNG kullanın."});}
        }).RequireRateLimiting("translation");
        app.MapGet("/api/admin/certificates/{id:guid}/file",async (Guid id,HttpContext ctx,CertificateEvidenceService service,CancellationToken token)=>{
            ctx.Response.Headers.CacheControl="no-store";ctx.Response.Headers["X-Content-Type-Options"]="nosniff";
            try {var file=await service.FileAsync(ctx.User,id,token);return file is null?Results.NotFound():Results.File(file.Value.Bytes,file.Value.Mime,file.Value.Name);}
            catch(Exception ex) when(ex is IOException or UnauthorizedAccessException){return Results.Problem("Belge okunamadı veya bütünlüğü doğrulanamadı.",statusCode:503);}
        }).RequireAuthorization("Admin");
    }
}
