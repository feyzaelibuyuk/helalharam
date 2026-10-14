using System.Net.Http.Json;
using Plugin.Maui.OCR;

namespace helalharam;

public sealed class OcrPage : ContentPage
{
    private readonly Image preview = new() { HeightRequest = 180, Aspect = Aspect.AspectFit, IsVisible = false };
    private readonly Editor text = new() { Placeholder = "Fotoğraftan okunan metni burada kontrol edip düzeltin.", HeightRequest = 210, MaxLength = 12000 };
    private readonly Picker mode = new() { Title = "Fotoğrafın türü", ItemsSource = new[] { "Paket içindekileri", "Restoran menüsü" }, SelectedIndex = 0 };
    private readonly Picker language = new() { Title = "Metnin gerçek dili", ItemsSource = new[] { "Türkçe", "İngilizce", "Fransızca", "Almanca", "Diğer / bilinmiyor" }, SelectedIndex = 0 };
    private readonly CheckBox reviewed = new();
    private readonly Label status = new() { Text = "Net, düz ve yalnız ilgili metni içeren bir fotoğraf seçin." };
    private readonly VerticalStackLayout results = new() { Spacing = 12 };
    private readonly Button capture = new() { Text = "Fotoğraf çek" };
    private readonly Button pick = new() { Text = "Galeriden fotoğraf seç" };
    private readonly Button analyze = new() { Text = "Kontrol ettiğim metni kullan" };
    private readonly HttpClient client = new()
    {
#if ANDROID
        BaseAddress = new Uri("http://10.0.2.2:5088/")
#else
        BaseAddress = new Uri("http://localhost:5088/")
#endif
    };
    private CancellationTokenSource lifetime = new();
    private bool busy;
    private bool visible;
    private string Language => new[] { "tr", "en", "fr", "de", "other" }[Math.Max(0, language.SelectedIndex)];

