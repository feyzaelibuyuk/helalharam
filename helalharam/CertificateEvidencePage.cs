using System.Net.Http.Json;
using System.Globalization;
using HelalRehber.Shared;
namespace helalharam;
public sealed class CertificateEvidencePage:ContentPage
{
#if ANDROID
    private const string BaseUrl="http://10.0.2.2:5088/";
#else
    private const string BaseUrl="http://localhost:5088/";
#endif
    private readonly HttpClient client=new(new HttpClientHandler {UseCookies=true,AllowAutoRedirect=false}){BaseAddress=new Uri(BaseUrl),Timeout=TimeSpan.FromSeconds(60)};
    private readonly Entry name=new(){Placeholder="Ürün veya işletme adı",MaxLength=160}, reference=new(){Placeholder="Barkod / kayıt referansı",MaxLength=100},number=new(){Placeholder="Sertifika numarası",MaxLength=100},url=new(){Placeholder="Kaynak bağlantısı (https)",Keyboard=Keyboard.Url,MaxLength=1000},from=new(){Placeholder="Başlangıç yyyy-MM-dd (isteğe bağlı)",MaxLength=10},until=new(){Placeholder="Bitiş yyyy-MM-dd (isteğe bağlı)",MaxLength=10};
    private readonly Editor scope=new(){Placeholder="Ürün, üretim yeri / şube kapsamı",MaxLength=1000,HeightRequest=90},notes=new(){Placeholder="Not (isteğe bağlı)",MaxLength=2000,HeightRequest=70};
    private readonly Picker kind=new(){Title="Kayıt türü",ItemsSource=new[]{"Paketli ürün","Restoran / şube"},SelectedIndex=0},issuer=new(){Title="Belgeyi düzenleyen kuruluş"};
    private readonly CheckBox consent=new();private readonly Label status=new(),fileLabel=new(){Text="Dosya seçilmedi. Dosya veya kaynak bağlantısı ekleyin."};
    private readonly Image preview=new(){HeightRequest=200,Aspect=Aspect.AspectFit,IsVisible=false};
    private bool visible;
    private readonly Guid submission=Guid.NewGuid();private byte[]? bytes;private string fileName="";private bool busy,saved;private CancellationTokenSource lifetime=new();
    public CertificateEvidencePage()
    {
        Title="Sertifika kaynakları";
        var stack=new VerticalStackLayout {Padding=20,Spacing=12};
        stack.Add(new Label {Text="Markette / restoranda hemen kullan",FontSize=24,FontAttributes=FontAttributes.Bold});
        stack.Add(new Label {Text="Etiket veya menü için belge formu doldurmanız gerekmez. Fotoğrafı çekin, okunan metni kontrol edin; içerik değerlendirmesi, çeviri veya işletmeye sorulacak sorularla devam edin."});
        var labelNow=new Button {Text="İçindekiler fotoğrafını şimdi oku"};labelNow.Clicked+=async(_,_)=>{if(!busy)await Navigation.PushAsync(new OcrPage());};stack.Add(labelNow);
        var menuNow=new Button {Text="Menü fotoğrafını şimdi oku"};menuNow.Clicked+=async(_,_)=>{if(!busy)await Navigation.PushAsync(new OcrPage(1));};stack.Add(menuNow);
        stack.Add(new Label {Text="Resmî sertifika kaynakları",FontSize=24,FontAttributes=FontAttributes.Bold});
        stack.Add(new Label {Text="Bu liste tüm dünyadaki kuruluşları kapsamaz. HAK kuruluş akreditasyonunu gösterir; ürün sertifikası değildir. Logo veya fotoğraf tek başına doğrulama sağlamaz."});
        foreach(var a in CertificateAuthorities.All){var button=new Button {Text=a.Id+" · "+a.Region};button.Clicked+=async(_,_)=>{try{await Browser.Default.OpenAsync(a.Url,BrowserLaunchMode.SystemPreferred);}catch{status.Text="Kaynak açılamadı.";}};stack.Add(button);}
        stack.Add(new Label {Text="Sertifika fotoğrafını incelemeye gönder (isteğe bağlı)",FontSize=22,FontAttributes=FontAttributes.Bold});
        stack.Add(new Label {Text="Belge ve bilgiler yalnız yöneticiye görünür. İnceleme katalog/helal onayı değildir. Kişisel bilgileri kaldırın; paylaşma hakkınız olan dosyayı gönderin. Virüs taraması yapılmaz. Bilgisayardaki web/API sunucusu 5088 portunda açık olmalı."});
        issuer.ItemsSource=CertificateAuthorities.All.Where(x=>x.Id!="HAK").Select(x=>x.Id).Append("OTHER").ToArray();issuer.SelectedIndex=0;
        foreach(var view in new View[]{kind,name,reference,issuer,number,scope,from,until,url,notes})stack.Add(view);
        var camera=new Button {Text="Sertifikanın fotoğrafını şimdi çek"};camera.Clicked+=async(_,_)=>await CaptureAsync();stack.Add(camera);
        stack.Add(new Label {Text="Fotoğraf önce bu ekranda gösterilir. İncelemeye gönder düğmesine basana kadar sunucuya yüklenmez. Etiket/menü analizi için yukarıdaki hızlı okuma düğmelerini kullanın."});
        var pick=new Button {Text="PDF/JPG/PNG seç · En fazla 5 MiB"};pick.Clicked+=async(_,_)=>await PickAsync();stack.Add(pick);stack.Add(fileLabel);stack.Add(preview);
        var clear=new Button {Text="Seçilen dosyayı kaldır"};clear.Clicked+=(_,_)=>{if(busy||saved)return;bytes=null;fileName="";preview.Source=null;preview.IsVisible=false;fileLabel.Text="Dosya kaldırıldı.";};stack.Add(clear);
        stack.Add(new Label {Text="Belgeyi paylaşma hakkım var; yalnız yönetici tarafından incelenmesi için saklanmasını kabul ediyorum."});stack.Add(consent);
        var send=new Button {Text="İncelemeye gönder"};send.Clicked+=async(_,_)=>{if(busy||saved)return;send.IsEnabled=false;await SendAsync();send.IsEnabled=!saved;};stack.Add(send);stack.Add(status);
        var close=new Button {Text=Ui.T("close")};close.Clicked+=async(_,_)=>await Navigation.PopModalAsync();stack.Add(close);Content=new ScrollView {Content=stack};
    }
    protected override void OnDisappearing(){visible=false;lifetime.Cancel();base.OnDisappearing();}
    protected override void OnAppearing(){base.OnAppearing();visible=true;if(lifetime.IsCancellationRequested){lifetime.Dispose();lifetime=new();}}
    private void SelectEvidence(byte[] selected,string selectedName)
    {
        bytes=selected;fileName=selectedName;fileLabel.Text=fileName+" · "+bytes.Length+" bayt";
        var image=CertificateEvidenceRules.Mime(selected)?.StartsWith("image/")==true;
        preview.Source=image?ImageSource.FromStream(()=>new MemoryStream(selected,writable:false)):null;preview.IsVisible=image;
    }
    private async Task CaptureAsync()
    {
        if(busy||saved)return;busy=true;
        try {
            if(!MediaPicker.Default.IsCaptureSupported){status.Text="Bu cihazda kamera kullanılamıyor. Galeriden fotoğraf seçebilirsiniz.";return;}
            var photo=await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions {Title="Sertifika fotoğrafı"});
            if(photo is null||!visible)return;
            var token=lifetime.Token;
            var selected=await OcrImage.ReadAsync(photo,token);token.ThrowIfCancellationRequested();
            if(selected.Length>CertificateEvidenceRules.MaxFileBytes){status.Text="Fotoğraf 5 MiB sınırını aşıyor. Daha düşük çözünürlükle tekrar çekin; önceki seçiminiz korundu.";return;}
            var mime=CertificateEvidenceRules.Mime(selected);
            if(mime is not ("image/jpeg" or "image/png")){status.Text="JPG/PNG fotoğraf gerekli. Önceki seçiminiz korundu.";return;}
            SelectEvidence(selected,"sertifika-fotograf"+(mime=="image/png"?".png":".jpg"));
            status.Text="Fotoğraf hazır. Kontrol edin; isterseniz yeniden çekin veya kaldırın. Henüz gönderilmedi.";
        }catch(PermissionException){status.Text="Kamera izni verilmedi. Galeriden fotoğraf seçebilirsiniz.";}
        catch(OperationCanceledException){}
        catch(InvalidDataException ex){status.Text=ex.Message;}
        catch(Exception){status.Text="Fotoğraf çekilemedi. Önceki seçiminiz korundu.";}
        finally{busy=false;}
    }
    private async Task PickAsync()
    {
        if(busy||saved)return;busy=true;
        try {
            var file=await FilePicker.Default.PickAsync(new PickOptions {PickerTitle="Paylaşma hakkınız olan sertifika belgesi",FileTypes=new FilePickerFileType(new Dictionary<DevicePlatform,IEnumerable<string>>{{DevicePlatform.Android,new[]{"application/pdf","image/png","image/jpeg"}},{DevicePlatform.WinUI,new[]{".pdf",".png",".jpg",".jpeg"}},{DevicePlatform.iOS,new[]{"com.adobe.pdf","public.png","public.jpeg"}},{DevicePlatform.MacCatalyst,new[]{"com.adobe.pdf","public.png","public.jpeg"}}})});
            if(file is null||!visible)return;await using var stream=await file.OpenReadAsync();using var buffer=new MemoryStream();var chunk=new byte[65536];int read;
            while((read=await stream.ReadAsync(chunk,lifetime.Token))>0){if(buffer.Length+read>CertificateEvidenceRules.MaxFileBytes){status.Text="Dosya 5 MiB sınırını aşıyor. Önceki seçiminiz korundu.";return;}buffer.Write(chunk,0,read);}
            var selected=buffer.ToArray();if(CertificateEvidenceRules.Mime(selected) is null){status.Text="PDF/JPG/PNG dosyası seçin. Önceki seçiminiz korundu.";return;}
            SelectEvidence(selected,Path.GetFileName(file.FileName));
        }catch(OperationCanceledException){}catch(Exception){status.Text="Dosya okunamadı.";}finally{busy=false;}
    }
    private async Task SendAsync()
    {
        busy=true;
        try {
            DateOnly? start=null,end=null;
            if(!string.IsNullOrWhiteSpace(from.Text)){if(!DateOnly.TryParseExact(from.Text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)){status.Text="Başlangıç yyyy-MM-dd biçiminde olmalı.";return;}start=date;}
            if(!string.IsNullOrWhiteSpace(until.Text)){if(!DateOnly.TryParseExact(until.Text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)){status.Text="Bitiş yyyy-MM-dd biçiminde olmalı.";return;}end=date;}
            var input=new CertificateEvidenceInput {SubmissionId=submission,Kind=kind.SelectedIndex==0?"product":"restaurant",Name=name.Text??"",Reference=reference.Text??"",Issuer=issuer.SelectedItem?.ToString()??"",Number=number.Text??"",Scope=scope.Text??"",SourceUrl=url.Text??"",Notes=notes.Text??"",ValidFrom=start,ValidUntil=end,Consent=consent.IsChecked};
            var errors=CertificateEvidenceRules.Validate(input);if(bytes is null&&input.SourceUrl.Length==0)errors.Add("Dosya veya kaynak bağlantısı gerekli.");if(errors.Count>0){status.Text=string.Join("\n",errors);return;}
            status.Text="Gönderiliyor…";var af=await client.GetFromJsonAsync<TokenResult>("api/certificates/form-token",lifetime.Token);
            using var form=new MultipartFormDataContent();
            foreach(var pair in new Dictionary<string,string>{{"SubmissionId",submission.ToString()},{"Kind",input.Kind},{"Name",input.Name},{"Reference",input.Reference},{"Issuer",input.Issuer},{"Number",input.Number},{"Scope",input.Scope},{"SourceUrl",input.SourceUrl},{"Notes",input.Notes},{"ValidFrom",start?.ToString("yyyy-MM-dd")??""},{"ValidUntil",end?.ToString("yyyy-MM-dd")??""},{"Consent",input.Consent?"true":"false"}})form.Add(new StringContent(pair.Value),pair.Key);
            if(bytes is not null)form.Add(new ByteArrayContent(bytes),"File",fileName);
            using var request=new HttpRequestMessage(HttpMethod.Post,"api/certificates/submissions"){Content=form};request.Headers.Add(af!.Header,af.Token);
            using var response=await client.SendAsync(request,lifetime.Token);var result=await response.Content.ReadFromJsonAsync<SubmitResult>(lifetime.Token);status.Text=result?.Message??"Gönderim işlenemedi.";
            if(response.IsSuccessStatusCode){saved=true;status.Text+="\nGönderim kimliği: "+result?.Id;bytes=null;preview.Source=null;preview.IsVisible=false;}
        }catch(OperationCanceledException){status.Text="Gönderim kesildi. Aynı form ile tekrar deneyebilirsiniz.";}catch(Exception){status.Text="Sunucuya ulaşılamadı. Girdileriniz korunuyor; tekrar deneyin.";}finally{busy=false;}
    }
    private sealed record TokenResult(string Token,string Header);
    private sealed record SubmitResult(string Message,Guid? Id);
}
