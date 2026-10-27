using Microsoft.AspNetCore.Antiforgery;
namespace BlazorApp1.Services;
public static class CommunityEndpoints
{
    public static void MapCommunity(this WebApplication app)
    {
        app.MapPost("/community/{action}",async (string action,HttpContext context,IAntiforgery af,CommunityIdentity identity,CommunityService service)=>{
            try{await af.ValidateRequestAsync(context);}catch(AntiforgeryValidationException){return Results.BadRequest("Form süresi dolmuş. Topluluk sayfasını yenileyin.");}
            var form=await context.Request.ReadFormAsync();if(!Guid.TryParse(form["id"],out var id))return Results.BadRequest("Paylaşım kimliği geçersiz.");
            CommunityResult result;
            if(action=="vote")result=await service.VoteAsync(identity.Read(context),id,form["value"].ToString());
            else if(action=="withdraw"&&int.TryParse(form["revision"],out var revision))result=await service.WithdrawAsync(identity.Read(context),id,revision);
            else return Results.NotFound();
            if(result.HttpStatus==200)return Results.Redirect(action=="withdraw"?"/community/mine?notice=withdrawn":"/community?notice=vote");
            var message=System.Text.Encodings.Web.HtmlEncoder.Default.Encode(result.Message);
            return Results.Content("<!doctype html><html lang='tr'><meta charset='utf-8'><title>Topluluk işlemi</title><main style='max-width:700px;margin:60px auto;font-family:system-ui'><h1>İşlem tamamlanamadı</h1><p>"+message+"</p><a href='/community'>Topluluğa dön</a> · <a href='/community/mine'>Paylaşımlarım</a></main></html>","text/html; charset=utf-8",statusCode:result.HttpStatus);
        }).RequireRateLimiting("correction-posts");
    }
}
