# Çok alfabeli Android OCR — Trello 24, ilk parça

Alfabe modeli metni fotoğraftan okur; arayüz dili, çeviri ve dini değerlendirme dili birbirinden ayrıdır. Bu branch arayüzün çok dilli hale getirilmesini veya çeviriyi tamamlamaz. Trello 24 devam eder.

Android etiket/menü fotoğrafı ekranında Latin, Çince, Japonca, Korece, Devanagari seçilebilir. ML Kit modelleri APK içinde paketlenir; görsel işleme cihazdadır. Fotoğraf dış servise yüklenmez. İlk paket boyutu artar. Okunan metni kullanıcı fotoğrafla karşılaştırıp düzeltmeli ve gerçek dilini ayrıca seçmelidir. Yanlış model eksik/yanlış sonuç verebilir; model otomatik dil tespiti değildir.

Arapça, Kiril ve diğer alfabeler bu motorla henüz desteklenmez; açık uyarı ve elle metin girişi yolu vardır. Diğer platformlarda yeni modeller doğrulanmadığından Latin dışı deneme desteklenmez mesajı verir. Android modelleri C# .NET Android bağlayıcılarıyla çağrılır.

İçerik değerlendirmesi TR/EN/FR/DE kapsamını korur. Çince/Japonca/Korece/Hintçe/İtalyanca/İspanyolca/Arapça/Rusça seçimleri kaynak dili olarak gönderilir; mevcut motor bunlara yetersiz bilgi yanıtı verir. Bir OCR eşleşmesi helal veya alerjen güvenliği onayı değildir. Tam metin çevirisi Trello 25 kapsamındadır.

Kaynaklar: https://developers.google.com/ml-kit/vision/text-recognition/v2/languages ve mevcut Plugin.Maui.OCR v1.1.1 Android kaynak kodu: https://github.com/kfrancis/ocr/blob/14e88c3576769f47c40bb1908ccbdb9bb5f5a6dc/src/Plugin.Maui.OCR/OcrImplementation.android.cs . Eklentinin bu sürümü Latin recognizer çağırdığı için seçenek eklemek tek başına yeterli değildir; yeni ScriptOcr doğrudan ilgili ML Kit recognizer'ını açar. Paketler mevcut Latin sürümüyle eşlenmiş 116.0.1.4 kullanır.

## Kontrol

Debug-only OCR_SCRIPT_SMOKE testi uygulamada üretilen yapay Latin/Çince/Japonca/Korece/Hintçe resimlerini ilgili native motorla okur. Kullanıcı fotoğrafına erişmez. TestBuild paketinde test alıcısı bulunmaz. Bu test gerçek menü/ambalaj fotoğrafının doğruluğunu garanti etmez. Elle kamera/galeri, boş görüntü, yanlış model, izin reddi ve sayfadan ayrılma testleri ayrıca gerekir.

## Doğrulama sonuçları

21 Ekim 2026: Debug native OCR_SCRIPT_SMOKE testi Android 35'te beş modelin tamamında beklenen metni okudu: Latin gelatin, Çince 牛奶, Japonca 牛乳, Korece 우유 ve Devanagari दूध. Bu test uygulamanın ürettiği görüntülerdir; gerçek kullanıcı etiketi değildir. Yeni ScriptOcr, işlem bitince native recognizer'ı kapatır; iptal edilen sayfaya sonucu yerleştirmez.

Web derlemesi 0 hata/0 uyarı. Yeni diller için IngredientChecks çalıştırması Windows Uygulama Denetimi tarafından engellendi; güvenlik kuralı değiştirilmedi. Çalışan HTTP analiz API'sinde zh/ja/ko/hi/ar/it metinlerinin tümü Yetersiz bilgi verdiği bağımsız doğrulandı. Hiçbiri otomatik helal/alerjen güvenliği onayı üretmedi.

## Trello 24'ün kalan arayüz parçası

Arayüz metinleri kaynak anahtarlarına taşınacak; ilk çeviri seti Türkçe/İngilizce/Arapça olarak hazırlanıp genişletilebilir olacak. Sağdan sola düzen, dil seçiminin kalıcılığı ve eksik çeviri için açık Türkçe geri dönüşü ayrı doğrulanmalı. Kaynak etiket/menü metni bu seçimle değiştirilmemeli; değerlendirme/sertifika kaynağı ve dilleri gizlenmemeli. Bu branch'te arayüz dili seçici yoktur.

Son TestBuild APK derlemesi 0 hata ve mevcut 4 XA4301 uyarısıyla tamamlandı. İmza doğrulandı; teslim APK manifestinde OCR test alıcısı bulunmadığı kontrol edildi. API 35 emülatörüne güncelleme kurulumu başarılı.

28 Ekim 2026 Latin modelin loş/silik/bulanık/kadraj koşullarındaki kontrollü test sonuçları: [OCR koşul testi](OCR_KOSUL_TESTI.md). Bunlar fiziksel ışık eşiği veya tüm alfabelerde başarı garantisi değildir.
