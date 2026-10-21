# Etiket ve menü OCR — Android ilk sürüm

Katalog veya Ürünler → Etiket fotoğrafını oku; Menü → Menü fotoğrafını oku. Kamera veya galeri seçimi ile fotoğraf alınır. Fotoğraf önizlemesi ile karşılaştırılan metin düzenlenebilir. Kullanıcı metni ve gerçek dilini kontrol ettiğini işaretleyince analiz başlatılır. Metin, dil veya tür değişince önceki sonuç ve onay sıfırlanır.

- Paket: mevcut /api/ingredients/assess servisine düzeltilmiş metin gönderilir. TR/EN/FR/DE sınırlı içerik kuralları; diğer diller yetersiz bilgi verir. Fotoğraf, marka veya sertifika OCR metninden doğrulanmaz.
- Menü: cihazda genel sorular ve TR/EN/FR/DE anahtar kelimelerine göre et, bulyon, jelatin ve alkol soruları hazırlanır. Yemek adından tam içerik tahmini veya uygunluk onayı verilmez. İşletmenin yanıtı olmadan doğrulanmış alternatif önerilmez.
- OCR: Plugin.Maui.OCR 1.1.1, Android ML Kit Latin recognizer. Türkçe dahil Latin harfli fotoğraflar denenebilir; doğruluk garanti edilmez. Arapça/Çince gibi alfabeler ve otomatik çeviri bu görevin kapsamında değildir. Menü soruları Türkçe gösterilir.
- Fotoğraf sunucuya yüklenmez. Android görüntüsü en fazla 16 MB / 100 milyon piksel kabul edilir, uzun kenar örneklenerek en fazla 2048 piksele indirilir; EXIF yönü düzeltilir. Uygulamanın EXIF için oluşturduğu geçici dosya silinir. MAUI/kamera uygulaması kendi önbellek dosyasını tutabilir; galerideki özgün fotoğraf silinmez.
- Metin en fazla 12.000 karakterdir. Okunamayan/aşırı büyük fotoğraf, iptal, izin reddi ve sunucu erişim hatalarında elle metin girişi kullanılabilir.

## Kontrol

1. Blazor projesini 5088 portunda çalıştırın, Android uygulamasını başlatın.
2. Katalog → Etiket fotoğrafını oku → Galeriden fotoğraf seç. Net bir JPG/PNG içindekiler fotoğrafı seçin; çıkan metni özgün fotoğrafla karşılaştırın.
3. Gerçek dili seçin. Onay kutusu boşken değerlendirme başlamamalı. Düzeltip onaylayın; un/su/tuz ile sınırlı kurallarda risk bulunmadı, kaynağı belirsiz jelatin ile şüpheli sonuç beklenir. Bu sonuç helal onayı değildir.
4. Metni/dili değiştirin; eski sonuç ve onay silinmeli. Diğer dilde yetersiz bilgi beklenir.
5. Menü türünü seçin; Chicken soup / Poulet sauce / Hähnchen Suppe ile et ve bulyon soruları beklenir.
6. Fotoğraf çekme iznini reddetme, fotoğraf seçimini iptal etme, boş/bulanık fotoğraf ve Blazor kapalıyken istek davranışını deneyin.
7. Fotoğraf çek → yatay/dikey fotoğraf; metnin doğru yönde okunduğunu kontrol edin. Çok küçük etiket için yalnız içindekiler bölümünün yakın fotoğrafını çekin.

Menü kuralları için otomatik kontroller: tests/MenuChecks. tests/OcrAndroidSmoke.cs yalnız Android Debug APK'sına dahil edilir; OCR_SMOKE yayınını alınca uygulama verisine erişmeden test görüntüsü üretir, görüntü normalizasyonu ve gerçek ML Kit OCR motorunu çalıştırır. Emülatör testinde gelatin ve E471 ifadeleri başarıyla okundu (OcrSmoke: PASS). Release derlemesine bu alıcı dahil edilmez. Gerçek kamera/galeri OCR doğruluğu ve izin akışları cihazda ayrıca kontrol edilmelidir. Bu branch feat/barkod-kamera üzerinden açılmıştır; kamera değişikliklerini de içerir. GitHub Desktop ile merge etmeden önce cihaz kontrolü önerilir.

Kaynaklar: [OCR paketi](https://www.nuget.org/packages/Plugin.Maui.OCR/1.1.1), [Android uygulama kaynağı](https://github.com/kfrancis/ocr/blob/v1.1.1/src/Plugin.Maui.OCR/OcrImplementation.android.cs), [ML Kit dilleri](https://developers.google.com/ml-kit/vision/text-recognition/v2/languages), [MAUI fotoğraf seçimi](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device-media/picker?view=net-maui-10.0).

Android paketleme XA4301 uyarısı: ML Kit bağımlılıklarında aynı yerel OCR kütüphanesinin tekrarı bulunuyor. Her mimaride iki kopyanın SHA256 değerleri aynı; emülatörde OCR motoru çalıştı. Uyarı gizlenmedi.

Debug APK yüklüyken yerel OCR testini tekrar çalıştırma:

```powershell
& 'C:\Android\Sdk\platform-tools\adb.exe' shell am broadcast -a com.companyname.helalharam.OCR_SMOKE -p com.companyname.helalharam
& 'C:\Android\Sdk\platform-tools\adb.exe' logcat -d -s OcrSmoke:I
```

Sonraki güncelleme: çok alfabeli Android OCR kapsamı ve sınırlamaları için [COK_ALFABELI_OCR](COK_ALFABELI_OCR.md). Önceki Latin-only test sonuçları tarihsel kayıttır.
