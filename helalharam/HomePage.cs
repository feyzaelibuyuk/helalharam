namespace helalharam;

public sealed class HomePage : TabbedPage
{
    private readonly Color green = Color.FromArgb("#166456");
    private readonly string[] demoProducts = ["Demo Meyveli Şeker", "Demo Sebzeli Kraker"];

    public HomePage()
    {
        Title = "Helal Rehber";
        BarBackgroundColor = green;
        BarTextColor = Colors.White;
        Children.Add(new MainPage { Title = "Katalog" });
        Children.Add(new FavoritesPage());
        Children.Add(Products());
        Children.Add(Menu());
        Children.Add(Community());
        Children.Add(Notifications());
        Children.Add(Profile());
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

    private ContentPage Products()
    {
        var stack = Stack("Ne yediğini keşfet", "DEMO • Bu katalog eğitim amaçlıdır. Gerçek ürün veya dini uygunluk kararı içermez.");
        var search = new Entry { Placeholder = "Ürün adı veya demo barkodu: 0001 / 0002" };
        var results = new VerticalStackLayout { Spacing = 14 };
        void Find()
        {
            results.Clear();
            var query = search.Text?.Trim() ?? "";
            var matches = demoProducts.Select((name, index) => (name, index))
                .Where(p => query.Length == 0 || p.name.Contains(query, StringComparison.OrdinalIgnoreCase) || query == $"000{p.index + 1}").ToArray();
            if (matches.Length == 0) results.Add(Text("Ürün bulunamadı. İçindekiler metnini menü/içerik ekranına yazabilirsiniz."));
            foreach (var item in matches)
            {
                results.Add(Text(item.name));
                results.Add(Text(item.index == 0
                    ? "Şüpheli gıda — Demo etikette jelatin var; kaynağı belirtilmemiş. Ürünün uygunluğu doğrulanamıyor."
                    : "Yetersiz bilgi — Demo içerik: un, bitkisel yağ, tuz. Tam üretim ve kaynak bilgisi olmadan uygunluk doğrulanamaz."));
                results.Add(Text("Kaynak: uygulama içi kurgu örnek. Doğrulanmış kayıt değildir."));
                results.Add(Text(item.index == 0 ? "Alternatif arama ölçütü: aynı kategoride, jelatin kaynağı belgelenmiş ürün. Henüz doğrulanmış alternatif yok." : "Henüz doğrulanmış alternatif yok."));
            }
        }
        stack.Add(search);
        stack.Add(Action("Ürün ara", (_, _) => Find()));
        stack.Add(Action("Etiket fotoğrafını oku", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new OcrPage()))));
        stack.Add(results);
        Find();
        return Page("Ürünler", stack);
    }

    private ContentPage Menu()
    {
        var stack = Stack("Menüden soruya", "Menüdeki yemek adlarını ve varsa içerik açıklamalarını yaz. Sorular işletmeden bilgi almak içindir; uygunluk sonucu değildir.");
        var input = new Editor { Placeholder = "Örnek: Mercimek çorbası, tavuk ızgara, kremalı makarna", HeightRequest = 130 };
        var output = Text("");
        stack.Add(Action("Menü fotoğrafını oku", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new OcrPage(1)))));
        stack.Add(input);
        stack.Add(Action("Sorulacak soruları hazırla", (_, _) =>
        {
            var value = input.Text?.Trim() ?? "";
            if (value.Length == 0) { output.Text = "Önce menü veya içerik metnini gir."; return; }
            output.Text = MenuQuestions.Prepare(value, "tr");
        }));
        stack.Add(output);
        return Page("Menü", stack);
    }

    private ContentPage Community()
    {
        var stack = Stack("Deneyimini paylaş", "İşletmeye ne sordun, ne yanıt aldın? Kayıtlar bu cihazda saklanır; yayınlanmaz ve doğrulanmış uygunluk sayılmaz.");
        var shop = new Entry { Placeholder = "Dükkân ve şube" };
        var dish = new Entry { Placeholder = "Yediğin yemek" };
        var date = new DatePicker { Date = DateTime.Today, MaximumDate = DateTime.Today };
        var question = new Entry { Placeholder = "İşletmeye sorduğun soru" };
        var answer = new Editor { Placeholder = "Alınan yanıt ve deneyimin", HeightRequest = 100 };
        var source = new Entry { Placeholder = "Kaynak / belge bağlantısı (isteğe bağlı)" };
        var saved = Text(Preferences.Default.Get("communityDraft", "Henüz yerel deneyim kaydı yok."));
        foreach (var view in new View[] { shop, dish, date, question, answer, source }) stack.Add(view);
        stack.Add(Action("Deneyimi cihazda kaydet", (_, _) =>
        {
            if (new[] { shop.Text, dish.Text, question.Text, answer.Text }.Any(string.IsNullOrWhiteSpace))
            { saved.Text = "Dükkân, yemek, soru ve yanıt alanlarını doldur."; return; }
            saved.Text = $"DOĞRULANMAMIŞ KULLANICI DENEYİMİ\n{shop.Text} • {dish.Text}\nZiyaret: {date.Date:dd.MM.yyyy}\nSoru: {question.Text}\nYanıt: {answer.Text}\nKaynak: {source.Text}\nYerel taslak — moderasyona gönderilmedi.";
            Preferences.Default.Set("communityDraft", saved.Text);
        }));
        stack.Add(Action("Yerel deneyimi sil", (_, _) => { Preferences.Default.Remove("communityDraft"); saved.Text = "Yerel kayıt silindi."; }));
        stack.Add(saved);
        return Page("Deneyimler", stack);
    }

    private ContentPage Notifications()
    {
        var stack = Stack("Bildirimler", "Gerçek bildirim servisi henüz bağlı değil. Planlanan bildirimler:");
        stack.Add(Text("• Favori ürünün içerik veya değerlendirme bilgisi değişti.\n\n• Beklediğin ürünün eksik bilgisi tamamlandı.\n\n• Gönderdiğin deneyim veya düzeltme moderasyonda sonuçlandı."));
        return Page("Bildirimler", stack);
    }

    private ContentPage Profile()
    {
        var stack = Stack("Beslenme profilim", "İlk sürüm: helal beslenme. Diğer dini profiller, kendilerine ait doğrulanmış kaynak ve kurallarla eklenecek.");
        stack.Add(Text("Seçili profil: Helal\n\nDemo değerlendirmeler dini hüküm veya sertifika değildir."));
        var personal = new Entry { Placeholder = "Ek olarak kaçındığın içerikler", Text = Preferences.Default.Get("avoid", "") };
        stack.Add(personal);
        var feedback = Text("");
        stack.Add(Action("Tercihleri kaydet", (_, _) => { Preferences.Default.Set("avoid", personal.Text ?? ""); feedback.Text = "Tercihler cihazda kaydedildi. Değerlendirme kurallarına bağlanması sonraki görevde yapılacak."; }));
        stack.Add(feedback);
        stack.Add(Action("Gizlilik ve kullanım sınırları", async (_, _) => await Navigation.PushModalAsync(new NavigationPage(new PrivacyPage()))));
        return Page("Profil", stack);
    }
}
