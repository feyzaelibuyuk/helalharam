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
        _showRestaurants = false;
        ProductsButton.BackgroundColor = Color.FromArgb("#245342");
        ProductsButton.TextColor = Colors.White;
        RestaurantsButton.BackgroundColor = Color.FromArgb("#EEEFEB");
        RestaurantsButton.TextColor = Color.FromArgb("#6F7D74");
        await LoadResultsAsync();
    }

    private async void OnRestaurantsClicked(object? sender, EventArgs e)
    {
        _showRestaurants = true;
        RestaurantsButton.BackgroundColor = Color.FromArgb("#245342");
        RestaurantsButton.TextColor = Colors.White;
        ProductsButton.BackgroundColor = Color.FromArgb("#EEEFEB");
        ProductsButton.TextColor = Color.FromArgb("#6F7D74");
        await LoadResultsAsync();
    }

    private async Task LoadResultsAsync()
    {
        ConnectionStatus.Text = "Bilgiler yükleniyor…";
        var query = Uri.EscapeDataString(SearchEntry.Text?.Trim() ?? "");
        try
        {
            if (_showRestaurants)
            {
                var records = await _httpClient.GetFromJsonAsync<List<RestaurantDto>>($"api/restaurants?q={query}") ?? [];
                ResultsList.ItemsSource = records.Select(r => new CatalogCard(r.Dish, r.Name, r.PossibleIngredients,
                    $"Sorulabilecekler: {r.Questions}", $"Bilgi kaynağı: {r.InformationSource}", "İşletmeye danışın")).ToList();
                ResultCount.Text = $"{records.Count} yemek kaydı · İşletme bilgileri kaynağıyla gösterilir";
            }
            else
            {
                var records = await _httpClient.GetFromJsonAsync<List<ProductDto>>($"api/products?q={query}") ?? [];
                ResultsList.ItemsSource = records.Select(p => new CatalogCard(p.Name, p.Brand, p.Ingredients,
                    p.Note, $"Bilgi kaynağı: {p.Source}", p.Assessment)).ToList();
                ResultCount.Text = $"{records.Count} ürün kaydı · Bilgiler kaynak durumuyla gösterilir";
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

    private sealed record CatalogCard(string Title, string Subtitle, string Details, string Note, string Source, string Status);
    private sealed class ProductDto
    {
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Ingredients { get; set; } = "";
        public string Assessment { get; set; } = "Bilgi bekleniyor";
        public string Note { get; set; } = "";
        public string Source { get; set; } = "";
    }
    private sealed class RestaurantDto
    {
        public string Name { get; set; } = "";
        public string Dish { get; set; } = "";
        public string PossibleIngredients { get; set; } = "";
        public string Questions { get; set; } = "";
        public string InformationSource { get; set; } = "";
    }
}
