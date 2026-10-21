using System.Net.Http.Json;
using HelalRehber.Shared;
namespace helalharam;
public sealed class FeedbackPage : ContentPage
{
    private readonly Picker category = new() { Title = Ui.T("feedbackType"), ItemsSource = FeedbackRules.Categories.Select(x => Ui.T(x.Id)).ToArray() };
    private readonly Editor message = new() { Placeholder = Ui.T("feedbackPrompt"), MaxLength = 2000, HeightRequest = 180 };
    private readonly Label status = new();
    private readonly Button send = new() { Text = Ui.T("feedbackSend") };
    private readonly FeedbackInput draft = new() { SubmissionId = Guid.NewGuid(),
#if ANDROID
        Client = "android",
#else
        Client = "other",
#endif
        Version = AppInfo.Current.VersionString + " (" + AppInfo.Current.BuildString + ")" };
    private readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(20),
#if ANDROID
        BaseAddress = new Uri("http://10.0.2.2:5088/")
#else
        BaseAddress = new Uri("http://localhost:5088/")
#endif
    };
    private bool attempted, busy, sent;
    public FeedbackPage()
    {
        FlowDirection = Ui.Direction;
        Title = Ui.T("feedback");
        var stack = new VerticalStackLayout { Padding = 22, Spacing = 14 };
        stack.Add(new Label { Text = Ui.T("feedbackHeading"), FontSize = 25 });
        stack.Add(new Label { Text = Ui.T("feedbackPrivacy") });
        stack.Add(category); stack.Add(message); stack.Add(status); stack.Add(send);
        var close = new Button { Text = Ui.T("close") }; close.Clicked += async (_, _) => await Navigation.PopModalAsync(); stack.Add(close);
        Content = new ScrollView { Content = stack };
        send.Clicked += async (_, _) => await SubmitAsync();
    }
    private async Task SubmitAsync()
    {
        if (busy || sent) return;
        if (!attempted)
        {
            draft.Category = category.SelectedIndex >= 0 ? FeedbackRules.Categories[category.SelectedIndex].Id : "";
            draft.Message = message.Text ?? "";
            var errors = FeedbackRules.Validate(draft); if(errors.Count > 0) { status.Text = string.Join("\n", errors); return; }
        }
        busy = true; category.IsEnabled = message.IsEnabled = send.IsEnabled = false;
        try
        {
            attempted = true;
            using var response = await client.PostAsJsonAsync("api/feedback", draft);
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) { status.Text = "Gönderim sınırına ulaşıldı. Bir dakika sonra aynı gönderimi tekrar deneyin."; return; }
            var result = await response.Content.ReadFromJsonAsync<Result>();
            if (response.IsSuccessStatusCode && result?.Receipt is { } receipt)
            { sent = true; status.Text = $"Geri bildiriminiz kaydedildi.\nGönderim: {receipt}"; send.Text = Ui.T("sent"); }
            else
            {
                status.Text = result?.Message ?? "Gönderim tamamlanamadı. Tekrar deneyin.";
                if(response.StatusCode == System.Net.HttpStatusCode.BadRequest) { attempted = false; draft.SubmissionId = Guid.NewGuid(); category.IsEnabled = message.IsEnabled = true; }
            }
        }
        catch(Exception ex) when(ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        { status.Text = "Sonuç alınamadı. Metniniz korunuyor; bağlantıyı kontrol edip aynı gönderimi tekrar deneyin."; }
        finally { busy = false; send.IsEnabled = !sent; if(!sent) send.Text = Ui.T("retry"); }
    }
    private sealed class Result { public string Message { get; set; } = ""; public Guid? Receipt { get; set; } }
}
