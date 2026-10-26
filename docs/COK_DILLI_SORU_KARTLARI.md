# Çok dilli soru kartları ve seslendirme — Trello 26

## Kullanım

Web: üst menü → Çok dilli soru kartları veya /questions. Restoran yemek detayından da bağlantı vardır. Mobil: Menü → Çok dilli soru kartları ve seslendirme. Menü OCR sonucundan aynı ekran açılır.

İşletmeye gösterilecek dili seçin. Her kartta Türkçe anlam ve seçilen dilde büyük metin görünür. Arapça sağdan sola gösterilir. Seslendir seçilen dilde sesi kullanır; Sesi durdur oynatmayı keser. Yeni kart/dil seçimi, sayfadan çıkma ve mobilde arka plana geçme önceki sesi iptal eder. Kayıt veya mikrofon izni gerekmez. Yanıtlar kaydedilmez, sertifika doğrulaması yapılmaz.

## Kapsam

TR, EN, AR, FR, DE, IT, ES, JA, KO, basitleştirilmiş ZH: 10 dilde 8 sabit soru bulunur. Tam içerik/sos/katkılar, domuz kaynakları, et tedarikçisi ve kapsamı geçerli sertifika, alkollü içecek kullanımı, jelatin kaynağı, et suyu/bulyon, ortak ekipman/yağ, bileşen çıkarma veya içerikleri açıklanan alternatif sorulur.

Bu liste OCR, genel çeviri veya arayüz dili listesi değildir. Kartlar menüde ilgili madde bulunduğunu iddia etmez. Eski menü anahtar kelime önerileri ve sınırlı TR/EN/FR/DE eşleştirmesi korunur; sabit kartlar tüm yemekler için ayrıca açılabilir. Diğer din/diyet ve kişisel alerjen soru kartları bu görevle yapılmaz. Dil çeşitleri taslaktır; ana dili konuşan kişilerce anlam ve doğallık incelemesi yapılmadan uzman onaylı denmez.

## Ücretsiz ses

Uygulama ücretli TTS/çeviri API'si çağırmaz; soru metinleri C# kataloğundadır ve çeviri motoru açık olmasa da gösterilir.

Web, tarayıcının SpeechSynthesis API'sini kullanan küçük bir JavaScript köprüsü içerir (tarayıcı cihaz API'si). Sadece localService=true olarak bildirilen ve dil kodu eşleşen sesi seçer; ağ sesi veya yanlış dilde varsayılan ses seçmez. Dil sesi yoksa metin gösterilir ve açıklama verilir. Sesler geç yüklenirse yeniden Seslendir kullanılabilir; otomatik konuşma başlatılmaz. JavaScript/ses API'si desteklenmezse kart metinleri okunabilir kalır.

Mobil C# .NET MAUI TextToSpeech.Default kullanır; GetLocalesAsync sonucundan seçilen dile eşleşen locale bulunur. ISO3/BCP47 dil adları normalize edilir. Eşleşen ses yoksa varsayılan dile düşmez. Cihaz ses motoru kendi ayarına göre internet kullanabilir; tamamen çevrimdışı olacağı garanti edilmez. Telefon ayarlarından ilgili dilin ses verisi kurulmalıdır. Android 11+ TTS_SERVICE görünürlük sorgusu manifestte vardır.

## Doğrulama

MenuChecks: eski menü davranışı, 10 dilde eksiksiz/tekil kart kimlikleri ve özgün Türkçe anlam, bilinmeyen dil reddi, Arapça RTL, cihaz ISO3/BCP47 dil normalizasyonu geçti.

Node ses köprüsü testleri: doğru yerel ses/tam metin, yanlış dilde varsayılan ses kullanmama, ağ sesi reddi, geç yüklenen ses listesi, kart değişince iptal/eski tamamlanma bildirimi, durdurma/sayfadan çıkış ve desteklenmeyen tarayıcı durumları: 6/6 geçti. Bu testler gerçek hoparlör sesini veya tarayıcı görselini doğrulamaz.

Web 0 hata/0 uyarıyla derlendi. Android TestBuild 0 hata ve önceden bilinen 4 XA4301 OCR kitaplık uyarısıyla derlendi. Önceden tespit edilen Windows Uygulama Denetimi engeli nedeniyle yeni web uygulamasının gerçek tarayıcı akışı ve gerçek cihaz ses çıktısı bu oturumda doğrulanmış sayılmaz. İnsan dil kontrolü ve cihazda dinleme testi bekliyor; Trello 26 test aşamasındadır.

Manuel kontrol: /questions → Japonca seç → Türkçe anlam/Japonca büyük metin → ses varsa Japonca oku, yoksa açık mesaj → Arapça seç → RTL metin → yeni kart sesi eskisini kessin → durdur/geri düğmesi sesi kessin. Androidde Menü ve menü OCR bağlantılarını ayrıca kontrol edin. Bu işlem Azure/LibreTranslate hesabı veya servisinin açık olmasını gerektirmez.

Resmî kaynaklar:
- https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device-media/text-to-speech?view=net-maui-10.0
- https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis/getVoices
- https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesisVoice/localService
