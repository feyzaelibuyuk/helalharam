# OCR koşul testi — 28 Ekim 2026

Pixel_7 API 35 emülatöründe uygulamanın gerçek OcrImage → ScriptOcr → ML Kit Latin hattı çalıştırıldı. İki sabit yapay etiket (İngilizce ve Türkçe) kullanıldı; gerçek kullanıcı fotoğrafları okunmadı. Toplam 15 deterministik örnekte tüm ölçümler alındı; aşağıdaki hatalar başarısız okumalar olarak korunmuştur.

| Koşul | Ham karakter hatası | Normalize hata | Kaybolan kritik ifadeler |
| --- | ---: | ---: | --- |
| Temiz etiket | 0.0% | 0.0% | — |
| Loş görüntü | 0.0% | 0.0% | — |
| Çok loş + gürültü | 6.0% | 1.7% | — |
| Silik baskı | 0.0% | 0.0% | — |
| Orta bulanıklık | 0.0% | 0.0% | — |
| Ağır bulanıklık | 100.0% | 100.0% | wheat, milk, peanuts, gelatin, e471 |
| 12° dönme / kadraj kaybı | 13.4% | 15.0% | — |
| 25° dönme / kadraj kaybı | 44.8% | 48.3% | wheat, gelatin, e471 |
| Parlama ile kapanan yazı | 35.8% | 35.0% | wheat, milk, peanuts |
| Kesilmiş etiket | 19.4% | 18.3% | milk, peanuts |
| Küçük yazı | 0.0% | 0.0% | — |
| Düşük JPEG kalitesi | 0.0% | 0.0% | — |
| Boş görüntü (negatif kontrol) | 0.0% | 0.0% | — |
| Türkçe temiz etiket | 5.9% | 0.0% | — |
| Loş + bulanık + gürültü | 10.4% | 10.0% | wheat, milk, peanuts |

Ham karakter hatası Levenshtein uzaklığı / kaynak metin uzunluğudur. Normalizasyon noktalama, büyük/küçük harf ve aksanları kaldırır; bu metnin birebir doğru olduğu anlamına gelmez. Türkçe temiz örnekte `fıstığı` gibi harfler yanlış okunduğundan ham hata vardır; normalize hata sıfır diye kusursuz Türkçe OCR denemez. Kritik kelime kontrolü sınırlı bir listedir; eksik çıkmaması tüm metnin doğru olduğunu kanıtlamaz. Örneğin 12° senaryoda kritik liste korunurken metnin başı kayboldu.

## Ne söyleyebiliriz?

Bu sabit örneklerde loş görüntü, silik baskı ve orta bulanıklık okunabildi. Çok bulanıklıkta metin çıkmadı. Parlama/kesilmiş kadraj/birleşik bozulmada süt, yer fıstığı gibi içerikler kayboldu. Dolayısıyla OCR sonucunda metin bulunması başarı veya güvenli içerik sonucu değildir. Otomatik ışık güven eşiği bu testten üretilmedi. Pozitif/negatif metin uydurma ya da sessiz kelime düzeltme eklenmedi.

Görüntü parlaklığı 0–255 gri değerleridir, fiziksel ortamın lüks ölçümü değildir. Bulanıklık küçültüp büyütme ile, gürültü sabit tohumlu piksel değişimiyle simüle edildi. Dönme senaryolarında görüntü 1800×500 sabit tutulduğundan kadraj taşması da vardır; bunlar saf açı dayanıklılığı ölçümü değildir. Font, etiket, mesafe ve cihaz çeşitliliği sınırlıdır. Çince/Japonca/Korece/Hintçe kötü ışık testleri bu koşul setinde yoktur.

Gerçek ışık sınırı için aynı fiziksel etiket, sabit mesafe ve kamera ile örneğin 20/50/100/300/500 lüks kontrollü çekimler, parlak/kavisli ambalaj ve farklı telefonlar ayrıca denenmelidir. Bu değerler destek eşiği veya başarı vaadi değil, ilerideki ölçüm planıdır. Onaysız eksik metin uygunluk değerlendirmesine geçmemeli; mevcut elle fotoğrafla karşılaştırma onayı sürer.

## Uygulama yönlendirmesi

TR/EN/AR fotoğraf ipuçları: etiketi tam kadraja al, sabitle, odağı doğrula, loşsa ışığı artır, parlama varsa açıyı değiştir, silik/bulanık görüntüyü yeniden çek. Okuma tamamlandığında artık “OCR metni taslaktır; kelime/satır eksik olabilir” denir. Onay metni tüm etiketle karşılaştırmayı ister; uzun metin küçük ekranda taşmasın diye onay satırı Grid içinde sarılır. Bunlar kaliteyi otomatik ölçen bir eşik değildir.

## Tekrar çalıştırma

Yalnız ayrı QA paketiyle `-p:OcrQualityTests=true -p:ApplicationId=com.companyname.helalharam.ocrqa` Debug derlemesi kullanılır. Koşul testi alıcısı normal derlemede, Release'te ve TestBuild paketinde derlenmez. QA APK emülatöre kurulduktan sonra `python tests/run-ocr-quality-android.py` çalıştırılır. Her koşul yeni çalışma kimliğiyle doğrulanır; eski sonuçlar sayılmaz. Ham rapor OCR_KOSUL_SONUCLARI.json'dadır. Ölçüm tamamlanınca QA uygulaması kaldırılır; asıl uygulama verileri korunur.

QA paketlemesi: 0 hata, bilinen 4 XA4301 OCR uyarısı. Gerçek kamera/galeri izin akışı ve fiziksel ışık ölçümü yapılmadı. Fotoğraf ipuçlarının normal Android Compile kontrolü ayrıca raporlanır.

Resmî görüntü önerileri: https://developers.google.com/ml-kit/vision/text-recognition/v2/android#input-image-guidelines . Google yeterli karakter piksel boyutu ve netliği vurgular; kabul edilmeyen sonuçta yeniden çekim önerir.

Normal Android TestBuild Compile kontrolü 0 hata/0 uyarıyla geçti. MSBuild Compile öğelerinde OcrQualityAndroid ve OcrAndroidSmoke bulunmadığı doğrulandı. Ayrı com.companyname.helalharam.ocrqa paketi sonuçlar toplandıktan sonra kaldırıldı; asıl uygulama kaldırılmadı. İpuçları/onay metni değişikliğinin yeni tam APK paketlemesi ve görsel kontrolü bu görevde yapılmadı. JSON süreleri yalnız native OCR çağrısını kapsar; kamera/galeri veya tüm işlem süresi değildir.
