using HelalRehber.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Security.Cryptography;

namespace BlazorApp1.Services;

public sealed class BrowserFavorites(IDataProtectionProvider protection)
{
    private const string Cookie = "helal-favorites-v1";
    private readonly IDataProtector protector = protection.CreateProtector("HelalRehber.Favorites.v1");
    private readonly ChunkingCookieManager cookies = new();
    public List<FavoriteItem> Read(HttpContext? context)
    {
        if (context is null) return [];
        try
        {
            var data = cookies.GetRequestCookie(context, Cookie);
            return data is null ? [] : FavoriteList.Read(protector.Unprotect(data));
        }
        catch (CryptographicException) { return []; }
    }
    public void Write(HttpContext context, List<FavoriteItem> items)
    {
        var options = new CookieOptions { HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Lax, IsEssential = true, Path = "/", MaxAge = TimeSpan.FromDays(365) };
        if (items.Count == 0) cookies.DeleteCookie(context, Cookie, options);
        else cookies.AppendResponseCookie(context, Cookie, protector.Protect(JsonSerializer.Serialize(items)), options);
    }
}
