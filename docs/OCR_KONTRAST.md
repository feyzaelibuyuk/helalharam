# İsteğe bağlı OCR kontrastı — 28 Ekim 2026

Aynı 15 sabit yapay görüntü gerçek Android OcrImage→ScriptOcr→ML Kit Latin hattında normal ve kontrastlı kopyayla okundu. Toplam 30 native okuma ölçüldü. Veri kaynağı OCR_KONTRAST_SONUCLARI.json. Gerçek telefonun ışık seviyesi veya genel doğruluk puanı değildir.

| Örnek | Normalize hata önce | Sonra | Kontrast sonrası kayıp kritik ifadeler |
| --- | ---: | ---: | --- |
| clear | 0.0% | 0.0% | — |
| dim | 0.0% | 0.0% | — |
| very_dim_noisy | 1.7% | 0.0% | — |
| faint_print | 0.0% | 0.0% | — |
| moderate_blur | 0.0% | 0.0% | — |
| severe_blur | 100.0% | 100.0% | wheat, milk, peanuts, gelatin, e471 |
| tilt_12 | 15.0% | 15.0% | — |
| tilt_25 | 48.3% | 48.3% | wheat, gelatin, e471 |
| glare | 35.0% | 35.0% | wheat, milk, peanuts |
| cropped | 18.3% | 18.3% | milk, peanuts |
| tiny_print | 0.0% | 0.0% | — |
| jpeg_low | 0.0% | 0.0% | — |
| blank | 0.0% | 0.0% | — |
| turkish_clear | 0.0% | 1.6% | yer fistigi |
| combined_dim_blur | 10.0% | 5.0% | wheat, peanuts |

Normalize hata iki örnekte azaldı (çok loş/gürültülü ve birleşik loş/bulanık), temiz Türkçe örnekte arttı, diğerlerinde aynı kaldı. Türkçe örnekte `yer fistiğı` bu kez `yer fistiğ` oldu; kritik ifade kontrolü kaybı yakaladı. Birleşik örnekte süt eşleşmesi geri geldi ama buğday/yer fıstığı hâlâ yanlış; iyileşme başarılı tam okuma değildir. Ham karakter hatası ve okunan metinler JSON'da ayrıca tutulur. Noktalama/aksan normalizasyonunun sınırları OCR_KOSUL_TESTI.md'de açıklanır.

## Kullanım

Android OCR ekranında **Kontrastı artır (isteğe bağlı)** seçeneği bulunur, varsayılan kapalıdır. Sonraki çekim/galeri okumasına uygulanır; mevcut editör metnini kendiliğinden yeniden yazmaz. Seçenek değiştirilince eski karşılaştırma onayı sıfırlanır. İşlem sürerken seçenek kilitlenir; sayfadan ayrılma iptali korunur. Sonuç mesajı işlenmiş kopyanın kullanıldığını belirtir; önizleme kontrast öncesi normalize görüntüdür. Orijinal fotoğraf dosyası değiştirilmez, kontrastlı kopya kalıcı saklanmaz ve yüklenmez. Bütün metin yine elle karşılaştırılıp onaylanmalıdır.

Kullanıcıya bazı harflerin daha kötü okunabileceği söylenir. Otomatik “en iyi sonuç” seçimi, sessiz kelime düzeltme veya kalite/helal/alerjen güvenliği onayı yoktur. Parlama ile tamamen örtülen veya kadraj dışındaki yazı geri getirilemez; yeniden çekim gerekir. Bu yöntem fiziksel bir lüks eşiği belirlemez.

## Teknik sınır

C# Android görüntü işleme: normalize edilmiş en fazla 2048×2048 piksel; gri ton, %0,5/%99,5 histogram uçlarına göre doğrusal kontrast, neredeyse tekdüze görüntüde işlem yapmama; JPEG95 kopya. Döngüler iptal kontrolü yapar, native bitmapler dispose edilir. Android dışındaki platformlarda seçenek gösterilmez. Ek hesap/ücret/servis yoktur.

Ayrı QA APK paketlemesi 0 hata, bilinen 4 XA4301 uyarısıyla geçti. Normal TestBuild Android Compile kontrolü 0 hata/0 uyarı verdi. Gerçek kontrast algoritması 15 koşulda test edildi; yeni checkbox'ın kamera/galeri dokunma akışı ve yeni normal tam APK bu görevde denenmedi/paketlenmedi. Test alıcısı TestBuild/Release'ten dışlanır; ayrı QA uygulaması ölçüm sonrası kaldırılır. Tekrar test: OCR_KOSUL_TESTI.md'deki opt-in QA kurulumu ve tests/run-ocr-quality-android.py.
