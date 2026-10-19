using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
if (args.Contains("--help")) { Console.WriteLine("Kullanım: dotnet AdminSetup.dll <BlazorApp1.csproj tam yolu>. Parola yalnız etkileşimli konsolda alınır; yazdırılmaz."); return; }
if (args.Length != 1 || !File.Exists(args[0]) || Console.IsInputRedirected) { Console.Error.WriteLine("Geçerli proje yolu ve etkileşimli konsol gerekiyor."); Environment.ExitCode = 1; return; }
string ReadPassword(string prompt)
{
    Console.Write(prompt); var input = new StringBuilder();
    while (true) { var key = Console.ReadKey(true); if (key.Key == ConsoleKey.Enter) break; if (key.Key == ConsoleKey.Backspace) { if (input.Length > 0) input.Length--; } else if (!char.IsControl(key.KeyChar)) input.Append(key.KeyChar); }
    Console.WriteLine(); return input.ToString();
}
Console.WriteLine("Bu araç yerel geliştirme yöneticisinin parolasını ayarlar/değiştirir. Kullanıcı adı: admin. Parola ekranda görünmez.");
var password = ReadPassword("Yeni parola (12–256 karakter): ");
if (password.Length is < 12 or > 256 || password != ReadPassword("Parolayı tekrar girin: ")) { Console.Error.WriteLine("Uzunluk uygun değil veya parolalar eşleşmedi. Değişiklik yapılmadı."); Environment.ExitCode = 1; return; }
var hash = new PasswordHasher<string>().HashPassword("admin", password);
var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
foreach(var value in new[]{"user-secrets","set","--project",Path.GetFullPath(args[0])}) start.ArgumentList.Add(value);
using var process = Process.Start(start) ?? throw new InvalidOperationException("User Secrets başlatılamadı.");
var output = process.StandardOutput.ReadToEndAsync(); var error = process.StandardError.ReadToEndAsync();
await process.StandardInput.WriteAsync(JsonSerializer.Serialize(new Dictionary<string,string>{{"Admin:PasswordHash",hash}})); process.StandardInput.Close();
await process.WaitForExitAsync(); await output; await error;
if(process.ExitCode != 0){Console.Error.WriteLine("Parola ayarı kaydedilemedi. Projenin UserSecretsId ayarını kontrol edin."); Environment.ExitCode = 1; return;}
Console.WriteLine("Parola özeti Windows kullanıcı profilindeki User Secrets deposuna kaydedildi. Düz parola kaydedilmedi. Web uygulamasını Development ortamında yeniden başlatıp /admin/login açın.");
