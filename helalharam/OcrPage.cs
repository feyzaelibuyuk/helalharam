using System.Net.Http.Json;

namespace helalharam;

public sealed class OcrPage : ContentPage
{
    private readonly Image preview = new() { HeightRequest = 180, Aspect = Aspect.AspectFit, IsVisible = false };
    private readonly Editor text = new() { Placeholder = Ui.T("ocrEditor"), HeightRequest = 210, MaxLength = 12000 };
    private readonly Picker mode = new() { Title = Ui.T("photoType"), ItemsSource = new[] { Ui.T("packageIngredients"), Ui.T("restaurantMenu") }, SelectedIndex = 0 };
    private readonly Picker script = new() { Title = Ui.T("ocrModel"), ItemsSource = ScriptOcr.Scripts.Select(x => Ui.T("script_" + x.Code)).ToArray(), SelectedIndex = 0 };
    private readonly Picker language = new() { Title = Ui.T("textLanguage"), ItemsSource = new[] { "Türkçe", "English", "Français", "Deutsch", "Italiano", "Español", "中文", "日本語", "한국어", "हिन्दी", "العربية", "Русский", "Other / bilinmiyor" }, SelectedIndex = 0 };
    private readonly CheckBox reviewed = new();
    private readonly Label status = new() { Text = Ui.T("photoHint") };
    private readonly VerticalStackLayout results = new() { Spacing = 12 };
    private readonly Button capture = new() { Text = Ui.T("capture") };
    private readonly Button pick = new() { Text = Ui.T("pickPhoto") };
    private readonly Button analyze = new() { Text = Ui.T("useReviewed") };
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
    private string Language => new[] { "tr", "en", "fr", "de", "it", "es", "zh", "ja", "ko", "hi", "ar", "ru", "other" }[Math.Max(0, language.SelectedIndex)];

    public OcrPage(int selectedMode = 0)
    {
        FlowDirection = Ui.Direction;
        Title = Ui.T("ocrTitle");
        BackgroundColor = Color.FromArgb("#F4F7F2");
        mode.SelectedIndex = selectedMode;
        var stack = new VerticalStackLayout { Padding = 22, Spacing = 14 };
        stack.Add(new Label { Text = Ui.T("ocrHeading"), FontSize = 25, TextColor = Color.FromArgb("#166456") });
        stack.Add(new Label { Text = Ui.T("ocrIntro") });
        stack.Add(mode); stack.Add(script); stack.Add(capture); stack.Add(pick); stack.Add(status); stack.Add(preview); stack.Add(text);
        stack.Add(new Label { Text = Ui.T("actualLanguage") }); stack.Add(language);
        var confirm = new HorizontalStackLayout(); confirm.Add(reviewed);
        confirm.Add(new Label { Text = Ui.T("reviewed"), VerticalOptions = LayoutOptions.Center });
        stack.Add(new Label { Text = Ui.T("ocrCoverage") });
        stack.Add(confirm); stack.Add(analyze);
        var translate=new Button { Text="Metnin tamamını çevir (etiket / menü)" };
        translate.Clicked+=async(_,_)=>{ if(busy)return; await Navigation.PushAsync(new TranslationPage(text.Text??"",Language=="other" ? "auto" : Language,mode.SelectedIndex)); };
        stack.Add(translate);stack.Add(results);
        stack.Add(new Label { Text = Ui.T("ocrPrivacy") });
        Content = new ScrollView { Content = stack };
        ToolbarItems.Add(new ToolbarItem(Ui.T("close"), null, async () => { if(Navigation.NavigationStack.Count>1)await Navigation.PopAsync();else await Navigation.PopModalAsync(); }));
        capture.Clicked += async (_, _) => await ReadPhotoAsync(true);
        pick.Clicked += async (_, _) => await ReadPhotoAsync(false);
        analyze.Clicked += async (_, _) => await AnalyzeAsync();
        text.TextChanged += (_, _) => ResetReview();
        language.SelectedIndexChanged += (_, _) => ResetReview();
        script.SelectedIndexChanged += (_, _) => ResetReview();
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
        text.IsEnabled = mode.IsEnabled = language.IsEnabled = script.IsEnabled = reviewed.IsEnabled = !value;
    }
    private async Task ReadPhotoAsync(bool useCamera)
    {
        if (busy) return;
        var scriptCode = ScriptOcr.Scripts[Math.Max(0, script.SelectedIndex)].Code;
        if(scriptCode == "unsupported") { status.Text = Ui.T("unsupportedScript"); return; }
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
            if (photo is null) { status.Text = Ui.T("photoCancelled"); return; }
            // Android may hide the page while its photo picker is active; resume this operation only after returning.
            if (!visible) return;
            token = lifetime.Token;
            status.Text = Ui.T("reading");
            var data = await OcrImage.ReadAsync(photo, token);
            text.Text = "";
            preview.Source = ImageSource.FromStream(() => new MemoryStream(data, writable: false));
            preview.IsVisible = true;
            var recognized = await ScriptOcr.ReadAsync(data, scriptCode, token);
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(recognized))
            { status.Text = Ui.T("readFailed"); return; }
            if (recognized.Length > 12000)
            { status.Text = "Okunan metin çok uzun. Yalnız içindekiler veya menü bölümünün fotoğrafını seçin."; return; }
            text.Text = recognized;
            status.Text = Ui.T("readDone");
        }
        catch (OperationCanceledException) { }
        catch (PermissionException) { status.Text = Ui.T("permission"); }
        catch (NotSupportedException ex) { status.Text = ex.Message; }
        catch (InvalidDataException ex) { status.Text = ex.Message; }
        catch (Exception) { status.Text = Ui.T("photoFailed"); }
        finally { SetBusy(false); }
    }
    private async Task AnalyzeAsync()
    {
        if (busy) return;
        results.Clear();
        if (string.IsNullOrWhiteSpace(text.Text)) { status.Text = Ui.T("textRequired"); return; }
        if (!reviewed.IsChecked) { status.Text = Ui.T("reviewRequired"); return; }
        if (mode.SelectedIndex == 1)
        {
            SetBusy(true);
            try {
                await DeviceFoodPreferences.AppendAsync(this,results,text.Text,Language);
                lifetime.Token.ThrowIfCancellationRequested();
                results.Add(new Label { Text = MenuQuestions.Prepare(text.Text, Language) });
                var cardsButton=new Button {Text="Çok dilli soru kartları ve seslendirme"};cardsButton.Clicked+=async(_,_)=>await Navigation.PushAsync(new QuestionCardsPage());results.Add(cardsButton);
                status.Text="İşletmeye sorulabilecek sorular hazırlandı.";
            }catch(OperationCanceledException){}finally{SetBusy(false);}
            return;
        }
        var token = lifetime.Token;
        SetBusy(true);
        try
        {
            await DeviceFoodPreferences.AppendAsync(this,results,text.Text,Language);
            token.ThrowIfCancellationRequested();
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
            results.Add(new Label { Text = $"{assessment.Profile}\n{assessment.RuleVersion}\nBu metin analizi sertifika doğrulaması değildir. Şüpheli paket içeriği için uygunluk doğrulanamaz.\n{HelalHaram.Shared.PrivacyNotice.Limits}" });
        }
        catch (OperationCanceledException) { if (visible) status.Text = "İstek tamamlanamadı. Tekrar deneyin."; }
        catch (Exception) { status.Text = "İçerik analizi alınamadı. İnternet ve analiz sunucusu bağlantısını kontrol edip tekrar deneyin. Bu hata uygunluk sonucu değildir."; }
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
