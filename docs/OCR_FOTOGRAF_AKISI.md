# OCR fotoğraf akışı

Yeni fotoğrafın OCR işlemi tamamlanmadan mevcut editör ve önizleme değiştirilmez. Başarılı, boş olmayan ve sınır içindeki metin geldiğinde yeni metin ile ona ait kontrast öncesi önizleme birlikte yerleştirilir. Başarısız okuma veya bozuk dosya önceki düzenlenmiş metni silmez. Hata mesajı önceki içeriğin yeni fotoğrafın sonucu olmadığını açıklar.

Yeni fotoğraf seçimi, kontrast/dil/model/metin değişikliği önceki karşılaştırma onayını sıfırlar. Eski metin korunsa bile tekrar elle kontrol/onay gerekir. Boş olmayan OCR çıktısı hâlâ eksik veya yanlış olabilir; bu düzenleme bir doğruluk eşiği değildir.

## Elle kontrol planı

1. Galeriden yalnız test için oluşturulan okunabilir etiketi seçin; metin ve önizlemenin aynı fotoğrafa ait olduğunu görün.
2. Metinde bir değişiklik yapın. Boş test görselini seçin; başarısızlık mesajı, önceki metin ve önceki önizleme korunmalı.
3. Galeri seçimini iptal edin; metin kaybolmamalı, onay kutusu açık kalmamalı.
4. Kontrastı açıp yeniden bir test görseli seçin; işlenmiş kopya uyarısı görünmeli, önizleme kontrast öncesi olmalı.
5. Kamera izni/çekim/iptal ve sayfadan ayrılma gerçek cihazda ayrıca denenmeli. Test izni veya gerçek kişisel fotoğraf paylaşımı otomatik yapılmaz.

Bu görevde kullanılan iki görsel kodla üretilmiş etikettir, gerçek kullanıcı fotoğrafı değildir. Fiili sonuçlar aşağıya eklenir.
