#if ANDROID && DEBUG
using Android.Content;
using Android.Graphics;
using Path = System.IO.Path;
using Paint = Android.Graphics.Paint;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace helalharam.Tests;

// Opt-in separate QA package only. Generates synthetic labels; never opens user photos.
[BroadcastReceiver(Enabled=true,Exported=true)]
[Android.App.IntentFilter(new[]{"com.companyname.helalharam.OCR_QUALITY"})]
public sealed class OcrQualityReceiver : BroadcastReceiver
{
    private static readonly SemaphoreSlim Gate=new(1,1);
    public override void OnReceive(Context? context,Intent? intent)
    {
        int index=intent?.GetIntExtra("case",-1)??-1;
        if(index<0||index>=Cases.Length)return;
        string run=intent?.GetStringExtra("run")??"";
        if(!Guid.TryParse(run,out _))return;
        var pending=GoAsync();
        _=Task.Run(async()=>{
            await Gate.WaitAsync();
            string path=Path.Combine(FileSystem.CacheDirectory,$"quality-{index}.jpg");
            try {
                var c=Cases[index];
                string expected=c.Turkish ? "İçindekiler: süt, buğday unu, yer fıstığı.\nJelatin E471, tuz, şeker." : "Ingredients: wheat flour, MILK, peanuts.\nGelatin E471, salt, sugar.";
                using var bitmap=Bitmap.CreateBitmap(1800,500,Bitmap.Config.Argb8888!)!;
                using(var canvas=new Canvas(bitmap)) {
                    canvas.DrawColor(new Android.Graphics.Color(c.Background,c.Background,c.Background));
                    canvas.Rotate(c.Angle,900,250);
                    using var paint=new Paint{Color=new Android.Graphics.Color(c.Foreground,c.Foreground,c.Foreground),TextSize=c.FontSize,AntiAlias=true};
                    if(!c.Blank){canvas.DrawText(expected.Split('\n')[0],50,150,paint);canvas.DrawText(expected.Split('\n')[1],50,280,paint);}
                }
                if(c.Noise>0){
                    var pixels=new int[bitmap.Width*bitmap.Height];bitmap.GetPixels(pixels,0,bitmap.Width,0,0,bitmap.Width,bitmap.Height);
                    var random=new Random(2400+index);
                    for(int i=0;i<pixels.Length;i++){int v=(pixels[i]>>16)&255;v=Math.Clamp(v+random.Next(-c.Noise,c.Noise+1),0,255);pixels[i]=unchecked((int)0xff000000)|(v<<16)|(v<<8)|v;}
                    bitmap.SetPixels(pixels,0,bitmap.Width,0,0,bitmap.Width,bitmap.Height);
                }
                if(c.Glare){using var canvas=new Canvas(bitmap);using var paint=new Paint{Color=Android.Graphics.Color.White};canvas.DrawOval(500,70,1450,210,paint);}
                using var output=new MemoryStream();
                if(c.Blur>1){
                    using var small=Bitmap.CreateScaledBitmap(bitmap,1800/c.Blur,500/c.Blur,true)!;
                    using var blurred=Bitmap.CreateScaledBitmap(small,1800,500,true)!;
                    blurred.Compress(Bitmap.CompressFormat.Jpeg!,c.Jpeg,output);
                }else if(c.Crop){using var cropped=Bitmap.CreateBitmap(bitmap,0,0,800,500)!;cropped.Compress(Bitmap.CompressFormat.Jpeg!,c.Jpeg,output);}
                else bitmap.Compress(Bitmap.CompressFormat.Jpeg!,c.Jpeg,output);
                await File.WriteAllBytesAsync(path,output.ToArray());
                var bytes=await OcrImage.ReadAsync(new FileResult(path),CancellationToken.None);
                var clock=System.Diagnostics.Stopwatch.StartNew();
                var actual=await ScriptOcr.ReadAsync(bytes,"latin",CancellationToken.None);clock.Stop();
                var normalized=Normalize(actual);var reference=c.Blank ? "" : Normalize(expected);
                string[] critical=c.Turkish?["sut","bugday","yer fistigi","jelatin","e471"]:["wheat","milk","peanuts","gelatin","e471"];
                var missing=c.Blank?Array.Empty<string>():critical.Where(x=>!Regex.IsMatch(normalized,"(?<![a-z0-9])"+Regex.Escape(x)+"(?![a-z0-9])")).ToArray();
                var enhanced=await OcrImage.EnhanceContrastAsync(bytes,CancellationToken.None);
                var contrastActual=await ScriptOcr.ReadAsync(enhanced,"latin",CancellationToken.None);
                var contrastNormalized=Normalize(contrastActual);
                var contrastMissing=c.Blank?Array.Empty<string>():critical.Where(x=>!Regex.IsMatch(contrastNormalized,"(?<![a-z0-9])"+Regex.Escape(x)+"(?![a-z0-9])")).ToArray();
                var contrastError=reference.Length==0?(contrastNormalized.Length==0?0d:1d):(double)Distance(reference,contrastNormalized)/reference.Length;
                var report=new {ContrastRecognized=contrastActual,ContrastMissingCritical=contrastMissing,ContrastNormalizedCharacterErrorRate=contrastError,Run=run,Index=index,c.Name,Expected=c.Blank?"":expected,Recognized=actual,NormalizedCharacterErrorRate=reference.Length==0?(normalized.Length==0?0d:1d):(double)Distance(reference,normalized)/reference.Length,MissingCritical=missing,ExactNormalized=normalized==reference,Seconds=clock.Elapsed.TotalSeconds,Parameters=c,Scope="Synthetic image degradation, not lux measurement or real camera accuracy"};
                string json=JsonSerializer.Serialize(report);
                await File.WriteAllTextAsync(Path.Combine(FileSystem.AppDataDirectory,$"ocr-quality-{index}.json"),json);
                Android.Util.Log.Info("OcrQuality","DONE "+index+" "+c.Name);
            }catch(Exception ex){await File.WriteAllTextAsync(Path.Combine(FileSystem.AppDataDirectory,$"ocr-quality-{index}.json"),JsonSerializer.Serialize(new{Run=run,Index=index,Error=ex.GetType().Name}));Android.Util.Log.Error("OcrQuality","ERROR "+index+" "+ex.GetType().Name);}
            finally{if(File.Exists(path))File.Delete(path);Gate.Release();pending?.Finish();}
        });
    }
    private static string Normalize(string text){var b=new StringBuilder();foreach(var c in text.Normalize(NormalizationForm.FormD))if(CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)b.Append(c);return Regex.Replace(b.ToString().ToLowerInvariant().Replace('ı','i'),"[^a-z0-9]+"," ").Trim();}
    private static int Distance(string a,string b){var row=Enumerable.Range(0,b.Length+1).ToArray();for(int i=1;i<=a.Length;i++){int prev=row[0];row[0]=i;for(int j=1;j<=b.Length;j++){int old=row[j];row[j]=Math.Min(Math.Min(row[j]+1,row[j-1]+1),prev+(a[i-1]==b[j-1]?0:1));prev=old;}}return row[b.Length];}
    private sealed record Condition(string Name,int Background=245,int Foreground=20,int Noise=0,int Blur=1,float Angle=0,float FontSize=64,int Jpeg=95,bool Glare=false,bool Crop=false,bool Blank=false,bool Turkish=false);
    private static readonly Condition[] Cases=[
        new("clear"),new("dim",70,8,5),new("very_dim_noisy",24,8,12),new("faint_print",245,215,4),
        new("moderate_blur",Blur:4),new("severe_blur",Blur:12),new("tilt_12",Angle:12),new("tilt_25",Angle:25),
        new("glare",Glare:true),new("cropped",Crop:true),new("tiny_print",FontSize:22),new("jpeg_low",Jpeg:15),
        new("blank",Blank:true),new("turkish_clear",Turkish:true),new("combined_dim_blur",65,20,8,6)
    ];
}
#endif
