# Ücretsiz OPUS-MT model denemesi — 28 Ekim 2026

Model: Helsinki-NLP/opus-mt-tc-big-en-tr. Resmî model kartı ve CC-BY-4.0 lisansı: https://huggingface.co/Helsinki-NLP/opus-mt-tc-big-en-tr . Ağırlık yaklaşık 470 MB. Proje OPUS-MT / Helsinki-NLP ekibine aittir; bu uygulama tarafından eğitilmedi. Dağıtım sırasında model lisansı ve atfı korunmalıdır.

Kurulu LibreTranslate ortamından ayrı `.verification/opus-env` oluşturuldu. Model ve kütüphaneler `.verification/` altında, Git dışında. Uygulama/servis yapılandırması değiştirilmedi. Model revision, ağırlık SHA256 ve ölçülen çıktılar `OPUS_MODEL_DENEMESI.json` dosyasında.

| Sentetik örnek | Argos 1.5 önceki sonuç | OPUS-MT sonucu |
| --- | --- | --- |
| İçindekiler ve eser miktar yer fıstığı | Yer fıstığı kayboldu | süt, buğday unu, soya ve yer fıstığı korundu |
| Besin değerinde fat/salt | fat → şişman | yağ/tuz doğru bağlamda, sayılar korundu |
| Restoranda domuz/alkol sorusu | Geçti | Geçti |

OPUS örnekleri yaklaşık 1–2 saniyede sonuçlandı (model yükleme süresi hariç). Üç sınırlı sözcük/bağlam örneği geçti; genel kalite, tüm diller, gerçek ambalajlar, OCR hataları, alerjen güvenliği veya helal doğruluğu kanıtlanmadı. İngilizce→Türkçe aday test edildi; Türkçe→İngilizce ve diğer diller bu modelin kapsamı değildir. Tam görev 25 bitmedi.

## Tekrar çalıştırma

Depo kökünde:

```text
.verification/opus-env/Scripts/python.exe tools/LocalTranslation/evaluate_opus.py
```

Bu komut var olan yerel dosyaları kullanır. Açık `--download` seçeneği eksik dosyaları resmî kaynaktan indirir. İlk indirilen revision kaydedilir ve sonraki denemelerde korunur. Windows Unicode dosya adı sorunu için yalnız SentencePiece model klasörünün mevcut kısa yolu kullanılır. İndirme normal klasöre yapılır; symlink/sistem izinleri değiştirilmez.

Araç yalnız repodaki sentetik test metinlerini işler. Metin HF'ye gönderilmez; HF yalnız model dosyaları indirmek için kullanılır. Çıktı yerel rapora yazılır. Geçen örneklerin dil uzmanı tarafından ayrıca incelenmesi gerekir.

## Sonraki entegrasyon işi

Bu deneme henüz C# API'ye bağlı değildir. Bağlamak için yalnız EN→TR isteklerini bu modele yönlendiren yerel servis adaptörü, uzun metinleri kesmeden işleme, satır/sayı/E kodlarını koruma testleri ve model yokken açık hata akışı gerekir. Diğer dil çiftleri için mevcut motor ayrı kalmalıdır. Otomatik sessiz motor değişikliği yapılmadı.
