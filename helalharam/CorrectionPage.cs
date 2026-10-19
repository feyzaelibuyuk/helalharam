using System.Net.Http.Json;
using HelalRehber.Shared;

namespace helalharam;

public sealed class CorrectionPage : ContentPage
{
    private readonly HttpClient client;
    private readonly CorrectionInput draft;
    private readonly Picker field = new() { Title = "Yanlış / eksik alan" };
    private readonly Editor suggestion = new() { Placeholder = "Neyi düzeltmek istediğinizi 10–2.000 karakterle açıklayın.", MaxLength = 2000, HeightRequest = 180 };
    private readonly Entry source = new() { Placeholder = "Kaynak bağlantısı (isteğe bağlı, https)", MaxLength = 1000, Keyboard = Keyboard.Url };
    private readonly Label status = new();
    private readonly Button send = new() { Text = "İncelemeye gönder" };
    private bool busy;
    private bool sent;
    private bool attempted;
    public CorrectionPage(string kind, string id, string title, HttpClient httpClient)
    {
        client = httpClient; draft = new() { Kind = kind, TargetId = id, SubmissionId = Guid.NewGuid() };
        Title = "Bilgi düzeltme"; BackgroundColor = Color.FromArgb("#F4F7F2");
        var body = new VerticalStackLayout { Padding = 22, Spacing = 14 };
        body.Add(new Label { Text = "Düzeltme öner", FontSize = 25 }); body.Add(new Label { Text = title });
        body.Add(new Label { Text = "Öneri inceleme bekler; katalog bilgisi doğrudan değişmez. Ad, telefon, e-posta veya başka kişisel bilgi yazmayın." });
        field.ItemsSource = CorrectionRules.Fields(kind).Select(x => x.Label).ToArray();
        body.Add(field); body.Add(suggestion); body.Add(source); body.Add(status); body.Add(send);
        body.Add(new Label { Text = "Kaynak bağlantısı otomatik doğrulanmaz. Gönderim sonunda takip numarasını saklayabilirsiniz. Otomatik bildirim gönderilmez." });
        Content = new ScrollView { Content = body };
        ToolbarItems.Add(new ToolbarItem("Kapat", null, async () => await Navigation.PopAsync()));
        send.Clicked += async (_, _) => await SubmitAsync();
    }
    private async Task SubmitAsync()
    {
        if (busy || sent) return;
        // After a timeout, retry exactly the same request id and content until the server confirms it.
        if (!attempted)
        {
            draft.Field = field.SelectedIndex >= 0 ? CorrectionRules.Fields(draft.Kind)[field.SelectedIndex].Id : "";
            draft.Suggestion = suggestion.Text ?? ""; draft.SourceUrl = source.Text ?? "";
            var errors = CorrectionRules.Validate(draft);
            if (errors.Count > 0) { status.Text = string.Join("\n", errors); return; }
        }
        busy = true; send.IsEnabled = false; field.IsEnabled = suggestion.IsEnabled = source.IsEnabled = false;
        try
        {
            attempted = true;
            using var response = await client.PostAsJsonAsync("api/corrections", draft);
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            { status.Text = "Gönderim sınırına ulaşıldı. Bir dakika sonra aynı öneriyle tekrar deneyin."; return; }
            var result = await response.Content.ReadFromJsonAsync<Result>();
            if (response.IsSuccessStatusCode && result?.Receipt is { } receipt)
            {
                sent = true; status.Text = $"{CorrectionRules.StatusLabel(receipt.Status)}\nGönderim: {receipt.Id}\nKatalog bilgisi değiştirilmedi."; send.Text = "Gönderildi";
            }
            else
            {
                if (response.StatusCode is System.Net.HttpStatusCode.BadRequest or System.Net.HttpStatusCode.NotFound)
                { attempted = false; draft.SubmissionId = Guid.NewGuid(); field.IsEnabled = suggestion.IsEnabled = source.IsEnabled = true; }
                status.Text = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ? "Gönderim sınırına ulaşıldı. Bir dakika sonra aynı öneriyle tekrar deneyin." : result?.Message ?? "Öneri kaydedilemedi. Tekrar deneyin.";
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        { status.Text = "Sonuç alınamadı. Metniniz korunuyor; bağlantıyı kontrol ederek aynı gönderimi tekrar deneyin."; }
        finally { busy = false; send.IsEnabled = !sent; if (!sent) send.Text = "Tekrar gönder"; }
    }
    private sealed class Result
    {
        public string Message { get; set; } = "";
        public CorrectionReceipt? Receipt { get; set; }
    }
}
