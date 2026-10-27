using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
namespace BlazorApp1.Services;
// A protected browser identity, not a verified person/account. Never accept identity from form data.
public sealed class CommunityIdentity(IDataProtectionProvider provider)
{
    public const string Cookie="helal-community-v1";
    private readonly IDataProtector protector=provider.CreateProtector("HelalRehber.Community.v1");
    public string? Read(HttpContext? context)
    {
        if(context is null||!context.Request.Cookies.TryGetValue(Cookie,out var cookie))return null;
        try {
            var parts=protector.Unprotect(cookie).Split('|');
            if(parts.Length!=2||!Guid.TryParseExact(parts[0],"N",out _)||!long.TryParse(parts[1],out var until)||until<=DateTimeOffset.UtcNow.ToUnixTimeSeconds())return null;
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(parts[0])));
        }catch(CryptographicException){return null;}
    }
    public void Ensure(HttpContext context)
    {
        if(Read(context) is not null)return;
        var payload=Guid.NewGuid().ToString("N")+"|"+DateTimeOffset.UtcNow.AddDays(365).ToUnixTimeSeconds();
        context.Response.Cookies.Append(Cookie,protector.Protect(payload),new CookieOptions {HttpOnly=true,Secure=context.Request.IsHttps,SameSite=SameSiteMode.Strict,IsEssential=true,Path="/",MaxAge=TimeSpan.FromDays(365)});
    }
}
