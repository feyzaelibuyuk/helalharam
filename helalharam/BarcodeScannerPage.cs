using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace helalharam;

public sealed class BarcodeScannerPage : ContentPage
{
    private readonly HttpClient client;
    private readonly VerticalStackLayout body = new() { Padding = 20, Spacing = 14 };
    private readonly Label status = new() { Text = "Kamera hazırlanıyor…", TextColor = Color.FromArgb("#20312C") };
    private readonly Entry manual = new() { Placeholder = "Barkodu elle gir", Keyboard = Keyboard.Numeric, MaxLength = 14 };
    private CameraBarcodeReaderView? reader;
    private bool active;
    private bool opening;
    private int generation;

    public BarcodeScannerPage(HttpClient httpClient)
    {
        client = httpClient; Title = "Barkod tara";
        BackgroundColor = Color.FromArgb("#F8F8F4");
        body.Add(new Label { Text = "Ambalaj barkodunu kameraya göster", FontSize = 22, TextColor = Color.FromArgb("#245342") });
        body.Add(status); body.Add(manual);
        var search = new Button { Text = "Barkodla ürünü getir", BackgroundColor = Color.FromArgb("#245342"), TextColor = Colors.White };
        search.Clicked += async (_, _) => await OpenAsync(manual.Text);
        body.Add(search);
        var retry = new Button { Text = "Kamerayı tekrar aç" }; retry.Clicked += async (_, _) => await StartCameraAsync(); body.Add(retry);
        body.Add(new Label { Text = "Kamera görüntüsü cihazda işlenir. OFF servisine yalnız barkod gönderilir. Emülatörde elle giriş seçeneğini de kullanabilirsiniz.", FontSize = 13 });
        Content = new ScrollView { Content = body };
        ToolbarItems.Add(new ToolbarItem("Kapat", null, async () => await Navigation.PopModalAsync()));
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); active = true; opening = false;
        await StartCameraAsync();
    }
    protected override void OnDisappearing()
    {
        active = false; generation++; StopCamera(); base.OnDisappearing();
    }
    private async Task StartCameraAsync()
    {
        var current = ++generation;
        StopCamera();
        try
        {
            if (!BarcodeScanning.IsSupported) { status.Text = "Bu cihazda kamera taraması desteklenmiyor. Barkodu elle girebilirsiniz."; return; }
            var permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permission != PermissionStatus.Granted) permission = await Permissions.RequestAsync<Permissions.Camera>();
            if (!active || current != generation) return;
            if (permission != PermissionStatus.Granted) { status.Text = "Kamera izni verilmedi. Elle giriş kullanılabilir; izni cihaz ayarlarından değiştirebilirsiniz."; return; }
            reader = new CameraBarcodeReaderView
            {
                HeightRequest = 320, IsDetecting = true,
                Options = new BarcodeReaderOptions { Formats = BarcodeFormats.OneDimensional, AutoRotate = true, Multiple = false }
            };
            reader.BarcodesDetected += OnDetected;
            body.Insert(2, reader);
            status.Text = "Barkodu çerçeveye alın. Okununca ürün bilgisi açılır.";
        }
        catch (Exception ex) when (ex is PermissionException or FeatureNotSupportedException or InvalidOperationException)
        { StopCamera(); status.Text = "Kamera başlatılamadı. Tekrar deneyin veya barkodu elle girin."; }
    }
    private void OnDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var current = generation;
        var code = e.Results.FirstOrDefault(r => ProductBarcode.IsValid(r.Value))?.Value;
        if (code is null) return;
        MainThread.BeginInvokeOnMainThread(async () => { if (active && !opening && current == generation && ReferenceEquals(sender, reader)) await OpenAsync(code); });
    }
    private async Task OpenAsync(string? value)
    {
        if (opening || !active) return;
        var code = value?.Trim() ?? "";
        if (!ProductBarcode.IsValid(code)) { status.Text = "8, 12, 13 veya 14 rakamlı ürün barkodu girin."; return; }
        opening = true; StopCamera();
        try { await Navigation.PushAsync(new OffProductPage(code, client)); }
        catch (InvalidOperationException) { opening = false; status.Text = "Ürün ekranı açılamadı. Tekrar deneyin."; }
    }
    private void StopCamera()
    {
        if (reader is null) return;
        reader.IsDetecting = false; reader.BarcodesDetected -= OnDetected;
        reader.Handler?.DisconnectHandler(); body.Remove(reader); reader = null;
    }
}