    public OcrPage(int selectedMode = 0)
    {
        Title = "Fotoğraftan metin oku";
        BackgroundColor = Color.FromArgb("#F4F7F2");
        mode.SelectedIndex = selectedMode;
        var stack = new VerticalStackLayout { Padding = 22, Spacing = 14 };
        stack.Add(new Label { Text = "Etiket / menü fotoğrafı", FontSize = 25, TextColor = Color.FromArgb("#166456") });
        stack.Add(new Label { Text = "Latin harfli metinler cihazda okunur. OCR hata yapabilir; özellikle katkı kodlarını ve eksik satırları kontrol edin. Çeviri henüz eklenmedi." });
        stack.Add(mode); stack.Add(capture); stack.Add(pick); stack.Add(status); stack.Add(preview); stack.Add(text);
        stack.Add(new Label { Text = "Metnin gerçek dili (değerlendirme dili):" }); stack.Add(language);
        var confirm = new HorizontalStackLayout(); confirm.Add(reviewed);
        confirm.Add(new Label { Text = "Metni ve dilini kontrol ettim.", VerticalOptions = LayoutOptions.Center });
        stack.Add(confirm); stack.Add(analyze); stack.Add(results);
        stack.Add(new Label { Text = "Fotoğraf sunucuya gönderilmez. Paket analizi düğmesine basınca düzelttiğiniz metin yerel C# API'ye gönderilir. Menü soruları cihazda hazırlanır." });
        Content = new ScrollView { Content = stack };
        ToolbarItems.Add(new ToolbarItem("Kapat", null, async () => await Navigation.PopModalAsync()));
        capture.Clicked += async (_, _) => await ReadPhotoAsync(true);
        pick.Clicked += async (_, _) => await ReadPhotoAsync(false);
        analyze.Clicked += async (_, _) => await AnalyzeAsync();
        text.TextChanged += (_, _) => ResetReview();
        language.SelectedIndexChanged += (_, _) => ResetReview();
        mode.SelectedIndexChanged += (_, _) => ResetReview();
    }
    private void ResetReview() { reviewed.IsChecked = false; results.Clear(); }
    protected override void OnAppearing()
    {
        base.OnAppearing(); visible = true;
        if (lifetime.IsCancellationRequested) { lifetime.Dispose(); lifetime = new(); }
    }
    protected override void OnDisappearing()
    {
        visible = false; lifetime.Cancel(); base.OnDisappearing();
    }
    private void SetBusy(bool value)
    {
        busy = value; capture.IsEnabled = pick.IsEnabled = analyze.IsEnabled = !value;
        text.IsEnabled = mode.IsEnabled = language.IsEnabled = reviewed.IsEnabled = !value;
    }
    private async Task ReadPhotoAsync(bool useCamera)
    {
        if (busy) return;
        var token = lifetime.Token;
        SetBusy(true); ResetReview();
        try
        {
            if (useCamera && !MediaPicker.Default.IsCaptureSupported)
            { status.Text = "Bu cihazda fotoğraf çekilemiyor. Galeriden seçebilir veya metni yazabilirsiniz."; return; }
            FileResult? photo;
            if (useCamera)
                photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Etiket veya menü" });
            else
            {
                var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "Etiket veya menü seç", SelectionLimit = 1 });
                photo = photos.FirstOrDefault();
            }
            if (photo is null) { status.Text = "Fotoğraf seçimi iptal edildi. Mevcut metin korunuyor."; return; }
            // Android may hide the page while its photo picker is active; resume this operation only after returning.
            if (!visible) return;
            token = lifetime.Token;
            status.Text = "Fotoğraf cihazda okunuyor…";
            var data = await OcrImage.ReadAsync(photo, token);
            text.Text = "";
            preview.Source = ImageSource.FromStream(() => new MemoryStream(data, writable: false));
            preview.IsVisible = true;
            await OcrPlugin.Default.InitAsync(token);
            var result = await OcrPlugin.Default.RecognizeTextAsync(data, false, token);
            token.ThrowIfCancellationRequested();
            if (!result.Success || string.IsNullOrWhiteSpace(result.AllText))
            { status.Text = "Metin okunamadı. Daha net bir Latin harfli fotoğraf deneyin veya metni yazın."; return; }
            if (result.AllText.Length > 12000)
            { status.Text = "Okunan metin çok uzun. Yalnız içindekiler veya menü bölümünün fotoğrafını seçin."; return; }
            text.Text = result.AllText;
            status.Text = "Metin okundu. Fotoğrafla karşılaştırın, hataları düzeltin ve gerçek dilini seçin.";
        }
        catch (OperationCanceledException) { }
        catch (PermissionException) { status.Text = "Fotoğraf izni verilmedi. Cihaz ayarlarından izin verebilir veya metni elle girebilirsiniz."; }
        catch (InvalidDataException ex) { status.Text = ex.Message; }
        catch (Exception) { status.Text = "Fotoğraf okunamadı. JPG/PNG deneyin; daha net bir fotoğraf seçin veya metni yazın."; }
        finally { SetBusy(false); }
    }
    private async Task AnalyzeAsync()
    {
        if (busy) return;
        results.Clear();
        if (string.IsNullOrWhiteSpace(text.Text)) { status.Text = "Önce fotoğraf okutun veya metin girin."; return; }
        if (!reviewed.IsChecked) { status.Text = "Metni ve gerçek dilini kontrol ederek kutuyu işaretleyin."; return; }
        if (mode.SelectedIndex == 1)
        { results.Add(new Label { Text = MenuQuestions.Prepare(text.Text, Language) }); status.Text = "İşletmeye sorulabilecek sorular hazırlandı."; return; }
        var token = lifetime.Token;
        SetBusy(true);
        try
        {
            using var response = await client.PostAsJsonAsync("api/ingredients/assess", new { Ingredients = text.Text, Language }, token);
            response.EnsureSuccessStatusCode();
            var assessment = await response.Content.ReadFromJsonAsync<Assessment>(cancellationToken: token);
            token.ThrowIfCancellationRequested();
            if (assessment is null) throw new InvalidDataException();
            status.Text = assessment.Status;
            results.Add(new Label { Text = assessment.Summary });
            results.Add(new Label { Text = assessment.Coverage });
            foreach (var finding in assessment.Findings ?? [])
                results.Add(new Label { Text = $"{finding.Ingredient} · {finding.MatchedText}\n{finding.Reason}\nKaynak: {finding.SourceTitle}\n{finding.SourceUrl}" });
            results.Add(new Label { Text = $"{assessment.Profile}\n{assessment.RuleVersion}\nBu metin analizi sertifika doğrulaması değildir. Şüpheli paket içeriği için uygunluk doğrulanamaz." });
        }
        catch (OperationCanceledException) { if (visible) status.Text = "İstek tamamlanamadı. Tekrar deneyin."; }
        catch (Exception) { status.Text = "İçerik analizi alınamadı. Bilgisayarda Blazor projesinin 5088 portunda çalıştığını kontrol edin."; }
        finally { SetBusy(false); }
    }
    private sealed class Assessment
    {
        public string Status { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Coverage { get; set; } = "";
        public string Profile { get; set; } = "";
        public string RuleVersion { get; set; } = "";
        public List<Finding>? Findings { get; set; }
    }
    private sealed class Finding
    {
        public string Ingredient { get; set; } = "";
        public string MatchedText { get; set; } = "";
        public string Reason { get; set; } = "";
        public string SourceTitle { get; set; } = "";
        public string SourceUrl { get; set; } = "";
    }
}
