using System.Net.Http.Json;

namespace helalharam;

public sealed class DishDetailsPage : ContentPage
{
    private readonly HttpClient client;
    private readonly int id;
    private readonly VerticalStackLayout body = new() { Padding = 22, Spacing = 14 };
    private bool loading;
    public DishDetailsPage(int restaurantId, HttpClient httpClient)
    {
        id = restaurantId; client = httpClient; Title = "Yemek detayı";
        BackgroundColor = Color.FromArgb("#F4F7F2"); Content = new ScrollView { Content = body };
        ToolbarItems.Add(new ToolbarItem("Kapat", null, async () => await Navigation.PopModalAsync()));
    }
    protected override async void OnAppearing() { base.OnAppearing(); if (!loading) await LoadAsync(); }
    private void Text(string value, int size = 16) => body.Add(new Label { Text = value, FontSize = size, TextColor = Color.FromArgb("#20312C") });
    private async Task LoadAsync()
    {
        loading = true; body.Clear(); Text("Yemek yükleniyor…");
        try
        {
            using var response = await client.GetAsync($"api/restaurants/{id}");
            body.Clear();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) { Text("Yemek bulunamadı veya Türkiye kapsamı dışında."); return; }
            response.EnsureSuccessStatusCode();
            var detail = await response.Content.ReadFromJsonAsync<Detail>();
            if (detail is null) { Text("Yemek bilgisi alınamadı."); Retry(); return; }
            var r = detail.Restaurant;
            if (r.IsDemo) Text("DEMO · Şehir/şube dahil kurgu kayıt, gerçek işletme değildir.");
            Text(r.Dish, 26); Text(r.Name, 22);
            Text($"Ülke: {(r.CountryCode == "TR" ? "Türkiye" : "Belirtilmedi")}\nŞehir: {Known(r.City)}\nŞube: {Known(r.Branch)}");
            Text("İşletmeye danışın", 20); Text(detail.Note);
            Text("Kayıtlı içerik beyanı", 20); Text(string.IsNullOrWhiteSpace(r.ConfirmedIngredients) ? "Tam içerik listesi işletmeden alınmadı." : r.ConfirmedIngredients);
            Text("Olası içerikler — doğrulanmadı", 20); Text(Known(r.PossibleIngredients));
            Text($"Kaynak: {Known(r.InformationSource)}\nKayıt kontrolü: {r.LastCheckedUtc?.ToString("dd.MM.yyyy") ?? "Doğrulanmadı"}\nKayıt dili: {Known(r.Language)}");
            Text("İşletmeye sorulabilecekler", 20); Text(detail.Questions);
            Text("Alternatif yemek", 20); Text("İçeriği belirsiz bileşenin çıkarılmasını veya içerikleri açık başka bir yemek isteyebilirsin. Henüz kaynağı kontrol edilmiş alternatif yemek kaydı yok.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        { body.Clear(); Text("Yemek bilgisi alınamadı. Bilgisayarda Blazor projesinin çalıştığını kontrol edin."); Retry(); }
        finally { loading = false; }
    }
    private static string Known(string value) => string.IsNullOrWhiteSpace(value) ? "Belirtilmedi" : value;
    private void Retry() { var button = new Button { Text = "Tekrar dene" }; button.Clicked += async (_, _) => { if (!loading) await LoadAsync(); }; body.Add(button); }
    private sealed class Detail
    {
        public Restaurant Restaurant { get; set; } = new();
        public string Questions { get; set; } = "";
        public string Note { get; set; } = "";
    }
    private sealed class Restaurant
    {
        public string Name { get; set; } = "";
        public string Dish { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public string City { get; set; } = "";
        public string Branch { get; set; } = "";
        public string Language { get; set; } = "";
        public string ConfirmedIngredients { get; set; } = "";
        public string PossibleIngredients { get; set; } = "";
        public string InformationSource { get; set; } = "";
        public bool IsDemo { get; set; }
        public DateTime? LastCheckedUtc { get; set; }
    }
}
