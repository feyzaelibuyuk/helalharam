# Etiket / menü tam metin çevirisi — Trello 25

## Kullanım

Web: /translate. OFF ürününde /translate/off/{barcode} özgün içindekileri forma getirir. Barkod bulunamadığında boş metin formu veya mobil OCR yolu kullanılabilir. Mobil: Profil → Etiket / menü çevirisi; OCR ekranından okunan metni çeviri sayfasına gönder; gerçek ürün detayından içindekileri çevir.

İçindekiler, alerjen/eser miktar uyarıları, besin değerleri ve diğer açıklamaları kapsayan tüm kaynak metni girin. Metin 12.000 karakteri aşarsa reddedilir; otomatik kısaltma/özetleme yapılmaz. Eksik fotoğraf/OCR satırı tamamlanmış sayılmaz. Kaynak ve hedef dil arayüz dilinden ayrıdır; hedef diller Microsoft'un güncel translation dil listesinden gelir. Arapça/Kiril fotoğraf OCR desteği bu görevle eklenmez; o dillerde elle girilmiş metin çevrilebilir.

İki onay gerekir: özgün metni kontrol etme ve metnin Microsoft Translator'a gönderilmesi. Fotoğraf dış servise yüklenmez. Web formu metni URL'ye koymaz, çeviri yanıtları no-store kullanır, metin/çeviri DB veya kalıcı önbelleğe yazılmaz. Anahtar yalnız sunucuda kalır. Kullanıcı metni uygulama günlüklerine yazılmaz.

Etiket modunda özgün dil kuralları TR/EN/FR/DE ise kaynak değerlendirmesi ayrı verilir. Çeviri üzerindeki kısıtlı/şüpheli eşleşmeler yalnız ön kontroldür; ürün veya alerjen onayı değildir. Hedef bu dört dil dışındaysa açıkça gösterilen ek İngilizce çeviri ön kontrolde kullanılır; iki hedef karakter kotasına dahildir. Dil algılama güveni düşükse özgün değerlendirme yapılmaz. E kodu çeviride eşleşmezse uyarı verilir. Bu kontroller tüm anlam kayıplarını veya alerjen hatalarını yakalama garantisi değildir. Menü modunda ürün uygunluk sınıflaması yapılmaz; işletmeye tarif/hazırlama sorulur.

## Gerçek servisi açma — kullanıcının yapacağı bölüm

Bu geliştirmede Azure hesabı/anahtarı olmadığı için gerçek kullanıcı metni çevirisi etkin değildir. Sahte/demonstrasyon çevirisi başarı gibi gösterilmez. Dil listesi anahtarsız resmî uç noktadan alınabilir; bu, çeviri anahtarı yerine geçmez.

1. Azure hesabından Translator kaynağı oluşturun; yalnız F0 ücretsiz katmanını seçin. Ücretli plan seçmeyin. F0 seçeneği yoksa devam etmeden koşulları kontrol edin. Microsoft fiyatlandırması: https://azure.microsoft.com/en-us/pricing/details/translator/ . F0 aylık 2 milyon karakter olarak listelenir; kota ve hesap koşullarını portalda doğrulayın.
2. Kaynak oluşturulunca Keys and Endpoint bölümündeki key ve region/location bilgilerini bilgisayarınızda kullanın. Anahtarı sohbete, GitHub'a veya ekran görüntüsüne koymayın.
3. Depo kökünde `dotnet build tools/TranslatorSetup/TranslatorSetup.csproj` çalıştırın.
4. `dotnet tools/TranslatorSetup/bin/Debug/net10.0/TranslatorSetup.dll BlazorApp1/BlazorApp1.csproj` çalıştırın. Bölgeyi ve görünmeyen anahtarı yerel konsola girin. Araç Translation:Enabled, Key, Region ve DailyCharacterLimit ayarlarını User Secrets'a ekler; yönetici ayarını silmez. User Secrets şifrelenmiş kasa değildir; Windows kullanıcı hesabı erişimiyle korunmalıdır.
5. Development web/API'yi yeniden başlatın. /translate ekranında kısa, kişisel bilgi içermeyen gerçek etiket metniyle çeviriyi ve özgün metni karşılaştırın.

Üretimde anahtar hosting secret ayarından Translation__Key, Translation__Region, Translation__Enabled=true ile sağlanır. Anahtar istemciye/APK'ya gönderilmez.

## Sınırlar ve test

Uygulama IP başına dakikada 5 çeviri POST'una izin verir; web ve API ayrı IP pencereleri kullanır. Uygulama genelinde varsayılan UTC gününe göre günlük 50.000 hedef karakter sınırı vardır; ek İngilizce hedef iki kat sayılır. Sayaç RAM'dedir, yeniden başlatmada sıfırlanır ve birden fazla sunucuda paylaşılmaz; faturalama garantisi değildir. Gerçek kota/plan Azure'da yönetilir. Otomatik ücretli plan geçişi veya çeviri tekrar denemesi yapılmaz; başarısız istekler de yerel bütçede sayılır. Dil listesi 24 saat önbellekte tutulur; kullanıcı metni tutulmaz.

/api/translation/options GET yapılandırma durumu ve dil listesi, /api/translation POST çeviri sağlar. Onaysız/boş/aşırı uzun/yanlış dil girişleri 400; yapılandırılmamış/yanlış anahtar 503; servis/kota 429; dış servis hatası 502; zaman aşımı 504. Kişisel metin kaybolmadan tekrar denenebilir. Çeviri POST gövdesi 256 KB (12.000 Unicode karakterin JSON/form kaçışları için); diğer formlar 64 KB kalır.

Kaynak: https://learn.microsoft.com/en-us/azure/ai-services/translator/text-translation/reference/v3/translate ve https://learn.microsoft.com/en-us/azure/ai-services/translator/text-translation/reference/v3/languages . Kontrollü test yanıtları yalnız testte kullanılmalı; gerçek çeviri doğruluğu anahtar bağlanıp gerçek metinle doğrulanmadan Trello 25 tamamlanmış sayılmaz.

## Bu oturumun gerçek doğrulama durumu

Web, TranslationChecks ve TranslatorSetup projeleri 0 hata/0 uyarıyla derlendi. TranslationChecks çalıştırması ve yeni web DLL'sinin başlatılması Windows Uygulama Denetimi tarafından 0x800711C7 ile engellendi. Koruma devre dışı bırakılmadı; kopyalama/yeniden adlandırma yoluyla engel aşılmadı. Servis mock testleri ve web uçtan uca kontrolleri başarılı sayılmadı; gerçek çeviri doğrulanmadı. Önceki çalışan web süreci korunur; yeni çeviri endpoint'leri orada henüz yoktur.

Anahtarsız resmî Microsoft dil uç noktası bağımsız HTTP kontrolünde 138 dil döndürdü; tr/en/ar/ja/ko/zh-Hans/hi mevcut. Bu yalnız dil listesi doğrulamasıdır, gerçek çeviri değildir. Azure hesabı/anahtarı oluşturulmadı veya okunmadı.

Son Android TestBuild 0 hata ve mevcut 4 XA4301 OCR uyarısıyla derlendi. APK imzası doğrulandı. artifacts/android-translation-preview/ altında ayrı önizleme paketi hazırlandı; çalışan emülatör sürümünün yerine kurulmadı. Yeni sunucu çalışmadan mobil çeviri dil listesi alınamaz ve düğme kapalı kalır. Mobil dokunma akışı ve gerçek Azure yanıtı henüz test edilmedi. Trello 25 devam eder.
