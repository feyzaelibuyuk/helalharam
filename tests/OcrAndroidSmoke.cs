#if ANDROID && DEBUG
using Android.Content;
using Android.Graphics;
using Plugin.Maui.OCR;

namespace helalharam.Tests;

// Debug-only native OCR integration fixture. No user photos or app data are accessed.
[BroadcastReceiver(Enabled = true, Exported = true)]
[Android.App.IntentFilter(new[] { "com.companyname.helalharam.OCR_SMOKE" })]
public sealed class OcrSmokeReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        var pending = GoAsync();
        _ = Task.Run(async () =>
        {
            var path = System.IO.Path.Combine(FileSystem.CacheDirectory, $"ocr-fixture-{Guid.NewGuid():N}.jpg");
            try
            {
                using var bitmap = Bitmap.CreateBitmap(1600, 600, Bitmap.Config.Argb8888!)!;
                using var canvas = new Canvas(bitmap);
                canvas.DrawColor(Android.Graphics.Color.White);
                using var paint = new Android.Graphics.Paint { Color = Android.Graphics.Color.Black, TextSize = 64, AntiAlias = true };
                canvas.DrawText("Ingredients: wheat flour", 50, 120, paint);
                canvas.DrawText("gelatin, E471, salt", 50, 240, paint);
                using (var file = File.Create(path)) bitmap.Compress(Bitmap.CompressFormat.Jpeg!, 95, file);
                var bytes = await OcrImage.ReadAsync(new FileResult(path), CancellationToken.None);
                var result = await OcrPlugin.Default.RecognizeTextAsync(bytes);
                if (!result.Success || !result.AllText.Contains("gelatin", StringComparison.OrdinalIgnoreCase) || !result.AllText.Contains("E471"))
                    throw new InvalidOperationException("Fixture text mismatch: " + result.AllText);
                Android.Util.Log.Info("OcrSmoke", "PASS: native Android OCR recognized gelatin and E471 from generated photo.");
            }
            catch (Exception ex) { Android.Util.Log.Error("OcrSmoke", "FAIL: " + ex); }
            finally { if (File.Exists(path)) File.Delete(path); pending?.Finish(); }
        });
    }
}
#endif
