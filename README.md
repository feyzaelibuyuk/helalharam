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

## Yeni mobil akışlar
Menü metninden sorular, cihazda deneyim taslağı, profil tercihleri ve demo ürün ekranı eklendi. Mevcut API kataloğu Katalog sekmesinde korunur. Katalog sekmesinde kamera ile barkod tarama eklendi. Etiket ve menü fotoğrafları için Android OCR eklendi; kullanım ve dil kapsamı: docs/ETIKET_MENU_OCR.md. Kamera kullanımı ve cihaz kontrol adımları: docs/BARKOD_KAMERA.md.
Özellikler: docs/OZELLIKLER.md. Android derleme: ./build-android.ps1; emülatörde çalıştırma: ./build-android.ps1 -Run.
Aktarım öncesi ilk çalışmanın dosya ve Git yedekleri migration-backup klasöründe yereldir, GitHub'a gönderilmez.

## Android cihaz seçimi
Bu bilgisayarda Pixel_7 (API 35) sistem imajı kuruludur. API 36 cihazının sistem imajı eksik olduğu için açılmaz. Visual Studio'da API 35 cihazını seçin veya Android Device Manager üzerinden başlatın.
Komut satırı APK'sı C# derlemelerini içerir; build-android.ps1 ile oluşturulabilir.

## Ürün kataloğu
Ürün detayları, ülke filtresi ve demo alternatif akışı için docs/URUN_KATALOG.md dosyasına bakın. Web ve mobil Katalog sekmesi aynı servisi kullanır. Mobil Ürünler sekmesi ilk yerel demo olarak korunmuştur.

## Gerçek ürün API'si
Open Food Facts barkod bağlantısı web ve mobilde eklendi. Kullanım ve sınırlar: docs/OPEN_FOOD_FACTS.md. API anahtarı gerekmez. Gerçek ürünlerde sınırlı kaynaklı içerik kuralları uygulanır; sertifika onayı verilmez. Kapsam ve deneme: docs/ICERIK_KURALLARI.md.


## Favoriler
Web ve mobil ürün favorileri eklendi. Kapsam, saklama ve kontrol adımları: docs/FAVORILER.md. Listeler tarayıcıya/cihaza özeldir; hesap eşitlemesi yoktur.


## Türkiye restoran/yemek detayları
Şehir ve şube bilgileri, kaynak, içerik belirsizliği ve işletme soruları eklendi. Mevcut kayıtlar DEMO'dur; gerçek işletme verisi bağlı değildir. Kontrol adımları: docs/TURKIYE_YEMEK_DETAY.md.


## Bilgi düzeltme önerileri
Web ve mobil ürün/yemek detaylarından düzeltme önerisi gönderilebilir. Öneriler ayrı tabloda inceleme bekler; katalog otomatik değişmez. Kapsam ve kontrol: docs/BILGI_DUZELTME.md.


## Gönderim yönetimi
Yönetici erişimiyle kuyruk, inceleme kararı ve geçmiş eklendi. Varsayılan parola yoktur; yerel kurulum için configure-admin.ps1 ve docs/YONETIM_INCELEME.md dosyasına bakın. Katalog otomatik değiştirilmez.

Gizlilik ve hata ekranları: [HATA_GIZLILIK](docs/HATA_GIZLILIK.md). Web: /privacy; mobil: Profil → Gizlilik ve kullanım sınırları.

Web yayın paketi ve kalıcı sunucu verileri: [WEB_YAYIN](docs/WEB_YAYIN.md).

Android sunum/test paketi 0.1.0: [ANDROID_TEST](docs/ANDROID_TEST.md).

İlk kullanıcı geri bildirim formu ve deneme planı: [GERI_BILDIRIM](docs/GERI_BILDIRIM.md).
