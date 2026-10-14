using System.Net.Http.Json;

namespace helalharam;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private bool _showRestaurants;
    private bool _hasLoaded;

    public MainPage()
    {
        InitializeComponent();
#if ANDROID
        _httpClient = new HttpClient { BaseAddress = new Uri("http://10.0.2.2:5088/") };
#else
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5088/") };
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_hasLoaded)
        {
            _hasLoaded = true;
            await LoadResultsAsync();
        }
    }

    private async void OnSearchClicked(object? sender, EventArgs e) => await LoadResultsAsync();
    private async void OnSearchCompleted(object? sender, EventArgs e) => await LoadResultsAsync();

    private async void OnProductsClicked(object? sender, EventArgs e)
    {
        _showRestaurants = false; CountryEntry.IsVisible = true; CityEntry.IsVisible = false;
        ProductsButton.BackgroundColor = Color.FromArgb("#245342");
        ProductsButton.TextColor = Colors.White;
        RestaurantsButton.BackgroundColor = Color.FromArgb("#EEEFEB");
        RestaurantsButton.TextColor = Color.FromArgb("#6F7D74");
        await LoadResultsAsync();
    }

    private async void OnRestaurantsClicked(object? sender, EventArgs e)
    {
        _showRestaurants = true; CountryEntry.IsVisible = false; CityEntry.IsVisible = true;
        RestaurantsButton.BackgroundColor = Color.FromArgb("#245342");
        RestaurantsButton.TextColor = Colors.White;
        ProductsButton.BackgroundColor = Color.FromArgb("#EEEFEB");
        ProductsButton.TextColor = Color.FromArgb("#6F7D74");
        await LoadResultsAsync();
    }

    private async Task LoadResultsAsync()
    {
        var country = CountryEntry.Text?.Trim() ?? "";
        if (!_showRestaurants && country.Length > 0 &&
            (country.Length != 2 || !country.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z')))
        {
            ConnectionStatus.Text = "Ülke için iki harfli kod kullanın (TR, DE gibi).";
            ResultsList.ItemsSource = Array.Empty<CatalogCard>();
            ResultCount.Text = "Ülke filtresini düzeltin.";
            return;
        }
        ConnectionStatus.Text = "Bilgiler yükleniyor…";
        var query = Uri.EscapeDataString(SearchEntry.Text?.Trim() ?? "");
        try
        {
            if (_showRestaurants)
            {
                var records = await _httpClient.GetFromJsonAsync<List<RestaurantDto>>($"api/restaurants?q={query}&city={Uri.EscapeDataString(CityEntry.Text?.Trim() ?? "")}") ?? [];
                ResultsList.ItemsSource = records.Select(r => new CatalogCard(r.Dish, r.Name, r.PossibleIngredients,
                    $"Sorulabilecekler: {r.Questions}", $"{(r.IsDemo ? "DEMO · Kurgu işletme" : "Kaynak bilgisi doğrulanmalı")} · {r.City} · {r.Branch}\nBilgi kaynağı: {r.InformationSource}", "İşletmeye danışın", RestaurantId: r.Id)).ToList();
                ResultCount.Text = $"{records.Count} yemek kaydı · İşletme bilgileri kaynağıyla gösterilir";
            }
            else
            {
                var records = await _httpClient.GetFromJsonAsync<List<ProductDto>>($"api/products?q={query}&country={Uri.EscapeDataString(CountryEntry.Text?.Trim() ?? "")}") ?? [];
                ResultsList.ItemsSource = records.Select(p => new CatalogCard(p.Name, p.Brand, p.Ingredients,
                    p.Note, $"Bilgi kaynağı: {p.Source}", p.Assessment, p.Id)).ToList();
                ResultCount.Text = records.Count == 0 ? "Ürün bulunamadı. Ülke filtresini kontrol edin veya içindekiler metnini Menü ekranına girin. Kamera barkod taraması kullanılabilir; etiket fotoğrafını OCR ile okuyabilirsiniz." : $"{records.Count} ürün kaydı · Detay ve alternatifleri inceleyebilirsiniz";
            }
            ConnectionStatus.Text = "";
        }
        catch (HttpRequestException)
        {
            ResultsList.ItemsSource = Array.Empty<CatalogCard>();
            ResultCount.Text = "Kayıtlar henüz alınamadı";
            ConnectionStatus.Text = "Bilgisayarda Blazor web projesi çalışırken tekrar dene.";
        }
        catch (TaskCanceledException)
        {
            ConnectionStatus.Text = "Sunucu yanıt vermedi. Web projesinin çalıştığını kontrol et.";
        }
    }

    private async void OnOcrClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new NavigationPage(new OcrPage()));

    private async void OnScanBarcodeClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new NavigationPage(new BarcodeScannerPage(_httpClient)));

    private async void OnOffBarcodeClicked(object? sender, EventArgs e)
    {
        var code = OffBarcodeEntry.Text?.Trim() ?? "";
        if (!ProductBarcode.IsValid(code))
        {
            await DisplayAlertAsync("Barkod", "8, 12, 13 veya 14 rakamlı barkod girin.", "Tamam");
            return;
        }
        await Navigation.PushModalAsync(new NavigationPage(new OffProductPage(code, _httpClient)));
    }

    private async void OnDishDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: CatalogCard card } && card.RestaurantId > 0)
            await Navigation.PushModalAsync(new NavigationPage(new DishDetailsPage(card.RestaurantId, _httpClient)));
    }
    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: CatalogCard card } && card.ProductId > 0)
            await Navigation.PushModalAsync(new NavigationPage(new ProductDetailsPage(card.ProductId, _httpClient)));
    }
    private sealed record CatalogCard(string Title, string Subtitle, string Details, string Note, string Source, string Status, int ProductId = 0, int RestaurantId = 0)
    { public bool IsProduct => ProductId > 0; public bool IsRestaurant => RestaurantId > 0; }
    private sealed class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Ingredients { get; set; } = "";
        public string Assessment { get; set; } = "Bilgi bekleniyor";
        public string Note { get; set; } = "";
        public string Source { get; set; } = "";
    }
    private sealed class RestaurantDto
    {
        public int Id { get; set; }
        public string City { get; set; } = "";
        public string Branch { get; set; } = "";
        public bool IsDemo { get; set; }
        public string Name { get; set; } = "";
        public string Dish { get; set; } = "";
        public string PossibleIngredients { get; set; } = "";
        public string Questions { get; set; } = "";
        public string InformationSource { get; set; } = "";
    }
}
