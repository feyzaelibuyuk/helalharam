using HelalRehber.Shared;
namespace BlazorApp1.Services;
public sealed class WebUi
{
    public const string CookieName = "helal-ui-language";
    public string Language { get; }
    public string Direction => UiCatalog.IsRtl(Language) ? "rtl" : "ltr";
    public WebUi(IHttpContextAccessor context) => Language = UiCatalog.Normalize(context.HttpContext?.Request.Cookies[CookieName]);
    public string T(string key) => UiCatalog.Text(key, Language);
    public string F(string key, params object[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture, T(key), args);
}
