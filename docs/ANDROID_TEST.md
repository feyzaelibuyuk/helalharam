# Android test sürümü 0.1.0 (2)

Bu, hocaya yerel sunum için geliştirme anahtarıyla imzalanmış Debug APK'dır. Mağaza veya herkese açık dağıtım sürümü değildir. UI son tasarım değildir. Uygulama kimliği com.companyname.helalharam olarak korunur; önceki aynı imzalı kurulum üzerine güncellenebilir. APK, .NET derlemelerini içerir; Visual Studio/hata ayıklayıcı bağlı olmadan açılmalıdır. TestBuild paketinde OCR test broadcast alıcısı derlenmez.

## Hazır paket

artifacts/android-test/HelalRehber-0.1.0-test.apk
SHA256.txt dosyası paket özeti, KURULUM.md bu yönergedir. artifacts/ Git'e eklenmez. APK'ya yönetici parolası, web User Secrets veya veritabanı dahil edilmez.

Betik çalıştırmaya izin verilen ortamda yeniden üretme: depo kökünde ./build-android.ps1 -TestPackage. PowerShell betikleri engelliyorsa güvenlik politikasını değiştirmeyin; derleme normal dotnet komutlarıyla yapılabilir. Türkçe klasör yolları nedeniyle geçici R: eşlemesi gerekir; R: kullanımda ise üzerine yazmayın. CLI home gerçek depo/.local-dotnet olmalıdır. Kullanılan derleme:

```text
dotnet build R:/helalharam/helalharam.csproj -f net10.0-android -p:TargetFrameworks=net10.0-android -p:EmbedAssembliesIntoApk=true -p:UseSharedCompilation=false -p:TestBuild=true
```

## Emülatörde kurulum

1. Android Device Manager'dan Pixel 7 Android 15 / API 35 cihazını başlatın; ana ekran gelsin.
2. APK'yı açık emülatör penceresine sürükleyin veya Android SDK adb ile install -r kullanın.
3. Uygulama listesinden Helal Rehber Test'i açın. Aynı paket farklı anahtarla önceden kurulmuşsa kaldırmadan önce yerel favori/tercihlerin silineceğini dikkate alın; otomatik kaldırma yapılmaz.
4. Web/API Development ortamında localhost:5088 üzerinde açık olmalı. Emülatör 10.0.2.2:5088 kullanır. Başka portla sunum yapmayın.

Gerçek telefon bu adresle bilgisayara bağlanamaz. Telefona özel HTTPS API yapılandırması ayrı görevdir. İnternet OFF sorguları için gerekir. Yerel menü soruları ve kaydedilmiş favori listesi çevrimdışı açılabilir; güncel ürün detayı/analizi sunucu gerektirir.

## Sunum kontrol listesi

- Uygulamayı kapatıp simgeden açın: çökme olmamalı, Profil'de sürüm görünmeli.
- Katalogda ürün arayın, detay ve kaynak/demo ayrımını gösterin.
- Gerçek OFF barkodu 3017620422003 ile sorgulayın; ağ/veri kaynağına bağlı sonuç değişebilir. Sonuç bulunmaması uygunluk kararı değildir.
- Favoriye ekleyin, uygulamayı yeniden açıp listede kaldığını kontrol edin.
- Restoran/yemek arayın ve işletmeye sorulacak soruları açın.
- Kamera iznini kabul/ret durumlarında deneyin; ret durumunda elle giriş yolu olmalı.
- Etiket fotoğrafında okunan metni elle doğrulayın, yanlışları düzeltin. Menü ve paket modlarını ayrı deneyin. Latin dışı OCR ve tam çeviri henüz hazır değildir.
- Düzeltme önerisi gönderip web yönetici ekranında göründüğünü kontrol edin. Test önerisini TEST olarak belirtin; değerlendirme otomatik katalog değişikliği değildir.
- Profil → Gizlilik ve kullanım sınırları → Kapat yolunu deneyin.
- İnternet/API erişimi olmadığında hata mesajı görünmeli; helal veya alerjen yok sonucuna dönüşmemeli.

Kamera/galeri, dokunma akışları ve kişisel izin durumları elle kontrol gerektirir. APK derlemesinin geçmesi bunları tek başına doğrulamaz. Eksik içerik/risk bulunamaması helal onayı veya alerjen yokluğu değildir.

## Bu pakette doğrulananlar

21 Ekim 2026: Android derlemesi 0 hata, önceden bilinen 4 XA4301 OCR kütüphanesi uyarısıyla tamamlandı. APK imzası apksigner ile doğrulandı. Paket adı/sürümü 0.1.0 (2), arm64-v8a ve x86_64 mimarileri aapt ile kontrol edildi. OCR_SMOKE test alıcısı paket manifestinde yok. Pixel_7 API 35 cihazına adb install -r başarılı; hata ayıklayıcısız açılışta işlem çalışıyor ve AndroidRuntime hata günlüğünde çökme yok. Web API /api/products HTTP 200. Paketleme betiği sözdizimi kontrol edildi; betik çalıştırma kısıtı nedeniyle bu oturumda eşdeğer derleme/kopyalama komutları kullanıldı. Kamera/galeri, izin reddi, favori ve öneri dokunma akışları bu görevde elle test edilmedi; sunum öncesi yukarıdaki liste tamamlanmalı.

Device Manager'daki pixel_7_api_35 kaydı mevcut olmayan google_apis_playstore görüntüsüne işaret ediyor. Doğrulamada kurulu google_apis görüntüsünü kullanan Pixel_7 çalıştırıldı. Bozuk kayıt silinmedi veya değiştirilmedi.

Sonraki güncelleme: çok alfabeli Android OCR kapsamı ve sınırlamaları için [COK_ALFABELI_OCR](COK_ALFABELI_OCR.md). Önceki Latin-only test sonuçları tarihsel kayıttır.

## Profil üzerinden bağlantı kontrolü

Profil → Bağlantıyı kontrol et → Bağlantıyı kontrol et yolunu kullanın. Düğme basılınca sabit yerel adresteki `/api/products` sorgulanır; konum, kişisel içerik veya çerez gönderilmez. Başarı için HTTP başarı kodu, JSON içerik türü ve ürün dizisi beklenir. Kontrolün zaman sınırı 10 saniyedir; sayfa kapanınca istek iptal edilir.

Başarı sadece yerel ürün API'sine erişimi gösterir. OFF, çeviri motoru, kamera ve harita ayrı denenmelidir. Gerçek telefon 10.0.2.2 adresiyle bilgisayara bağlanamaz.

Elle deneme: Sunucuyu kapatıp kontrol edin; açıklayıcı hata görülmeli. Sunucuyu yukarıdaki komutla başlatıp yeniden deneyin; başarı görülmeli. İstek sürerken sayfayı kapatıp tekrar açın. Birden fazla dokunuş eşzamanlı istek başlatmamalı.

28 Ekim 2026: Yeni ekran Android Compile hedefiyle 0 hata/0 uyarı verdi. Bağlı cihaz/emülatör bulunmadığından yukarıdaki dokunma denemeleri yapılmadı. Bu kontrol yeni APK paketinin oluşturulduğu anlamına gelmez.
