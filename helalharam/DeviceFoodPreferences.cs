using System.Text.Json;
using HelalRehber.Shared;
namespace helalharam;
internal static class DeviceFoodPreferences
{
    public const string Key="food-preferences-v1";
    public static async Task AppendAsync(Page page,VerticalStackLayout body,string? text,string? language,string[]? allergens=null,string[]? traces=null)
    {
        body.Add(new Label {Text="Kişisel alerjen ve diyet kontrolü",FontSize=21,FontAttributes=FontAttributes.Bold});
        try {
            var preferences=FoodPreferenceRules.Read(await SecureStorage.Default.GetAsync(Key));
            var result=FoodPreferenceRules.Evaluate(preferences,text,language,allergens,traces);
            body.Add(new Label {Text=result.Summary});
            foreach(var finding in result.Findings)body.Add(new Label {Text=finding.Item+" · "+finding.Context+"\n"+finding.Match,BackgroundColor=Color.FromArgb("#FFF0DD"),Padding=12});
            body.Add(new Label {Text=result.Coverage,FontSize=13});
        }catch(Exception){body.Add(new Label {Text="Cihazdaki kişisel tercihler okunamadı; kişisel kontrol uygulanamadı. Tercihler ekranını kontrol edin."});}
        body.Add(new Label {Text=FoodPreferenceRules.Limits,FontSize=13});
        var edit=new Button {Text="Alerjen / diyet tercihlerimi düzenle"};edit.Clicked+=async(_,_)=>await page.Navigation.PushAsync(new FoodPreferencesPage());body.Add(edit);
    }
}
