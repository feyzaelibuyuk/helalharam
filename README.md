# Helal mi?

Android + web MVP başlangıcı. Çözüm `helalharam.slnx` içindedir.

## Projeler

- `helalharam`: Android istemcisi (.NET MAUI, C#).
- `BlazorApp1`: Web arayüzü ve C# API. SQLite veritabanını ilk çalıştırmada oluşturur.

## Geliştirme

1. Visual Studio'da `helalharam.slnx` çözümünü aç.
2. NuGet paketlerini geri yükle.
3. Önce `BlazorApp1` projesini çalıştır. Web: `http://localhost:5088`; API: `/api/products` ve `/api/restaurants`.
4. Android emülatörünü başlat; sonra `helalharam` projesini Android hedefiyle çalıştır. Emülatör, API'ye `10.0.2.2:5088` üzerinden erişir.

Başlangıçtaki ürün ve restoran kayıtları yalnızca arayüz/API denemesi içindir; gerçek ürün veya işletme bilgisi değildir. Uygulama kesin dini hüküm vermez.
