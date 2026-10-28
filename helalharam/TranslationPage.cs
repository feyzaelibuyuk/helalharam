using System.Net.Http.Json;
using HelalRehber.Shared;
namespace helalharam;
public sealed class TranslationPage : ContentPage
{
    private readonly HttpClient client=new() { Timeout=TimeSpan.FromSeconds(300),
#if ANDROID
        BaseAddress=new Uri("http://10.0.2.2:5088/")
#else
        BaseAddress=new Uri("http://localhost:5088/")
#endif
    };
    private readonly Editor text=new() { HeightRequest=220, Placeholder="İçindekiler, alerjen / eser miktar uyarıları, besin değerleri ve diğer açıklamaların tamamı" };
    private readonly Picker source=new() { Title="Kaynak dil" };
    private readonly Picker target=new() { Title="Çeviri dili" };
    private readonly Picker mode=new() { Title="Metin türü", ItemsSource=new[]{"Paketin arka yüzü / etiket","Restoran menüsü"}, SelectedIndex=0 };
    private readonly CheckBox reviewed=new(), consent=new();
    private readonly Button send=new() { Text="Metnin tamamını çevir", IsEnabled=false };
    private readonly Button reload=new() { Text="Dil listesini yenile" };
    private readonly Label status=new();
    private readonly VerticalStackLayout results=new() { Spacing=12 };
    private TranslationOptions? options;
    private readonly string initialLanguage;
    private CancellationTokenSource lifetime=new();
    private bool visible,busy,loaded;
    public TranslationPage(string original="",string sourceLanguage="auto",int selectedMode=0)
    {
        Title="Etiket / menü çevirisi"; FlowDirection=Ui.Direction;initialLanguage=sourceLanguage=="zh-Hans" ? "zh" : sourceLanguage;
        text.Text=original; mode.SelectedIndex=selectedMode;
        var body=new VerticalStackLayout { Padding=22,Spacing=14 };
        body.Add(new Label { Text="Barkod bulunmasa da metnin tamamını çevir",FontSize=24 });
        body.Add(new Label { Text="Yalnız burada yer alan metin çevrilir. Fotoğrafta eksik / okunmayan bölümler çevrilmiş sayılmaz. Kaynak metni kontrol edip düzeltin. En fazla 12.000 karakter; otomatik kısaltma yapılmaz." });
        body.Add(mode);body.Add(text);body.Add(source);body.Add(target);
        var reviewRow=new VerticalStackLayout();reviewRow.Add(new Label { Text="Metni özgün etiket / menüyle karşılaştırıp düzelttim." });reviewRow.Add(reviewed);body.Add(reviewRow);
        var consentRow=new VerticalStackLayout();consentRow.Add(new Label { Text="Bu metnin sunucunun bilgisayarındaki yerel çeviri servisine gönderilmesini onaylıyorum. Fotoğraf gönderilmez. Gerekirse ek İngilizce ön kontrol çevirisi kullanılır; kotaya dahildir. Kişisel / sağlık bilgisi yazmayın." });consentRow.Add(consent);body.Add(consentRow);
        body.Add(status);body.Add(reload);body.Add(send);body.Add(results);
        var close=new Button { Text=Ui.T("close") };close.Clicked+=async(_,_)=>{if(Navigation.NavigationStack.Count>1)await Navigation.PopAsync();else await Navigation.PopModalAsync();};body.Add(close);
        Content=new ScrollView { Content=body };
        text.TextChanged+=(_,_)=>{reviewed.IsChecked=false;consent.IsChecked=false;results.Clear();};
        source.SelectedIndexChanged+=(_,_)=>{reviewed.IsChecked=false;results.Clear();};
        target.SelectedIndexChanged+=(_,_)=>results.Clear();mode.SelectedIndexChanged+=(_,_)=>results.Clear();
        send.Clicked+=async(_,_)=>await TranslateAsync();reload.Clicked+=async(_,_)=>await LoadAsync();
    }
    protected override async void OnAppearing(){base.OnAppearing();visible=true;if(lifetime.IsCancellationRequested){lifetime.Dispose();lifetime=new();}if(!loaded)await LoadAsync();}
    protected override void OnDisappearing(){visible=false;lifetime.Cancel();base.OnDisappearing();}
    private void SetBusy(bool value){busy=value;send.IsEnabled=!value&&options?.Configured==true&&options.Languages.Count>0;reload.IsEnabled=!value;text.IsEnabled=source.IsEnabled=target.IsEnabled=mode.IsEnabled=reviewed.IsEnabled=consent.IsEnabled=!value;}
    private async Task LoadAsync()
    {
        if(busy)return;SetBusy(true);var token=lifetime.Token;
        try
        {
            options=await client.GetFromJsonAsync<TranslationOptions>("api/translation/options",token);token.ThrowIfCancellationRequested();
            if(options is null)throw new InvalidDataException();
            source.ItemsSource=new[]{"Otomatik algıla — kontrol edin"}.Concat(options.Languages.Select(x=>$"{x.NativeName} ({x.Code})")).ToArray();
            target.ItemsSource=options.Languages.Select(x=>$"{x.NativeName} ({x.Code})").ToArray();
            source.SelectedIndex=options.Languages.FindIndex(x=>x.Code==initialLanguage)+1;
            target.SelectedIndex=options.Languages.Count==0 ? -1 : Math.Max(0,options.Languages.FindIndex(x=>x.Code==Ui.Language));
            status.Text=options.Message;loaded=options.Languages.Count>0;
        }
        catch(OperationCanceledException){if(visible)status.Text="Dil listesi isteği tamamlanamadı. Tekrar deneyin.";}
        catch(Exception){if(visible)status.Text="Dil listesi alınamadı. Yerel API ve OPUS çeviri geçidinin açık olduğunu kontrol edin.";}
        finally{SetBusy(false);}
    }
    private async Task TranslateAsync()
    {
        if(busy||options is null)return;
        var input=new TranslationInput { Text=text.Text??"",SourceLanguage=source.SelectedIndex<=0 ? "auto" : options.Languages[source.SelectedIndex-1].Code,TargetLanguage=target.SelectedIndex<0 ? "" : options.Languages[target.SelectedIndex].Code,Mode=mode.SelectedIndex==1?"menu":"label",ReviewedSource=reviewed.IsChecked,CloudConsent=consent.IsChecked };
        var errors=TranslationRules.Validate(input);if(errors.Count>0){status.Text=string.Join("\n",errors);return;}
        SetBusy(true);results.Clear();var token=lifetime.Token;
        try
        {
            using var response=await client.PostAsJsonAsync("api/translation",input,token);token.ThrowIfCancellationRequested();
            if(response.StatusCode==System.Net.HttpStatusCode.TooManyRequests){status.Text="Çeviri kullanım sınırına ulaşıldı. Daha sonra tekrar deneyin.";return;}
            var result=await response.Content.ReadFromJsonAsync<Result>(cancellationToken:token);token.ThrowIfCancellationRequested();
            status.Text=result?.Message??"Çeviri alınamadı. Özgün metniniz korunuyor.";
            if(!response.IsSuccessStatusCode||result?.Document is not { } doc)return;
            foreach(var warning in doc.Warnings)results.Add(new Label { Text=warning });
            results.Add(new Label { Text="ÖZGÜN METİN",FontAttributes=FontAttributes.Bold });results.Add(new Label { Text=doc.OriginalText });
            results.Add(new Label { Text=$"MAKİNE ÇEVİRİSİ · {doc.SourceLanguage} → {doc.TargetLanguage} · {doc.Provider}",FontAttributes=FontAttributes.Bold });
            results.Add(new Label { Text=doc.TranslatedText,FlowDirection=doc.Direction=="rtl"?FlowDirection.RightToLeft:FlowDirection.LeftToRight });
            results.Add(new Label {Text="Özgün metin: kişisel filtreler",FontAttributes=FontAttributes.Bold});
            await DeviceFoodPreferences.AppendAsync(this,results,doc.OriginalText,doc.SourceLanguage);token.ThrowIfCancellationRequested();
            results.Add(new Label {Text="Çeviri: kişisel filtreler — çeviri hatalı olabilir",FontAttributes=FontAttributes.Bold});
            await DeviceFoodPreferences.AppendAsync(this,results,doc.TranslatedText,doc.TargetLanguage);token.ThrowIfCancellationRequested();
            if(doc.OriginalAssessment is { } originalAssessment)
            {
                results.Add(new Label { Text="Özgün metin değerlendirmesi",FontAttributes=FontAttributes.Bold });
                results.Add(new Label { Text=originalAssessment.Status+" · "+originalAssessment.Summary });
                foreach(var finding in originalAssessment.Findings)results.Add(new Label { Text=$"{finding.Ingredient} · {finding.MatchedText}\n{finding.Reason}\n{finding.SourceUrl}" });
            }
            if(doc.TranslationPrecheck is { } check)
            {
                results.Add(new Label { Text="Çeviri üzerinde ön kontrol — ürün onayı değildir",FontAttributes=FontAttributes.Bold });
                if(doc.PrecheckLanguage!=doc.TargetLanguage){results.Add(new Label { Text=$"Ek ön kontrol metni ({doc.PrecheckLanguage})" });results.Add(new Label { Text=doc.PrecheckText,FlowDirection=FlowDirection.LeftToRight });}
                foreach(var severity in new[]{"restricted","uncertain"})
                {
                    results.Add(new Label { Text=severity=="restricted"?"Çeviride kısıtlı madde eşleşmeleri":"Çeviride şüpheli / kaynağı belirsiz eşleşmeler",FontAttributes=FontAttributes.Bold });
                    var findings=check.Findings.Where(x=>x.Severity==severity).ToList();
                    if(findings.Count==0)results.Add(new Label { Text="Bu sınırlı kurallarda eşleşme yok; helal veya alerjen güvenliği onayı değildir." });
                    foreach(var finding in findings)results.Add(new Label { Text=$"{finding.Ingredient} · {finding.MatchedText}\n{finding.Reason}\n{finding.SourceTitle}\n{finding.SourceUrl}" });
                }
            }
            if(doc.Mode=="menu")results.Add(new Label { Text="Menü çevirisi gerçek tarifi ve hazırlama yöntemini doğrulamaz. İçerikleri işletmeye sorun." });
        }
        catch(OperationCanceledException){if(visible)status.Text="Çeviri tamamlanamadı. Metniniz korunuyor; tekrar deneyin.";}
        catch(Exception){if(visible)status.Text="Çeviri alınamadı. Metniniz korunuyor; bağlantıyı kontrol edin.";}
        finally{SetBusy(false);}
    }
    private sealed class Result { public string Message{get;set;}="";public Document? Document{get;set;} }
    private sealed class Document { public string OriginalText{get;set;}="";public string TranslatedText{get;set;}="";public string SourceLanguage{get;set;}="";public string TargetLanguage{get;set;}="";public string Provider{get;set;}="";public string Direction{get;set;}="ltr";public string Mode{get;set;}="label";public List<string> Warnings{get;set;}=[];public string PrecheckText{get;set;}="";public string PrecheckLanguage{get;set;}="";public Assessment? OriginalAssessment{get;set;}public Assessment? TranslationPrecheck{get;set;} }
    private sealed class Assessment { public string Status{get;set;}="";public string Summary{get;set;}="";public List<Finding> Findings{get;set;}=[]; }
    private sealed class Finding { public string Ingredient{get;set;}="";public string MatchedText{get;set;}="";public string Severity{get;set;}="";public string Reason{get;set;}="";public string SourceTitle{get;set;}="";public string SourceUrl{get;set;}=""; }
}
