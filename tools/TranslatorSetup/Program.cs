using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
if(args.Contains("--help")){Console.WriteLine("Kullanım: dotnet TranslatorSetup.dll <BlazorApp1.csproj yolu>. Anahtarı yalnız yerel etkileşimli konsola girin; sohbet veya komut satırına yazmayın.");return;}
if(args.Length!=1||!File.Exists(args[0])||Console.IsInputRedirected){Console.Error.WriteLine("Geçerli proje yolu ve etkileşimli konsol gerekiyor.");Environment.ExitCode=1;return;}
Console.WriteLine("Azure Translator anahtarını yerel User Secrets deposuna kaydeder. Anahtar şifrelenmiş kasa değildir; kaynak koduna veya APK'ya yazılmaz. F0 ücretsiz kaynağını seçin; ücretli plan bu araçla yönetilmez.");
Console.Write("Bölge (Azure Keys and Endpoint ekranındaki location; global ise boş): ");var region=Console.ReadLine()?.Trim()??"";
if(region.Length>0&&!Regex.IsMatch(region,"^[a-z0-9-]{1,40}$")){Console.Error.WriteLine("Bölge biçimi geçersiz. Değişiklik yapılmadı.");Environment.ExitCode=1;return;}
Console.Write("API anahtarını yapıştırın (karakterler gizlidir): ");var input=new StringBuilder();
while(true){var key=Console.ReadKey(true);if(key.Key==ConsoleKey.Enter)break;if(key.Key==ConsoleKey.Backspace){if(input.Length>0)input.Length--;}else if(!char.IsControl(key.KeyChar))input.Append(key.KeyChar);}
Console.WriteLine();var secret=input.ToString().Trim();
if(secret.Length is < 16 or > 256||secret.Any(char.IsWhiteSpace)){Console.Error.WriteLine("Anahtar biçimi geçersiz. Değişiklik yapılmadı.");Environment.ExitCode=1;return;}
var start=new ProcessStartInfo("dotnet"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var value in new[]{"user-secrets","set","--project",Path.GetFullPath(args[0])})start.ArgumentList.Add(value);
using var process=Process.Start(start)!;var output=process.StandardOutput.ReadToEndAsync();var error=process.StandardError.ReadToEndAsync();
await process.StandardInput.WriteAsync(JsonSerializer.Serialize(new Dictionary<string,string>{{"Translation:Enabled","true"},{"Translation:Key",secret},{"Translation:Region",region},{"Translation:DailyCharacterLimit","50000"}}));process.StandardInput.Close();await process.WaitForExitAsync();await output;await error;
if(process.ExitCode!=0){Console.Error.WriteLine("Ayar kaydedilemedi. Proje yolunu ve UserSecretsId ayarını kontrol edin.");Environment.ExitCode=1;return;}
Console.WriteLine("Çeviri ayarları User Secrets'a kaydedildi. Anahtar ekrana yazdırılmadı. Development web/API uygulamasını yeniden başlatın. F0 kotasını Azure portalından izleyin; uygulama sınırı kalıcı faturalama koruması değildir.");
