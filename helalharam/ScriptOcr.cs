#if ANDROID
using Android.Gms.Extensions;
#endif
namespace helalharam;

internal static class ScriptOcr
{
    public static readonly (string Code, string Label)[] Scripts = [
        ("latin", "Latin (Türkçe, İngilizce, İtalyanca…)"), ("chinese", "Çince · 中文"),
        ("japanese", "Japonca · 日本語"), ("korean", "Korece · 한국어"),
        ("devanagari", "Devanagari · हिन्दी"), ("unsupported", "Arapça / Kiril / diğer — henüz desteklenmiyor")];
    public static async Task<string> ReadAsync(byte[] bytes, string script, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (script == "unsupported") throw new NotSupportedException("Bu alfabe cihazda okunamıyor. Metni elle girebilirsiniz; OCR sonucu çeviri veya uygunluk onayı değildir.");
#if ANDROID
        return await ReadAndroidAsync(bytes, script, token);
#else
        if(script != "latin") throw new NotSupportedException("Bu platformda yeni OCR modelleri henüz doğrulanmadı. Metni elle girin.");
        await Plugin.Maui.OCR.OcrPlugin.Default.InitAsync(token);
        var result = await Plugin.Maui.OCR.OcrPlugin.Default.RecognizeTextAsync(bytes, false, token);
        token.ThrowIfCancellationRequested();
        return result.Success ? result.AllText : "";
#endif
    }
#if ANDROID
    private static async Task<string> ReadAndroidAsync(byte[] bytes, string script, CancellationToken token)
    {
        using var bitmap = await Android.Graphics.BitmapFactory.DecodeByteArrayAsync(bytes, 0, bytes.Length)
            ?? throw new InvalidDataException("Görüntü çözümlenemedi.");
        using var image = Xamarin.Google.MLKit.Vision.Common.InputImage.FromBitmap(bitmap, 0);
        using var recognizer = script switch
        {
            "latin" => Xamarin.Google.MLKit.Vision.Text.TextRecognition.GetClient(Xamarin.Google.MLKit.Vision.Text.Latin.TextRecognizerOptions.DefaultOptions),
            "chinese" => Xamarin.Google.MLKit.Vision.Text.TextRecognition.GetClient(new Xamarin.Google.MLKit.Vision.Text.Chinese.ChineseTextRecognizerOptions.Builder().Build()),
            "japanese" => Xamarin.Google.MLKit.Vision.Text.TextRecognition.GetClient(new Xamarin.Google.MLKit.Vision.Text.Japanese.JapaneseTextRecognizerOptions.Builder().Build()),
            "korean" => Xamarin.Google.MLKit.Vision.Text.TextRecognition.GetClient(new Xamarin.Google.MLKit.Vision.Text.Korean.KoreanTextRecognizerOptions.Builder().Build()),
            "devanagari" => Xamarin.Google.MLKit.Vision.Text.TextRecognition.GetClient(new Xamarin.Google.MLKit.Vision.Text.Devanagari.DevanagariTextRecognizerOptions.Builder().Build()),
            _ => throw new NotSupportedException("OCR alfabesi desteklenmiyor.")
        };
        // Keep native image/recognizer alive until processing ends, even if the page closes.
        try
        {
            token.ThrowIfCancellationRequested();
            using var task = recognizer.Process(image);
            using var result = await task.AsAsync<Xamarin.Google.MLKit.Vision.Text.Text>();
            token.ThrowIfCancellationRequested();
            return result.GetText() ?? "";
        }
        finally { recognizer.Close(); }
    }
#endif
}
