using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Services;

public sealed class AdminAccess(IConfiguration configuration)
{
    public const string Scheme = "Admin";
    public const string Role = "Administrator";
    private string Hash => configuration["Admin:PasswordHash"] ?? "";
    public bool Configured => Hash.Length > 0;
    public string Stamp => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Hash)));
    public bool Verify(string username, string password)
    {
        if (!Configured || username != "admin" || password.Length is < 12 or > 256) return false;
        try { return new PasswordHasher<string>().VerifyHashedPassword("admin", Hash, password) != PasswordVerificationResult.Failed; }
        catch (Exception ex) when (ex is FormatException or ArgumentException) { return false; }
    }
    public bool IsCurrent(ClaimsPrincipal? user) => Configured && user?.Identity?.IsAuthenticated == true && user.IsInRole(Role) && user.FindFirst("credential_stamp")?.Value == Stamp;
    public ClaimsPrincipal Principal() => new(new ClaimsIdentity(new[] {
        new Claim(ClaimTypes.NameIdentifier, "admin"), new Claim(ClaimTypes.Name, "admin"),
        new Claim(ClaimTypes.Role, Role), new Claim("credential_stamp", Stamp)
    }, Scheme));
}
