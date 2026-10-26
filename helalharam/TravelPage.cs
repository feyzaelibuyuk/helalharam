using System.Globalization;
namespace helalharam;

// Shared responsive C# web screen inside MAUI; location permission remains native.
public sealed class TravelPage : ContentPage
{
#if ANDROID
    private const string BaseUrl="http://10.0.2.2:5088/";
#else
    private const string BaseUrl="http://localhost:5088/";
#endif
    private readonly WebView web=new();
    private readonly Label status=new();
    private CancellationTokenSource lifetime=new();
    private bool locating;
    public TravelPage()
    {
        Title="Harita ve seyahat";
        var locate=new Button { Text="Cihaz konumumu forma getir" };
        locate.Clicked+=async(_,_)=>await LocateAsync();
        var close=new Button { Text=Ui.T("close") };close.Clicked+=async(_,_)=>await Navigation.PopModalAsync();
        var header=new VerticalStackLayout { Padding=12,Spacing=8 };
        header.Add(new Label { Text="Seyahat ekranı yerel web/API sunucusunu kullanır. Konum yalnız düğmeyle alınır; bölge aramasını ekrandan ayrıca onaylayın. Harita için internet gerekir." });
        header.Add(locate);header.Add(status);header.Add(close);
        var grid=new Grid { RowDefinitions={new RowDefinition(GridLength.Auto),new RowDefinition(GridLength.Star)} };
        grid.Add(header);grid.Add(web,0,1);Content=grid;
        web.Navigating+=async(_,e)=>{
            if(Uri.TryCreate(e.Url,UriKind.Absolute,out var uri)&&uri.AbsoluteUri.StartsWith(BaseUrl,StringComparison.Ordinal))return;
            e.Cancel=true;
            if(uri is {Scheme:"https"})try{await Browser.Default.OpenAsync(uri,BrowserLaunchMode.SystemPreferred);}catch{status.Text="Dış bağlantı açılamadı.";}
        };
        web.Navigated+=(_,e)=>{if(e.Result!=WebNavigationResult.Success)status.Text="Seyahat ekranı açılamadı. Bilgisayardaki web/API sunucusunun 5088 portunda açık olduğunu kontrol edin.";};
#if ANDROID
        web.HandlerChanged+=(_,_)=>{if(web.Handler?.PlatformView is Android.Webkit.WebView native)native.Settings.UserAgentString+=" HelalRehber-StudentDemo/0.1";};
#endif
        web.Source=BaseUrl+"travel";
    }
    protected override void OnAppearing(){base.OnAppearing();if(lifetime.IsCancellationRequested){lifetime.Dispose();lifetime=new();}}
    protected override void OnDisappearing(){lifetime.Cancel();base.OnDisappearing();}
    private async Task LocateAsync()
    {
        if(locating)return;locating=true;var token=lifetime.Token;
        try
        {
            status.Text="Konum izni bekleniyor…";
            var location=await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Low,TimeSpan.FromSeconds(15)),token);
            token.ThrowIfCancellationRequested();
            if(location is null){status.Text="Konum alınamadı. Şehir ya da koordinat kullanın.";return;}
            var lat=HelalRehber.Shared.TravelRules.Coarse(location.Latitude).ToString(CultureInfo.InvariantCulture);
            var lon=HelalRehber.Shared.TravelRules.Coarse(location.Longitude).ToString(CultureInfo.InvariantCulture);
            var filled=await web.EvaluateJavaScriptAsync($"window.helalTravelSetPosition && window.helalTravelSetPosition({lat},{lon})");
            if(!token.IsCancellationRequested)status.Text=filled=="true"?"Yaklaşık konum forma geldi. Onayı verip Bu bölgede ara düğmesine basın.":"Önce seyahat arama ekranını açın, sonra tekrar deneyin.";
        }
        catch(OperationCanceledException){ }
        catch(PermissionException){status.Text="Konum izni verilmedi. Şehir veya koordinat kullanabilirsiniz.";}
        catch(Exception){if(!token.IsCancellationRequested)status.Text="Konum kullanılamıyor. Cihazın konum ayarlarını kontrol edin veya şehir seçin.";}
        finally{locating=false;}
    }
}
