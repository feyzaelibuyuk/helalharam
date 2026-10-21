using HelalRehber.Shared;
using Microsoft.Maui.Controls.Xaml;
namespace helalharam;
internal static class Ui
{
    public static string Language => UiCatalog.Normalize(Preferences.Default.Get("ui-language", "tr"));
    public static string T(string key) => UiCatalog.Text(key, Language);
    public static string F(string key, params object[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture, T(key), args);
    public static FlowDirection Direction => UiCatalog.IsRtl(Language) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
}
[ContentProperty(nameof(Key))]
public sealed class TrExtension : IMarkupExtension<string>
{
    public string Key { get; set; } = "";
    public string ProvideValue(IServiceProvider serviceProvider) => Ui.T(Key);
    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
