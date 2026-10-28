# Helal Rehber — Helal mi?

Gıda etiketlerini ve menüleri anlamaya yardımcı olan, geliştirme aşamasındaki bir öğrenci projesi. Android uygulaması **C# / .NET MAUI**, web arayüzü ve API **C# / ASP.NET Core Blazor** ile geliştirilir. Yerel çeviri motoru için ayrı bir Python servisi kullanılır; harita ve tarayıcı özelliklerinde küçük JavaScript köprüleri bulunur.

Proje şu anda ders sunumu ve portföy içindir. Herkese açık çalışan bir hizmet veya mağaza sürümü değildir. Mobil arayüz tasarımı geliştirilmektedir.

## Mevcut özellikler

- Open Food Facts üzerinden gerçek barkod sorgulama; marka, kaynak, özgün içerik ve veri eksikliği gösterimi.
- Android kamerayla barkod tarama; etiket/menü fotoğrafından OCR, okunan metni kontrol etme ve elle düzeltme.
- Sınırlı içerik kuralları, kullanıcı alerjen/diyet tercihleri ve belirsizlik uyarıları.
- Yerel OPUS-MT modeliyle İngilizce → Türkçe tam metin çevirisi; diğer dil çiftleri ayrıca kurulu yerel modellere bağlıdır.
- Menüden işletmeye sorulabilecek sorular ve cihazın desteklediği dillerde seslendirme.
- Harita/şehir araması; restoran bilgileri, kaynak ayrımı ve seyahat ekranı.
- Cihaza/tarayıcıya özgü favoriler; topluluk gönderileri, oylama, bildirim ve yönetici incelemesi.
- Bilgi düzeltme önerileri ve özel sertifika belgesi inceleme akışı. Belge incelemesi otomatik sertifika doğrulaması değildir.

Mobil alt sekmeler: **Katalog · Favoriler · Menü · Deneyimler · Profil**.

## Kapsam ve doğruluk sınırları

Her barkodun veritabanında bulunması, her dilin çevrilmesi veya her fotoğrafın okunması garanti edilmez. OCR Latin, Çince, Japonca, Korece ve Devanagari yazı sistemleri için yapılandırılmıştır; tüm dünya alfabeleri desteklenmez. Arayüzdeki Türkçe/İngilizce/Arapça seçenekleri OCR ve çeviri dil kapsamından ayrıdır.

OCR/çeviri hatası veya eksik kaynak bilgisi içerik kaybına yol açabilir. Kullanıcı özgün etiketi ve okunan metni kontrol etmelidir. Riskli madde bulunamaması **helal sertifikası, kesin dini hüküm veya alerjen yokluğu** anlamına gelmez. Topluluk oyu ve harita etiketi de sertifika değildir. Yerel başlangıç ürün/restoran kayıtları açıkça **DEMO** olarak gösterilir.

## Proje yapısı

| Klasör | İçerik |
| --- | --- |
| `helalharam/` | .NET MAUI Android istemcisi |
| `BlazorApp1/` | Blazor web arayüzü, API ve SQLite |
| `Shared/` | Paylaşılan modeller, kurallar ve metinler |
| `tools/` | Yerel yönetici/çeviri kurulum araçları ve çeviri servisi |
| `tests/` | C#, Python, JavaScript ve Android kontrol araçları |
| `docs/` | Özellik kapsamı, kurulum ve ölçüm raporları |

## Yerel çalıştırma

.NET 10 SDK, MAUI/Android iş yükü, uygun Android SDK ve JDK kurulmuş olmalıdır. Android emülatörü için API 35 kullanılarak deneme yapılmıştır; iOS/Windows dağıtımı doğrulanmış değildir.

1. Depoyu klonlayıp `helalharam.slnx` çözümünü Visual Studio'da açın; NuGet paketlerini geri yükleyin.
2. Web/API'yi başlatın:

   ```text
   dotnet run --project BlazorApp1/BlazorApp1.csproj --launch-profile http
   ```

   Yerel web: `http://localhost:5088`. SQLite veritabanı ilk çalıştırmada oluşturulur.
3. Android emülatörünü açın. Visual Studio'da `helalharam` projesini başlangıç projesi, çalışan Android emülatörünü hedef seçip çalıştırın. Emülatör API'ye `http://10.0.2.2:5088` üzerinden ulaşır. Bu adres fiziksel telefonda kullanılamaz.
4. Yönetici ekranı isteğe bağlıdır; varsayılan parola yoktur. Yerel kurulum için [yönetici yönergesi](docs/YONETIM_INCELEME.md).
5. Çeviri isteğe bağlıdır. Git klonlamak Python ortamını veya model ağırlıklarını getirmez. [OPUS kurulumu ve kapsamı](docs/OPUS_ENTEGRASYON.md), [model denemesi](docs/OPUS_MODEL_DENEMESI.md). Ortam/model kurulduktan sonra `start-opus-translation.cmd` kullanılır. Motor kapalıysa çeviri başarı sonucu üretmez.

Kamera/OCR cihazda, OFF ve harita dış bağlantıyla, bazı mobil ekranlar ise çalışan web/API ile işler. Çalıştırma ayrıntıları: [Android test sürümü](docs/ANDROID_TEST.md), [web yayın hazırlığı](docs/WEB_YAYIN.md).

## Doğrulama ve geliştirme durumu

Otomatik kontroller tek başına bütün mobil dokunma akışlarının doğrulandığı anlamına gelmez. OCR için sentetik bozulmuş görsellerle ölçümler vardır; bunlar gerçek kamera/ışık ortamlarının tamamını kapsamaz. Çeviri örnekleri genel doğruluk garantisi değildir.

- [OCR koşul ölçümleri](docs/OCR_KOSUL_TESTI.md) · [isteğe bağlı kontrast](docs/OCR_KONTRAST.md)
- [Çeviri entegrasyonu ve ölçümler](docs/OPUS_ENTEGRASYON.md)
- [Mobil gezinme](docs/MOBIL_GEZINME.md) · [UI çizim rehberi](docs/UI_CIZIM_REHBERI.md)
- [Gizlilik ve kullanım sınırları](docs/HATA_GIZLILIK.md)
- [Sunum kontrol listesi](docs/SUNUM_KONTROL_LISTESI.md)

## Kaynaklar ve kullanım hakları

Open Food Facts, OpenStreetMap, GeoNames ve kullanılan kütüphane/model lisansları: [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). Harici veriler kendi lisanslarına tabidir.

Copyright © 2026 Feyza Elibüyük. Projeye özgü kod için şu anda genel bir açık kaynak kullanım lisansı verilmemiştir; hakları saklıdır. Bu bildirim üçüncü taraf bileşenlerinin lisanslarını veya GitHub koşullarının sağladığı hakları değiştirmez. Public kaynak görünürlüğü MIT gibi bir lisans verildiği anlamına gelmez.

[Güvenlik ve hassas veri](SECURITY.md) · [Public paylaşım kontrolü](docs/PUBLIC_PAYLASIM.md)
