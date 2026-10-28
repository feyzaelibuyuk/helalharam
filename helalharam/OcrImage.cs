namespace helalharam;

internal static class OcrImage
{
    public const int MaxBytes = 16 * 1024 * 1024;
    public static async Task<byte[]> ReadAsync(FileResult file, CancellationToken token)
    {
        using var source = await file.OpenReadAsync();
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int count;
        while ((count = await source.ReadAsync(chunk, token)) > 0)
        {
            if (buffer.Length + count > MaxBytes) throw new InvalidDataException("Fotoğraf 16 MB sınırını aşıyor. Daha küçük bir fotoğraf seçin.");
            buffer.Write(chunk, 0, count);
        }
        if (buffer.Length == 0) throw new InvalidDataException("Fotoğraf boş veya okunamadı.");
        var data = buffer.ToArray();
#if ANDROID
        // Decode dimensions first to avoid allocating the full-resolution bitmap.
        var bounds = new Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
        Android.Graphics.BitmapFactory.DecodeByteArray(data, 0, data.Length, bounds);
        if (bounds.OutWidth <= 0 || bounds.OutHeight <= 0 || (long)bounds.OutWidth * bounds.OutHeight > 100_000_000)
            throw new InvalidDataException("Desteklenmeyen veya aşırı büyük görüntü. JPG/PNG seçin.");
        var sample = 1;
        while (Math.Max(bounds.OutWidth, bounds.OutHeight) / sample > 2048) sample *= 2;
        using var bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(data, 0, data.Length,
            new Android.Graphics.BitmapFactory.Options { InSampleSize = sample })
            ?? throw new InvalidDataException("Görüntü çözümlenemedi.");
        // EXIF orientation is retained in the source bytes; normalize it before OCR.
        var temporary = Path.Combine(FileSystem.CacheDirectory, $"ocr-{Guid.NewGuid():N}.img");
        try
        {
            await File.WriteAllBytesAsync(temporary, data, token);
            using var exif = new Android.Media.ExifInterface(temporary);
            var orientation = exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 1);
            using var matrix = new Android.Graphics.Matrix();
            switch (orientation)
            {
                case 2: matrix.SetScale(-1, 1); break;
                case 3: matrix.SetRotate(180); break;
                case 4: matrix.SetScale(1, -1); break;
                case 5: matrix.SetRotate(90); matrix.PostScale(-1, 1); break;
                case 6: matrix.SetRotate(90); break;
                case 7: matrix.SetRotate(-90); matrix.PostScale(-1, 1); break;
                case 8: matrix.SetRotate(-90); break;
            }
            using var output = new MemoryStream();
            if (orientation is >= 2 and <= 8)
            {
                using var rotated = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                rotated!.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg!, 95, output);
            }
            else bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg!, 95, output);
            return output.ToArray();
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
#else
        return data;
#endif
    }
    internal static Task<byte[]> EnhanceContrastAsync(byte[] normalized, CancellationToken token) => Task.Run<byte[]>(() =>
    {
#if ANDROID
        var bounds = new Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
        Android.Graphics.BitmapFactory.DecodeByteArray(normalized, 0, normalized.Length, bounds);
        if(bounds.OutWidth <= 0 || bounds.OutHeight <= 0 || (long)bounds.OutWidth * bounds.OutHeight > 2048L * 2048)
            throw new InvalidDataException("Kontrast işlemi için normalize edilmiş fotoğraf gerekir.");
        using var bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(normalized, 0, normalized.Length)
            ?? throw new InvalidDataException("Görüntü okunamadı.");
        var pixels = new int[bitmap.Width * bitmap.Height];
        bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
        var histogram = new int[256];
        for(int i=0;i<pixels.Length;i++)
        {
            if((i & 65535)==0) token.ThrowIfCancellationRequested();
            var c=pixels[i]; int gray=(77*((c>>16)&255)+150*((c>>8)&255)+29*(c&255))>>8;
            pixels[i]=gray; histogram[gray]++;
        }
        int low=0,high=255,count=0,cut=Math.Max(1,pixels.Length/200);
        while(low<255 && count+histogram[low]<cut) count+=histogram[low++];
        count=0;while(high>0 && count+histogram[high]<cut) count+=histogram[high--];
        if(high-low<8) return normalized; // Avoid amplifying almost uniform/blank images.
        for(int i=0;i<pixels.Length;i++)
        {
            if((i & 65535)==0) token.ThrowIfCancellationRequested();
            int gray=Math.Clamp((pixels[i]-low)*255/(high-low),0,255);
            pixels[i]=unchecked((int)0xff000000)|(gray<<16)|(gray<<8)|gray;
        }
        using var adjusted=Android.Graphics.Bitmap.CreateBitmap(pixels,bitmap.Width,bitmap.Height,Android.Graphics.Bitmap.Config.Argb8888!)!;
        using var output=new MemoryStream();adjusted.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg!,95,output);
        return output.ToArray();
#else
        throw new NotSupportedException("Kontrast iyileştirme bu platformda henüz desteklenmiyor.");
#endif
    }, token);

}
