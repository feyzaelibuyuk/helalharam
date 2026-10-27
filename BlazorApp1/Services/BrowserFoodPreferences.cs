using HelalRehber.Shared;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text.Json;
namespace BlazorApp1.Services;
public sealed class BrowserFoodPreferences(IDataProtectionProvider provider,IHttpContextAccessor context)
{
    public const string Cookie="helal-food-preferences-v1";
    private readonly IDataProtector protector=provider.CreateProtector("HelalRehber.FoodPreferences.v1");
    public FoodPreferences Read(){try{var cookie=context.HttpContext?.Request.Cookies[Cookie];return cookie is null?new():FoodPreferenceRules.Read(protector.Unprotect(cookie));}catch(CryptographicException){return new();}}
    public void Write(HttpContext ctx,FoodPreferences p){var options=new CookieOptions{HttpOnly=true,Secure=ctx.Request.IsHttps,SameSite=SameSiteMode.Strict,Path="/",MaxAge=TimeSpan.FromDays(365),IsEssential=true};if(!p.Active)ctx.Response.Cookies.Delete(Cookie,options);else ctx.Response.Cookies.Append(Cookie,protector.Protect(JsonSerializer.Serialize(p)),options);}
}
