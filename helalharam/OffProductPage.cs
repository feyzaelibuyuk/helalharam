using System.Net.Http.Json;

namespace helalharam;

public sealed class OffProductPage : ContentPage
{
    private readonly HttpClient client;
    private readonly string barcode;
    private readonly VerticalStackLayout body = new() { Padding = 22, Spacing = 14 };
    private bool loading;
    public OffProductPage(string code, HttpClient httpClient)
    {
        barcode = code; client = httpClient; Title = "Gerçek ürün";
        BackgroundColor = Color.FromArgb("#F8F8F4");
        Content = new ScrollView { Content = body };
        ToolbarItems.Add(new ToolbarItem("Kapat", null, async () => await Navigation.PopModalAsync()));
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!loading) await LoadAsync();
    }
    private void Text(string text, int size = 16) => body.Add(new Label { Text = text, FontSize = size, TextColor = Color.FromArgb("#20312C") });
    private async Task LoadAsync()
    {
        loading = true; body.Clear(); Text("Open Food Facts sorgulanıyor…");
        try
        {
            using var response = await client.GetAsync($"api/off/products/{Uri.EscapeDataString(barcode)}");
            var result = await response.Content.ReadFromJsonAsync<Result>();
            body.Clear();
            if (result?.Product is not { } p)
            {
                Text(result?.Message ?? "Ürün bilgisi alınamadı.");
                if (response.StatusCode is System.Net.HttpStatusCode.BadGateway or System.Net.HttpStatusCode.TooManyRequests) Retry();
                return;
            }
            Text(p.Name, 26); Text(p.Brand); Text($"Barkod: {p.Barcode}");
            if (Uri.TryCreate(p.ImageUrl, UriKind.Absolute, out var image) && image.Scheme == "https" && image.Host == "images.openfoodfacts.org")
                body.Add(new Image { Source = ImageSource.FromUri(image), HeightRequest = 220, Aspect = Aspect.AspectFit });
            Text(p.Assessment, 20); Text(p.AssessmentNote);
            if (p.IngredientAssessment is { } evaluation)
            {
                Text(evaluation.Profile); Text($"Kural sürümü: {evaluation.RuleVersion}", 13); Text(evaluation.Coverage, 13);
                foreach (var finding in evaluation.Findings)
                {
                    Text(finding.Ingredient, 19); Text($"Etikette eşleşen: {finding.MatchedText}"); Text(finding.Reason);
                    var uri = new Uri(finding.SourceUrl);
                    var reference = new Button { Text = finding.SourceTitle };
                    reference.Clicked += async (_, _) =>
                    {
                        try { await Launcher.Default.OpenAsync(uri); }
                        catch (Exception ex) when (ex is InvalidOperationException or FeatureNotSupportedException) { await DisplayAlertAsync("Kaynak", "Bağlantı açılamadı.", "Tamam"); }
                    };
                    body.Add(reference);
                }
            }
            Text("Özgün içindekiler", 20); Text(string.IsNullOrWhiteSpace(p.Ingredients) ? "Kaynakta içerik bulunmuyor." : p.Ingredients);
            Text($"Dil: {p.IngredientsLanguage}\nSatış ülke etiketleri: {Join(p.Countries)}\nKategori etiketleri: {Join(p.Categories)}");
            Text($"Kaynak etiketleri: {Join(p.Labels)}\nHelal etiketi, kontrol edilmiş sertifika anlamına gelmez.");
            Text($"Kaynak güncellemesi: {p.SourceUpdatedUtc?.ToString("dd.MM.yyyy") ?? "Belirtilmedi"}\nVeri alınma tarihi: {p.FetchedAtUtc:dd.MM.yyyy}{(result.FromCache ? " (önbellekten)" : "")}\nHelal uygunluk kontrolü: Henüz doğrulanmadı.");
            Text("Birden çok satış ülkesi, her ülkedeki formülün ayrı doğrulandığı anlamına gelmez. Çeviri henüz bağlı değil.");
            Text("Veri: Open Food Facts · ODbL. Ambalaj fotoğrafları: CC BY-SA.", 13);
            if (Uri.TryCreate(p.SourceUrl, UriKind.Absolute, out var source) && source.Scheme == "https" && source.Host == "world.openfoodfacts.org")
            {
                var button = new Button { Text = "Open Food Facts kaynağını aç" };
                button.Clicked += async (_, _) =>
                {
                    try { await Launcher.Default.OpenAsync(source); }
                    catch (Exception ex) when (ex is InvalidOperationException or FeatureNotSupportedException) { await DisplayAlertAsync("Kaynak", "Bağlantı açılamadı.", "Tamam"); }
                };
                body.Add(button);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        { body.Clear(); Text("Sunucuya ulaşılamadı. Blazor/API projesinin çalıştığını kontrol edin."); Retry(); }
        finally { loading = false; }
    }
    private void Retry() { var retry = new Button { Text = "Tekrar dene" }; retry.Clicked += async (_, _) => await LoadAsync(); body.Add(retry); }
    private static string Join(string[] tags) => tags.Length == 0 ? "Belirtilmedi" : string.Join(", ", tags.Select(t => t.Contains(':') ? t[(t.IndexOf(':') + 1)..] : t));
    private sealed class Result
    {
        public Product? Product { get; set; }
        public string Message { get; set; } = "";
        public bool FromCache { get; set; }
    }
    private sealed class Evaluation
    {
        public string Profile { get; set; } = "";
        public string RuleVersion { get; set; } = "";
        public string Coverage { get; set; } = "";
        public List<Finding> Findings { get; set; } = [];
    }
    private sealed class Finding
    {
        public string Ingredient { get; set; } = "";
        public string MatchedText { get; set; } = "";
        public string Reason { get; set; } = "";
        public string SourceTitle { get; set; } = "";
        public string SourceUrl { get; set; } = "";
    }
    private sealed class Product
    {
        public string Barcode { get; set; } = "";
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Ingredients { get; set; } = "";
        public string IngredientsLanguage { get; set; } = "";
        public string[] Countries { get; set; } = [];
        public string[] Categories { get; set; } = [];
        public string[] Labels { get; set; } = [];
        public string ImageUrl { get; set; } = "";
        public string SourceUrl { get; set; } = "";
        public DateTimeOffset FetchedAtUtc { get; set; }
        public DateTimeOffset? SourceUpdatedUtc { get; set; }
        public string Assessment { get; set; } = "";
        public string AssessmentNote { get; set; } = "";
        public Evaluation? IngredientAssessment { get; set; }
    }
}
