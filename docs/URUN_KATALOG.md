# Ürün kataloğu — kullanım ve kontroller

## Tamamlananlar

- Web ve MAUI Katalog sekmesinden ürün detayına erişim.
- İçerik, gerekçe, kategori, barkod, kaynak bağlantısı ve kontrol tarihi.
- Satış ülkesi koduyla filtreleme; ülke barkodun ilk rakamlarından tahmin edilmez.
- İçerik dili ve özgün etiket metninin ayrı saklanması.
- Aynı kategori/ülke için alternatif listesi ve alternatifin detayına geçiş.
- Ürün bulunamadığında içerik metni kullanma açıklaması. Gerçek kamera/OCR sonraki görevdir.

## Veri geçişi

ProductMetadata tablosu mevcut Products/Restaurants tablolarını değiştirmeden eklenir.
Mevcut ürün ve restoran kayıtları silinmez. Metadata olmayan kaydın ülkesi ve doğrulaması bilinmiyor kabul edilir.
Başlangıç kurulumu tekrar çalıştırıldığında demo ürünler çoğaltılmaz.

## Alternatif kuralı

Gerçek kayıtlarda: aynı kategori + aynı satış ülkesi + Uygun değerlendirmesi + kaynak kontrolü
+ http/https kaynak bağlantısı + gelecekte olmayan kontrol tarihi gerekir.
Bunlar dini sertifika anlamına gelmez. İleride kaynak tazeliği ve değerlendirme kural sürümü eklenecek.
Demo ürünler gerçek önerilere karışmaz. Örnek Sütlü Çikolata → Örnek Sade Çikolata akışı açıkça demo olarak gösterilir.
Demo verilerde doğrulama tarihi ve sertifika uydurulmaz.

## API

- GET /api/products?q=...&country=TR
- GET /api/products/by-barcode/{barcode}?country=TR
- GET /api/products/{id} → product, alternatives, alternativeMessage

Ülke filtresi boşsa tüm ülkeler gelir. Aynı barkod farklı ülke kayıtlarında varsa barkod sorgusu
liste döndürür; rastgele bir ülkenin kaydı seçilmez. Bilinmeyen barkod boş liste, bulunmayan ürün detayı 404,
geçersiz ülke kodu 400 döner. Alanlar global katalog bağlantısına hazırdır; dış katalog ve otomatik çeviri henüz bağlı değildir.

## Deneme

1. BlazorApp1'i yeniden başlat.
2. Ana sayfada Örnek Sütlü Çikolata → Detay ve alternatifler bağlantısını aç.
3. Kaynak, kontrol tarihi, ülke, dil ve özgün metni kontrol et.
4. Örnek Sade Çikolata alternatifinin detayını aç; DEMO uyarısını kontrol et.
5. Ülke TR: demo kayıtlar gelir; DE: kayıt yok. Boş ülke filtresi tüm kayıtları getirir.
6. Mobilde yeni APK'yı yükle; Katalog → Detay ve alternatifler akışını kullan.

Android: ./build-android.ps1 -Clean (Visual Studio ile komut satırı arasında önbellek sorunu olursa).
Testler: dotnet run --project tests/CatalogChecks/CatalogChecks.csproj
SQLite üzerinde eski şema/veri koruması, tekrar kurulum, ülke/barkod varyantları, yanlış alternatiflerin dışlanması kontrol edilir.

Mevcut SQLite bağımlılığında NU1903 güvenlik uyarısı var; bağımlılık güncellemesi ayrı görev olarak takip edilmelidir.
