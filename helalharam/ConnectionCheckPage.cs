using System.Text.Json;
namespace helalharam;

public sealed class ConnectionCheckPage : ContentPage
{
#if ANDROID
    private const string BaseUrl = "http://10.0.2.2:5088/";
#else
    private const string BaseUrl = "http://localhost:5088/";
#endif
    private readonly Label status = new() { Text = "Kontrolü başlatmak için düğmeye dokunun.", FontSize = 17 };
    private readonly Button check = new() { Text = "Bağlantıyı kontrol et" };
    private CancellationTokenSource? pending;

    public ConnectionCheckPage()
    {
        Title = "Bağlantı kontrolü";
        var body = new VerticalStackLayout { Padding = 22, Spacing = 16 };
        body.Add(new Label { Text = "Yerel sunucu bağlantısı", FontSize = 26, FontAttributes = FontAttributes.Bold });
        body.Add(new Label { Text = "Bu kontrol yalnız bilgisayardaki ürün API'sine erişimi dener. Kamera, OFF, harita ve çeviri motorunu doğrulamaz. Kişisel metin veya konum göndermez." });
        body.Add(new Label { Text = "Hedef: " + BaseUrl });
        body.Add(status); body.Add(check);
        check.Clicked += async (_, _) => await CheckAsync();
        body.Add(new Label { Text = "Sunucuyu açmak için bilgisayarda depo klasöründen şu komutu çalıştırın:" });
        body.Add(new Label { Text = "dotnet run --project BlazorApp1/BlazorApp1.csproj --launch-profile http" });
        body.Add(new Label { Text = "Android emülatörü bilgisayara 10.0.2.2 üzerinden bağlanır. Bu adres gerçek telefon için uygun değildir. Önce bilgisayarda http://localhost:5088 adresinin açıldığını kontrol edin." });
        var close = new Button { Text = Ui.T("close") };
        close.Clicked += async (_, _) => await Navigation.PopModalAsync(); body.Add(close);
        Content = new ScrollView { Content = body };
    }

    private async Task CheckAsync()
    {
        if (pending is not null) return;
        using var attempt = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        pending = attempt; check.IsEnabled = false; status.Text = "Ürün API'sine bağlanılıyor…";
        try
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { BaseAddress = new Uri(BaseUrl) };
            using var response = await client.GetAsync("api/products", HttpCompletionOption.ResponseHeadersRead, attempt.Token);
            if (!response.IsSuccessStatusCode) { status.Text = $"Sunucu yanıt verdi fakat ürün API'si başarısız: HTTP {(int)response.StatusCode}. Bilgisayardaki sunucu çıktısını kontrol edin."; return; }
            if (response.Content.Headers.ContentType?.MediaType != "application/json") { status.Text = "Sunucu yanıt verdi ama beklenen ürün API yanıtı gelmedi. Adres ve portu kontrol edin."; return; }
            using var stream = await response.Content.ReadAsStreamAsync(attempt.Token);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: attempt.Token);
            attempt.Token.ThrowIfCancellationRequested();
            status.Text = json.RootElement.ValueKind == JsonValueKind.Array
                ? "Bağlantı başarılı. Yerel ürün API'si yanıt veriyor. Diğer servisler ayrıca denenmelidir."
                : "Sunucuya ulaşıldı fakat ürün yanıtının biçimi beklenenden farklı.";
        }
        catch (OperationCanceledException) { if (ReferenceEquals(pending, attempt)) status.Text = "10 saniye içinde yanıt alınamadı. Sunucunun açık ve 5088 portunda olduğunu kontrol edip yeniden deneyin."; }
        catch (HttpRequestException) { status.Text = "Sunucuya bağlanılamadı. Bilgisayarda web uygulamasını başlatın, ardından yeniden deneyin."; }
        catch (JsonException) { status.Text = "Sunucu yanıtı okunamadı. Web uygulamasının çıktısını kontrol edin."; }
        finally { if (ReferenceEquals(pending, attempt)) { pending = null; check.IsEnabled = true; } }
    }
    protected override void OnDisappearing()
    {
        pending?.Cancel(); pending = null; check.IsEnabled = true;
        base.OnDisappearing();
    }
}
