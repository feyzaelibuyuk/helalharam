using HelalHaram.Shared;
namespace helalharam;
public sealed class PrivacyPage : ContentPage
{
    public PrivacyPage()
    {
        Title = "Gizlilik ve kullanım sınırları";
        var stack = new VerticalStackLayout { Padding = 22, Spacing = 16 };
        var close = new Button { Text = "Kapat" };
        close.Clicked += async (_, _) => await Navigation.PopModalAsync();
        stack.Add(close);
        foreach (var section in PrivacyNotice.Sections)
        {
            stack.Add(new Label { Text = section.Title, FontSize = 22, FontAttributes = FontAttributes.Bold });
            stack.Add(new Label { Text = section.Text, FontSize = 16 });
        }
        Content = new ScrollView { Content = stack };
    }
}
