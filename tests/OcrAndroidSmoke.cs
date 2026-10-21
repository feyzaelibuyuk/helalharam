#if ANDROID && DEBUG
using Android.Content;
using Android.Graphics;
using Plugin.Maui.OCR;

namespace helalharam.Tests;

// Debug-only native OCR integration fixture. No user photos or app data are accessed.
[BroadcastReceiver(Enabled = true, Exported = true)]
[Android.App.IntentFilter(new[] { "com.companyname.helalharam.OCR_SMOKE", "com.companyname.helalharam.OCR_SCRIPT_SMOKE" })]
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
                if(intent?.Action == "com.companyname.helalharam.OCR_SCRIPT_SMOKE")
                {
                    foreach(var fixture in new[] { ("latin", "Ingredients gelatin E471", "gelatin"), ("chinese", "食品 成分 牛奶", "牛奶"), ("japanese", "食品 原材料 牛乳", "牛乳"), ("korean", "식품 원재료 우유", "우유"), ("devanagari", "सामग्री दूध नमक", "दूध") })
                    {
                        using var picture = Bitmap.CreateBitmap(1800, 400, Bitmap.Config.Argb8888!)!;
                        using var drawing = new Canvas(picture); drawing.DrawColor(Android.Graphics.Color.White);
                        using var brush = new Android.Graphics.Paint { Color = Android.Graphics.Color.Black, TextSize = 90, AntiAlias = true };
                        drawing.DrawText(fixture.Item2, 40, 180, brush);
                        using var buffer = new MemoryStream(); picture.Compress(Bitmap.CompressFormat.Png!, 100, buffer);
                        var actual = await ScriptOcr.ReadAsync(buffer.ToArray(), fixture.Item1, CancellationToken.None);
                        if(!actual.Contains(fixture.Item3, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException(fixture.Item1 + " mismatch: " + actual);
                        Android.Util.Log.Info("OcrSmoke", "PASS script: " + fixture.Item1);
                    }
                    return;
                }
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
