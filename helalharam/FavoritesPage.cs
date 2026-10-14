using HelalRehber.Shared;
using System.Text.Json;

namespace helalharam;

internal static class DeviceFavorites
{
    private const string StorageKey = "favorites-v1";
    public static List<FavoriteItem> Read() => FavoriteList.Read(Preferences.Default.Get(StorageKey, "[]"));
    public static bool Toggle(FavoriteItem item)
    {
        var items = Read();
        if (items.RemoveAll(x => x.Key == item.Key) == 0 && !FavoriteList.Add(items, item)) return false;
        Preferences.Default.Set(StorageKey, JsonSerializer.Serialize(items)); return true;
    }
    public static void Remove(FavoriteItem item)
    {
        var items = Read(); items.RemoveAll(x => x.Key == item.Key);
        Preferences.Default.Set(StorageKey, JsonSerializer.Serialize(items));
    }
    public static void Clear() => Preferences.Default.Remove(StorageKey);
    public static Button Button(ContentPage page, FavoriteItem item)
    {
        var button = new Button { Text = Caption(), BackgroundColor = Color.FromArgb("#245342"), TextColor = Colors.White };
        string Caption() => Read().Any(x => x.Key == item.Key) ? "Favorilerden çıkar" : "Favorilere ekle";
        button.Clicked += async (_, _) =>
        {
            try
            {
                if (!Toggle(item)) await page.DisplayAlertAsync("Favoriler", "Favori alanı doldu. Bir ürünü çıkarıp tekrar deneyin.", "Tamam");
                button.Text = Caption();
            }
            catch (Exception) { await page.DisplayAlertAsync("Favoriler", "Favoriler kaydedilemedi. Tekrar deneyin.", "Tamam"); }
        };
        return button;
    }
}
public sealed class FavoritesPage : ContentPage
{
    private readonly VerticalStackLayout body = new() { Padding = 22, Spacing = 14 };
    private readonly HttpClient client = new()
    {
#if ANDROID
        BaseAddress = new Uri("http://10.0.2.2:5088/")
#else
        BaseAddress = new Uri("http://localhost:5088/")
#endif
    };
    public FavoritesPage()
    {
        Title = "Favoriler"; BackgroundColor = Color.FromArgb("#F4F7F2");
        Content = new ScrollView { Content = body };
    }
    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }
    private void Refresh()
    {
        body.Clear();
        body.Add(new Label { Text = "Favorilerim", FontSize = 26 });
        body.Add(new Label { Text = "Bu cihazda en fazla 20 ürün. Web veya başka cihazla eşitleme yok. Favori, uygunluk onayı değildir; ürün açılınca güncel bilgi alınır." });
        var items = DeviceFavorites.Read();
        if (items.Count == 0) body.Add(new Label { Text = "Henüz favori yok. Katalogdaki bir ürünün detayından ekleyebilirsin." });
        foreach (var item in items)
        {
            body.Add(new Label { Text = $"{item.Name}\n{item.Brand}\n{(item.Kind == "off" ? "Open Food Facts" : "Yerel katalog")} · {item.Id}", FontSize = 18 });
            var open = new Button { Text = "Güncel ürün bilgisini aç" };
            open.Clicked += async (_, _) => await Navigation.PushModalAsync(new NavigationPage(item.Kind == "off"
                ? new OffProductPage(item.Id, client) : new ProductDetailsPage(int.Parse(item.Id), client)));
            body.Add(open);
            var remove = new Button { Text = "Favoriden çıkar" };
            remove.Clicked += (_, _) => { DeviceFavorites.Remove(item); Refresh(); }; body.Add(remove);
        }
        if (items.Count > 0)
        {
            var clear = new Button { Text = "Tüm favorileri sil" };
            clear.Clicked += async (_, _) =>
            {
                if (await DisplayAlertAsync("Favoriler", "Bu cihazdaki tüm favoriler silinsin mi?", "Sil", "Vazgeç")) { DeviceFavorites.Clear(); Refresh(); }
            };
            body.Add(clear);
        }
    }
}
