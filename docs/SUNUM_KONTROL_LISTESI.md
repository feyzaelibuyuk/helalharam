# Hocaya sunum için kısa kontrol listesi

## Açılış

1. GitHub Desktop'ta sunacağınız commit'in seçili olduğundan ve Changes alanının temiz olduğundan emin olun.
2. Web için depo kökünde `dotnet run --project BlazorApp1/BlazorApp1.csproj --launch-profile http`. Adres `http://localhost:5088`.
3. Çeviri gösterecekseniz kurulu yerel motor için `start-opus-translation.cmd` dosyasını açın ve açık bırakın. İngilizce→Türkçe için OPUS modeli kurulmuş olmalı. Diğer yüklü çiftler/otomatik algılama için ayrıca `start-local-translation.cmd` açılabilir. Motor/model kurulumu ayrıca tamamlanmış olmalı; yalnızca web sunucusunun açık olması yeterli değildir. Yeni hesap veya ücretli çeviri anahtarı gerekmiyor.
4. Android için Visual Studio'da `helalharam` MAUI projesini ve çalışan API 35 emülatörünü seçip çalıştırın. Emülatörün API adresi `10.0.2.2:5088`; bilgisayardaki web/API açık kalmalı.

## Gösterilecek kısa akış

- Gerçek ambalaj barkodunu sorgula. OFF'de bulunamazsa bunu eksik veri olarak göster; OCR/elle içerik seçeneğine geç. Demo yerel ürünleri açıkça demo diye tanıt.
- Ürün detayında özgün içerik, kaynak/tarih, alerjen/iz etiketleri ve sertifika durumunu ayrı göster.
- Alerjen/diyet tercihinden bir test seçimi yap; etiket metnini yeniden değerlendir. Eşleşme yokluğunun güvenli/alerjensiz sonucu olmadığını belirt. Test tercihlerini sunumdan sonra temizle.
- Etiket veya menü fotoğrafını çek; OCR metnini özgün fotoğrafla karşılaştır. Alfabe modeli ve metin dilini doğru seç. Menü soruları gerçek tarif/kesim/ortak ekipman doğrulaması değildir.
- Tam metin çevirisine geç. Motor kapalıysa hata açıklaması gösterilir ve özgün metin korunur. Çeviri kalitesini abartma; modelin hata yapabileceğini açıkla.
- Soru kartını farklı dilde göster; cihazda uygun ses varsa seslendir.
- Harita araması için açık onay ver. OSM etiketini doğrulanmış restoran/sertifika gibi sunma.
- Sertifika fotoğrafını önizle; yüklemek, logo görmek veya incelemeyi tamamlamak otomatik helal onayı değildir.
- Topluluk deneyimini gönder; yönetici incelemesi öncesi herkese görünmediğini, yayınlandıktan sonra oyların değiştirilebildiğini göster. Gerçek dışı işletme/ziyaret iddiası oluşturma; gösterim metnini TEST olarak belirt ve sonrasında geri çek.

## Henüz tam bitmiş gibi sunulmaması gerekenler

- Tüm dünya barkodlarının veritabanında bulunması garanti değildir.
- OCR beş başlangıç alfabesiyle sınırlı; Arapça/Kiril OCR yok. Arayüz TR/EN/AR kısmi; tüm ekranlar/tüm diller çevrilmedi.
- Eski LibreTranslate EN→TR modelinde bilinen kalite sorunları vardır; yeni OPUS adayının yalnız sınırlı örnekleri kontrol edilmiştir. Tam/doğru global etiket çevirisi tamamlanmış sayılmaz.
- Kişisel kurallar sınırlı sözlük ve kaynak etiketlerine dayanır; klinik doğruluk/alerjensizlik/çapraz temas ölçümü yoktur.
- Diğer dini profiller, gerçek push bildirimleri, doğrulanmış kullanıcı hesabı ve kötüye kullanımın tam önlenmesi henüz yoktur.
- Resmî sertifikaların otomatik küresel doğrulama API'si yoktur. Topluluk oyu/rozet belge veya uzmanlık değildir.
- Web/uygulama herkese açık yayınlanmadı. İlk gerçek kullanıcı geri bildirimi, cihaz testleri ve mobil son tasarım ayrı işlerdir.

Durdurma: Visual Studio için Shift+F5; komut satırındaki web/çeviri servisi için Ctrl+C. Yönetici parolasını sunum kaydında veya ekran görüntüsünde göstermeyin.
