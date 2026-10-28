namespace helalharam;
// The moderated community UI is shared through a WebView; the existing device-only draft remains separate.
public sealed class CommunityPage:ContentPage
{
#if ANDROID
    private const string BaseUrl="http://10.0.2.2:5088/";
#else
    private const string BaseUrl="http://localhost:5088/";
#endif
    public CommunityPage(bool notifications = false)
    {
        Title=notifications ? Ui.T("notifications") : "Topluluk deneyimleri";var web=new WebView();var status=new Label();
        var header=new VerticalStackLayout {Padding=12,Spacing=8};
        header.Add(new Label {Text="İncelenen kullanıcı paylaşımları ve oylar. Bilgisayardaki web/API sunucusu 5088 portunda açık olmalı. Katılımcı kimliği bu web görünümünün çerezinde saklanır; hesap eşitlemesi yoktur. Paylaşımlar helal/sertifika onayı değildir."});
        var close=new Button {Text=Ui.T("close")};close.Clicked+=async(_,_)=>await Navigation.PopModalAsync();header.Add(close);header.Add(status);
        var retry = new Button { Text="Yeniden dene", IsVisible=false };
        retry.Clicked += (_,_) => { status.Text="Bağlanıyor…"; retry.IsVisible=false; web.Source=BaseUrl+(notifications ? "community/notifications" : "community"); };
        header.Add(retry);
        var grid=new Grid {RowDefinitions={new RowDefinition(GridLength.Auto),new RowDefinition(GridLength.Star)}};grid.Add(header);grid.Add(web,0,1);Content=grid;
        web.Navigating+=async(_,e)=>{if(Uri.TryCreate(e.Url,UriKind.Absolute,out var uri)&&uri.AbsoluteUri.StartsWith(BaseUrl,StringComparison.Ordinal))return;e.Cancel=true;if(uri is {Scheme:"https"})try{await Browser.Default.OpenAsync(uri,BrowserLaunchMode.SystemPreferred);}catch{status.Text="Dış bağlantı açılamadı.";}};
        web.Navigated+=(_,e)=>{
            var failed=e.Result!=WebNavigationResult.Success;
            retry.IsVisible=failed;
            status.Text=failed ? "Ekran açılamadı. İnternet/yerel sunucu bağlantısını kontrol edip yeniden deneyin." : "";
        };web.Source=BaseUrl+(notifications ? "community/notifications" : "community");
    }
}
