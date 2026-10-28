namespace helalharam;

public sealed class HomePage : TabbedPage
{
    private readonly Color green = Color.FromArgb("#166456");

    public HomePage()
    {
        Title = "Helal Rehber";
        FlowDirection = Ui.Direction;
        BarBackgroundColor = Colors.White;
        BarTextColor = Color.FromArgb("#1D342C");
        SelectedTabColor = green;
        UnselectedTabColor = Color.FromArgb("#71867B");
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this,
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetIsSwipePagingEnabled(this, false);
        AddTab(new MainPage { Title = Ui.T("catalog") }, "nav_catalog.png");
        AddTab(new FavoritesPage { Title = Ui.T("favorites") }, "nav_favorites.png");
        AddTab(Menu(), "nav_menu.png");
        AddTab(Community(), "nav_community.png");
        AddTab(Profile(), "nav_profile.png");
    }

    private void AddTab(ContentPage page, string icon)
    {
        page.IconImageSource = icon;
        Children.Add(page);
    }
    private Border Group(string title, params View[] views)
    {
        var content = new VerticalStackLayout { Spacing = 12 };
        content.Add(new Label { Text = title, FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = green });
        foreach(var view in views) content.Add(view);
        return new Border { Content = content, Padding = 16, BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#DFE7DF"), StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 } };
    }

    private ContentPage Page(string title, VerticalStackLayout stack) => new()
    {
        Title = title, BackgroundColor = Color.FromArgb("#F4F7F2"),
        Content = new ScrollView { Content = stack }
    };
    private VerticalStackLayout Stack(string heading, string description) => new()
    {
        Padding = 22, Spacing = 16,
        Children = { new Label { Text = heading, FontSize = 27, FontAttributes = FontAttributes.Bold, TextColor = green }, Text(description) }
    };
    private static Label Text(string value) => new() { Text = value, FontSize = 16, TextColor = Color.FromArgb("#243B35") };
    private Button Action(string title, EventHandler handler)
    {
        var button = new Button { Text = title, BackgroundColor = green, TextColor = Colors.White, CornerRadius = 12 };
        button.Clicked += handler;
        return button;
    }

    private ContentPage Menu()
    {
        var stack = Stack("Menüden soruya", "Menüdeki yemek adlarını ve varsa içerik açıklamalarını yaz. Sorular işletmeden bilgi almak içindir; uygunluk sonucu değildir.");
        var input = new Editor { Placeholder = "Örnek: Mercimek çorbası, tavuk ızgara, kremalı makarna", HeightRequest = 130 };
        var output = Text("");
        stack.Add(Action("Yakındaki restoranlar · Harita", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new TravelPage()))));
        stack.Add(Action("Menü fotoğrafını oku", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new OcrPage(1)))));
        stack.Add(Action("Çok dilli soru kartları ve seslendirme", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new QuestionCardsPage()))));
        stack.Add(input);
        stack.Add(Action("Sorulacak soruları hazırla", (_, _) =>
        {
            var value = input.Text?.Trim() ?? "";
            if (value.Length == 0) { output.Text = "Önce menü veya içerik metnini gir."; return; }
            output.Text = MenuQuestions.Prepare(value, "tr");
        }));
        stack.Add(output);
        return Page(Ui.T("menu"), stack);
    }

    private ContentPage Community()
    {
        var stack = Stack(Ui.T("community"), "İşletmeye ne sordun, ne yanıt aldın? Deneyimleri incele, kendi paylaşımını incelemeye gönder. Topluluk beyanları doğrulanmış uygunluk değildir.");
        stack.Add(Action("Topluluk paylaşımları ve oylar", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new CommunityPage()))));
        stack.Add(Action(Ui.T("notifications"), async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new CommunityPage(true)))));
        var local = new VerticalStackLayout { Spacing = 12, IsVisible = false };
        stack.Add(Action("Cihazdaki özel taslak notlarım", (_, _) => local.IsVisible = !local.IsVisible));
        local.Add(Text("Bu bölümdeki notlar yalnız bu cihazda kalır; incelemeye gönderilmez. Yayınlamak için yukarıdaki Topluluk paylaşımları ve oylar düğmesini kullanın."));
        stack.Add(local);
        var shop = new Entry { Placeholder = "Dükkân ve şube" };
        var dish = new Entry { Placeholder = "Yediğin yemek" };
        var date = new DatePicker { Date = DateTime.Today, MaximumDate = DateTime.Today };
        var question = new Entry { Placeholder = "İşletmeye sorduğun soru" };
        var answer = new Editor { Placeholder = "Alınan yanıt ve deneyimin", HeightRequest = 100 };
        var source = new Entry { Placeholder = "Kaynak / belge bağlantısı (isteğe bağlı)" };
        var saved = Text(Preferences.Default.Get("communityDraft", "Henüz yerel deneyim kaydı yok."));
        foreach (var view in new View[] { shop, dish, date, question, answer, source }) local.Add(view);
        local.Add(Action("Deneyimi cihazda kaydet", (_, _) =>
        {
            if (new[] { shop.Text, dish.Text, question.Text, answer.Text }.Any(string.IsNullOrWhiteSpace))
            { saved.Text = "Dükkân, yemek, soru ve yanıt alanlarını doldur."; return; }
            saved.Text = $"DOĞRULANMAMIŞ KULLANICI DENEYİMİ\n{shop.Text} • {dish.Text}\nZiyaret: {date.Date:dd.MM.yyyy}\nSoru: {question.Text}\nYanıt: {answer.Text}\nKaynak: {source.Text}\nYerel taslak — moderasyona gönderilmedi.";
            Preferences.Default.Set("communityDraft", saved.Text);
        }));
        local.Add(Action("Yerel deneyimi sil", (_, _) => { Preferences.Default.Remove("communityDraft"); saved.Text = "Yerel kayıt silindi."; }));
        local.Add(saved);
        return Page(Ui.T("community"), stack);
    }

    private ContentPage Profile()
    {
        var stack = Stack(Ui.T("profileHeading"), Ui.T("profileIntro"));
        stack.Add(Group("Beslenme tercihlerim", Text(Ui.T("profileLimit")),
            Action("Alerjen / diyet tercihleri", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new FoodPreferencesPage())))));
        stack.Add(Group("Keşif araçları",
            Action("Etiket / menü çevirisi", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new TranslationPage()))),
            Action("Harita ve seyahat", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new TravelPage()))),
            Action("Sertifika kaynakları / belge inceleme", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new CertificateEvidencePage())))));
        stack.Add(Group(Ui.T("notifications"),
            Text("Topluluk paylaşımlarının durum değişikliklerini ekranı açınca kontrol edebilirsiniz. Arka plan telefon bildirimi gönderilmez."),
            Action("İnceleme bildirimlerini aç", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new CommunityPage(true))))));
        var uiLanguage = new Picker { Title = Ui.T("language"), ItemsSource = new[] { "Türkçe", "English", "العربية" }, SelectedIndex = Array.IndexOf(HelalRehber.Shared.UiCatalog.Languages, Ui.Language) };
        var apply = Action(Ui.T("applyLanguage"), async (_, _) => {
            if (uiLanguage.SelectedIndex < 0) return;
            if (!await DisplayAlertAsync(Ui.T("confirmLanguage"), Ui.T("languageNote"), Ui.T("apply"), Ui.T("cancel"))) return;
            Preferences.Default.Set("ui-language", HelalRehber.Shared.UiCatalog.Languages[uiLanguage.SelectedIndex]);
            if (Window is { } window) { var home = new HomePage(); home.CurrentPage = home.Children.Last(); window.Page = home; }
        });
        stack.Add(Group(Ui.T("language"), uiLanguage, Text(Ui.T("languageNote")), apply));
        if(Ui.Language != "tr") stack.Add(Text(Ui.T("scope")));
        var personal = new Entry { Placeholder = "Eski not alanı — otomatik filtre değildir", Text = Preferences.Default.Get("avoid", "") };
        var feedback = Text("");
        stack.Add(Group("Özel notum", Text("Otomatik değerlendirme için yukarıdaki alerjen / diyet tercihlerini kullanın."), personal,
            Action(Ui.T("savePreferences"), (_, _) => { Preferences.Default.Set("avoid", personal.Text ?? ""); feedback.Text = Ui.T("preferencesSaved"); }), feedback));
        stack.Add(Group("Uygulama",
            Action(Ui.T("feedbackAction"), async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new FeedbackPage()))),
            Action(Ui.T("privacy"), async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new PrivacyPage()))),
            Action("Bağlantıyı kontrol et", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new ConnectionCheckPage()))),
            Text($"Sürüm: {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})"), Text(Ui.T("testSetup"))));
        return Page(Ui.T("profile"), stack);
    }
}
