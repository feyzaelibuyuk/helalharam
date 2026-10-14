using System.Net.Http.Json;

namespace helalharam;

public sealed class ProductDetailsPage : ContentPage
{
    private readonly int productId;
    private readonly HttpClient client;
    private readonly VerticalStackLayout body = new() { Padding = 22, Spacing = 14 };
    private bool loading;
    public ProductDetailsPage(int id, HttpClient httpClient)
    {
        productId = id; client = httpClient; Title = "Ürün detayı";
        BackgroundColor = Color.FromArgb("#F8F8F4");
        Content = new ScrollView { Content = body };
        ToolbarItems.Add(new ToolbarItem("Kapat", null, async () => await Navigation.PopModalAsync()));
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!loading) await LoadAsync();
    }
    private void Text(string value, int size = 16) => body.Add(new Label { Text = value, FontSize = size, TextColor = Color.FromArgb("#20312C") });
    private async Task LoadAsync()
    {
        loading = true; body.Clear(); Text("Ürün yükleniyor…");
        try
        {
            using var response = await client.GetAsync($"api/products/{productId}");
            body.Clear();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) { Text("Ürün bulunamadı."); return; }
            response.EnsureSuccessStatusCode();
            var detail = await response.Content.ReadFromJsonAsync<Detail>();
            if (detail is null) { Text("Ürün bilgisi alınamadı."); return; }
            var p = detail.Product;
            if (p.IsDemo) Text("DEMO • Kurgu kayıt; gerçek ürün veya uygunluk kararı değildir.");
            Text(p.Name, 26); Text($"{p.Brand} • {p.Category}"); Text(p.Assessment, 20); Text(p.Note);
            Text("İçindekiler", 20); Text(p.Ingredients);
            Text("Özgün etiket metni", 20); Text(string.IsNullOrEmpty(p.OriginalIngredients) ? "Kaydedilmedi" : p.OriginalIngredients);
            Text($"Dil: {Known(p.IngredientsLanguage)} • Satış ülkesi: {Known(p.CountryCode)}\nBarkod: {p.Barcode}");
            Text($"Kaynak: {p.Source}\nKontrol tarihi: {p.LastCheckedUtc?.ToString("dd.MM.yyyy") ?? "Henüz doğrulanmadı"}");
            Text(p.IsDemo ? "Demo — doğrulanmış değerlendirme değil." : p.IsVerified ? "Kayıtlı kaynak kontrol edilmiş." : "Kaynak kontrolü tamamlanmadı.");
            if (Uri.TryCreate(p.SourceUrl, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                var source = new Button { Text = "Kaynağı aç" };
                source.Clicked += async (_, _) =>
                {
                    try { await Launcher.Default.OpenAsync(uri); }
                    catch (Exception ex) when (ex is InvalidOperationException or FeatureNotSupportedException) { await DisplayAlertAsync("Kaynak", "Bağlantı açılamadı.", "Tamam"); }
                };
                body.Add(source);
            }
            Text(p.IsDemo ? "Örnek alternatifler" : "Uygun alternatifler", 22);
            Text(detail.AlternativeMessage);
            foreach (var alternative in detail.Alternatives)
            {
                Text(alternative.Product.Name, 19); Text(alternative.Reason);
                Text(alternative.Product.Ingredients); Text($"Kaynak: {alternative.Product.Source}");
                var id = alternative.Product.Id;
                var button = new Button { Text = "Alternatifin detayını incele", BackgroundColor = Color.FromArgb("#245342"), TextColor = Colors.White };
                button.Clicked += async (_, _) => await Navigation.PushAsync(new ProductDetailsPage(id, client));
                body.Add(button);
            }
            if (detail.Alternatives.Count == 0) Text("Henüz önerilecek alternatif yok.");
            Text("Şüpheli veya yetersiz bilgi, kesin haram hükmü değildir. Otomatik çeviri henüz bağlı değil.", 13);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            body.Clear(); Text("Ürün bilgisi alınamadı. Sunucu bağlantısını kontrol edin.");
            var retry = new Button { Text = "Tekrar dene" }; retry.Clicked += async (_, _) => await LoadAsync(); body.Add(retry);
        }
        finally { loading = false; }
    }
    private static string Known(string value) => value.Length == 0 ? "Belirtilmedi" : value;
    private sealed class Detail
    {
        public Product Product { get; set; } = new();
        public List<Alternative> Alternatives { get; set; } = [];
        public string AlternativeMessage { get; set; } = "";
    }
    private sealed class Alternative
    {
        public Product Product { get; set; } = new();
        public string Reason { get; set; } = "";
    }
    private sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Barcode { get; set; } = "";
        public string Category { get; set; } = "";
        public string Ingredients { get; set; } = "";
        public string Assessment { get; set; } = "";
        public string Note { get; set; } = "";
        public string Source { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public string IngredientsLanguage { get; set; } = "";
        public string OriginalIngredients { get; set; } = "";
        public string SourceUrl { get; set; } = "";
        public DateTime? LastCheckedUtc { get; set; }
        public bool IsVerified { get; set; }
        public bool IsDemo { get; set; }
    }
}
