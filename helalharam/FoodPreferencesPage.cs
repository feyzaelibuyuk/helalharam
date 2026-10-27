using HelalRehber.Shared;
using System.Text.Json;
namespace helalharam;
public sealed class FoodPreferencesPage:ContentPage
{
    private readonly Dictionary<string,CheckBox> allergens=new();
    private readonly Picker diet=new(){Title="Özel diyet",ItemsSource=new[]{"Seçilmedi","Vejetaryen","Vegan"},SelectedIndex=0};
    private readonly CheckBox gluten=new(),lactose=new();private readonly Editor custom=new(){Placeholder="Virgülle ayrılmış ek kaçınılan içerikler (en fazla 8)",MaxLength=500,HeightRequest=90};private readonly Label status=new();private bool loaded,busy;
    public FoodPreferencesPage()
    {
        Title="Alerjen / diyet tercihleri";FlowDirection=FlowDirection.LeftToRight;
        var body=new VerticalStackLayout{Padding=22,Spacing=12};body.Add(new Label{Text="Kişisel alerjen ve diyet tercihleri",FontSize=25,FontAttributes=FontAttributes.Bold});
        body.Add(new Label {Text="Seçimler cihazın güvenli deposunda saklanır; web profiliyle eşitlenmez, sunucuya/OFF/çeviri servisine gönderilmez. Bu alan tanı veya sağlık öyküsü için değildir."});
        body.Add(new Label {Text=FoodPreferenceRules.Limits});
        foreach(var a in FoodPreferenceRules.All){var box=new CheckBox();allergens[a.Id]=box;body.Add(new Label {Text=a.Name});body.Add(box);}
        body.Add(diet);body.Add(new Label{Text="Glutenden kaçınma (glutensiz onayı değildir)"});body.Add(gluten);body.Add(new Label{Text="Laktozdan kaçınma (süt alerjisiyle aynı değildir)"});body.Add(lactose);body.Add(custom);
        var save=new Button {Text="Tercihleri cihazda kaydet"};save.Clicked+=async(_,_)=>{
            if(busy)return;busy=true;
            try{var value=new FoodPreferences {Allergens=allergens.Where(x=>x.Value.IsChecked).Select(x=>x.Key).ToArray(),Diet=new[]{"none","vegetarian","vegan"}[Math.Max(0,diet.SelectedIndex)],AvoidGluten=gluten.IsChecked,AvoidLactose=lactose.IsChecked,Custom=custom.Text??""};var errors=FoodPreferenceRules.Validate(value);if(errors.Count>0){status.Text=string.Join("\n",errors);return;}await SecureStorage.Default.SetAsync(DeviceFoodPreferences.Key,JsonSerializer.Serialize(value));DeviceFoodPreferences.Changed();status.Text="Tercihler kaydedildi. Önceki ekrana dönünce kişisel uyarılar güncellenir; metin/çeviri korunur.";}
            catch(Exception){status.Text="Güvenli depoya kaydedilemedi. Girdileriniz korunuyor; yeniden deneyin.";}finally{busy=false;}
        };body.Add(save);
        var clear=new Button {Text="Tüm kişisel filtreleri temizle"};clear.Clicked+=(_,_)=>{if(busy)return;try{SecureStorage.Default.Remove(DeviceFoodPreferences.Key);DeviceFoodPreferences.Changed();Apply(new());status.Text="Kişisel filtreler bu cihazdan temizlendi.";}catch(Exception){status.Text="Tercihler temizlenemedi; cihazın güvenli deposunu kontrol edin.";}};body.Add(clear);body.Add(status);
        var close=new Button {Text=Ui.T("close")};close.Clicked+=async(_,_)=>{if(Navigation.NavigationStack.Count>1)await Navigation.PopAsync();else await Navigation.PopModalAsync();};body.Add(close);Content=new ScrollView {Content=body};
    }
    private void Apply(FoodPreferences p){foreach(var a in allergens)a.Value.IsChecked=p.Allergens.Contains(a.Key);diet.SelectedIndex=Array.IndexOf(new[]{"none","vegetarian","vegan"},p.Diet);gluten.IsChecked=p.AvoidGluten;lactose.IsChecked=p.AvoidLactose;custom.Text=p.Custom;}
    protected override async void OnAppearing(){base.OnAppearing();if(loaded)return;loaded=true;try{Apply(FoodPreferenceRules.Read(await SecureStorage.Default.GetAsync(DeviceFoodPreferences.Key)));}catch(Exception){status.Text="Kayıtlı tercihler okunamadı; bu ekranda görünen seçimler kayıtlı sayılmaz.";}}
}
