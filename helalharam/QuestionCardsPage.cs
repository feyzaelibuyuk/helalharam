using Microsoft.Maui.Media;
namespace helalharam;

public sealed class QuestionCardsPage : ContentPage
{
    private readonly Picker language=new() { Title="İşletmeye göstereceğim dil" };
    private readonly VerticalStackLayout cards=new() { Spacing=20 };
    private readonly Label status=new();
    private CancellationTokenSource? speech;
    private int generation;
    private bool visible;
    public QuestionCardsPage()
    {
        Title="Çok dilli soru kartları";
        var body=new VerticalStackLayout { Padding=22,Spacing=16 };
        body.Add(new Label { Text="Restoranda sor, yanıtı doğrula",FontSize=26 });
        body.Add(new Label { Text="Sabit soru kartlarıdır; menüde bu bileşenlerin bulunduğu veya işletmenin uygun olduğu anlamına gelmez. Metinler taslaktır; ana dili konuşan kişilerce kontrol edilmelidir." });
        body.Add(new Label { Text="Kart dili OCR/çeviri/arayüz dilinden bağımsızdır. Seslendirme cihazdaki ses motorunu kullanır; uygulama ücretli API çağırmaz. Ses motoru ayarına göre internet kullanabilir. Uygun dil sesi yoksa kartı metin olarak gösterin." });
        language.ItemsSource=QuestionCards.Languages.Select(x=>x.Name).ToArray();
        language.SelectedIndex=Math.Max(0,QuestionCards.Languages.ToList().FindIndex(x=>x.Code==Ui.Language));
        body.Add(language);
        var stop=new Button { Text="Sesi durdur" };stop.Clicked+=(_,_)=>{Stop();status.Text="Ses durduruldu.";};body.Add(stop);body.Add(status);body.Add(cards);
        var close=new Button { Text=Ui.T("close") };close.Clicked+=async(_,_)=>{Stop();if(Navigation.NavigationStack.Count>1)await Navigation.PopAsync();else await Navigation.PopModalAsync();};body.Add(close);
        language.SelectedIndexChanged+=(_,_)=>{Stop();Render();};
        Content=new ScrollView { Content=body };Render();
    }
    protected override void OnAppearing(){base.OnAppearing();visible=true;}
    protected override void OnDisappearing(){visible=false;Stop();base.OnDisappearing();}
    private void Stop(){generation++;speech?.Cancel();}
    private void Render()
    {
        cards.Clear();status.Text="Seslendirmek için bir kartın düğmesine basın.";
        if(language.SelectedIndex<0)return;
        var selected=QuestionCards.Languages[language.SelectedIndex];
        foreach(var card in QuestionCards.Get(selected.Code))
        {
            var box=new VerticalStackLayout { Spacing=10,Padding=14,BackgroundColor=Color.FromArgb("#F1F5F0") };
            box.Add(new Label { Text=card.Turkish,FontAttributes=FontAttributes.Bold });
            box.Add(new Label { Text=card.Text,FontSize=23,FlowDirection=selected.Direction=="rtl"?FlowDirection.RightToLeft:FlowDirection.LeftToRight });
            var button=new Button { Text="Seslendir · "+selected.Name };button.Clicked+=async(_,_)=>await SpeakAsync(card.Text,selected.Code);box.Add(button);cards.Add(box);
        }
    }
    private async Task SpeakAsync(string text,string code)
    {
        Stop();var version=generation;using var cts=new CancellationTokenSource();speech=cts;
        try
        {
            status.Text="Cihazın sesleri kontrol ediliyor…";
            var locales=await TextToSpeech.Default.GetLocalesAsync();
            if(!visible||version!=generation||cts.IsCancellationRequested)return;
            var locale=locales.FirstOrDefault(x=>QuestionCards.NormalizeVoiceLanguage(x.Language)==code);
            if(locale is null){status.Text="Bu dilde ses bulunamadı. Cihazın metin okuma ayarlarını kontrol edin; kartı metin olarak gösterebilirsiniz.";return;}
            status.Text="Seslendiriliyor…";
            await TextToSpeech.Default.SpeakAsync(text,new SpeechOptions { Locale=locale },cts.Token);
            if(visible&&version==generation)status.Text="Seslendirme tamamlandı.";
        }
        catch(OperationCanceledException) { }
        catch(Exception){if(visible&&version==generation)status.Text="Seslendirme kullanılamıyor. Cihazın ses motorunu kontrol edin; kartı metin olarak gösterebilirsiniz.";}
        finally {if(ReferenceEquals(speech,cts))speech=null;}
    }
}
