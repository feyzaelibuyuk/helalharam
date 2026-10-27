using HelalRehber.Shared;
namespace helalharam;
internal static class DeviceFoodPreferences
{
    public const string Key="food-preferences-v1";
    private static long revision;
    public static long Revision=>Interlocked.Read(ref revision);
    public static void Changed()=>Interlocked.Increment(ref revision);
    public static async Task AppendAsync(Page page,VerticalStackLayout body,string? text,string? language,string[]? allergens=null,string[]? traces=null)
    {
        var panel=new VerticalStackLayout {Spacing=10,FlowDirection=FlowDirection.LeftToRight};
        body.Add(panel);
        long displayedRevision=-1;var generation=0;
        async Task RefreshAsync()
        {
            var currentGeneration=++generation;var currentRevision=Revision;
            // Clear old warnings immediately, even if secure storage subsequently fails.
            panel.Clear();panel.Add(new Label {Text="Kişisel alerjen ve diyet kontrolü",FontSize=21,FontAttributes=FontAttributes.Bold});
            panel.Add(new Label {Text="Güncel tercihler kontrol ediliyor…"});
            FoodPreferenceResult? result=null;string? failure=null;
            try {var preferences=FoodPreferenceRules.Read(await SecureStorage.Default.GetAsync(Key));result=FoodPreferenceRules.Evaluate(preferences,text,language,allergens,traces);}
            catch(Exception){failure="Cihazdaki kişisel tercihler okunamadı; kişisel kontrol uygulanamadı. Tercihler ekranını kontrol edin.";}
            if(panel.Parent is null||generation!=currentGeneration)return;
            if(currentRevision!=Revision){await RefreshAsync();return;}
            panel.Clear();panel.Add(new Label {Text="Kişisel alerjen ve diyet kontrolü",FontSize=21,FontAttributes=FontAttributes.Bold});
            if(result is not null){
                panel.Add(new Label {Text=result.Summary});
                foreach(var finding in result.Findings)panel.Add(new Label {Text=finding.Item+" · "+finding.Context+"\n"+finding.Match,BackgroundColor=Color.FromArgb("#FFF0DD"),Padding=12});
                panel.Add(new Label {Text=result.Coverage,FontSize=13});
            }else panel.Add(new Label {Text=failure});
            panel.Add(new Label {Text=FoodPreferenceRules.Limits,FontSize=13});
            var edit=new Button {Text="Alerjen / diyet tercihlerimi düzenle"};edit.Clicked+=async(_,_)=>await page.Navigation.PushAsync(new FoodPreferencesPage());panel.Add(edit);
            displayedRevision=result is null?-1:currentRevision;
        }
        EventHandler appearing=async(_,_)=>{if(panel.Parent is not null&&displayedRevision!=Revision)await RefreshAsync();};
        page.Appearing+=appearing;
        panel.ParentChanged+=(_,_)=>{if(panel.Parent is null){generation++;page.Appearing-=appearing;}};
        await RefreshAsync();
    }
}
