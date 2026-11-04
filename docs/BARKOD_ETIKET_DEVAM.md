# Barkod sorgusundan etiketle devam

4 Kasım 2026: Mobilde ürün bulunmaması veya kaynak hatası yanıtının yanında yerel API bağlantı hatası/bozuk JSON durumunda da paketli etiket OCR/elle giriş yolu sunulur. Ürün bulunup özgün içindekiler boş veya yalnız boşluksa boş çeviri formu yerine etiket okuma/elle giriş düğmesi gösterilir. İçeriği olan ürünlerin çeviri bağlantısı korunur. Fotoğraf/metin ürün kaydına otomatik bağlanmaz ve kaynak içeriğinin yerine geçirilmez.

Webde boş içerikte veya başarısız sorguda /ingredients elle giriş bağlantısı vardır. Başarısız sorguda /translate tam metin bağlantısı da korunur. Webde kamera OCR eklendiği iddia edilmez.

OCR ekranında paket modu seçili açılır; kamera/galeri otomatik başlatılmaz. Metin kullanıcı tarafından kontrol edilir. OCR/elle girişe ulaşmak analiz/çeviri sunucularının çevrimdışı çalıştığı anlamına gelmez; mobil açıklama bu ayrımı belirtir.

Elle kontrol: (1) bulunmayan barkod, (2) API kapalı, (3) bulunan ama içeriksiz ürün, (4) içerikli ürün. İlk üçünde etiket yolu; dördüncüde dolu özgün metinle çeviri olmalı. Mobil etiket düğmesi paket modu açmalı; kapat/geri çalışmalı. Yerel/demo ürün kayıtları ve favoriler değiştirilmez.

Doğrulama: web build ve Android TestBuild Compile hedefi 0 hata / 0 uyarı ile geçti. Mevcut OffChecks testleri geçti (eşleme, eksik içerik, 404/429/503, önbellek, eşzamanlılık ve istek sınırı). Bunlar yeni mobil dokunma akışını doğrulamaz. Bu görevde yeni APK paketlenmedi; yukarıdaki cihaz denemeleri bekliyor.

## Tam APK kontrolü — 4 Kasım 2026

Tam Android TestBuild paketlemesi 0 hata ve bilinen 4 XA4301 uyarısıyla tamamlandı. APK imzası apksigner ile doğrulandı; paket kimliği com.companyname.helalharam, sürüm 0.1.0 (2), arm64-v8a ve x86_64 mimarileri aapt ile kontrol edildi. artifacts/android-test/HelalRehber-0.1.0-test.apk ve SHA256.txt yenilendi; dosyalar Git dışındadır.

Pixel_7 API35 emülatöründe adb install -r başarılı oldu, veri silinmedi. Uygulama işlemi başlatıldı; son AndroidRuntime hata kayıtlarında çıktı yoktu. Ancak ekran denemesi tamamlanmadı: görüntü yakalama ilk hatadan sonra pencere bağlantısı yenilendiğinde de “window crop is outside captured monitor” hatası verdi. Etiket düğmesi ve geri dönüş testi geçti sayılmaz. Önceki Compile/servis testleri yalnız kendi kapsamlarında geçerlidir.

Kalan elle kontrol: emülatör bildirim perdesini kapatın, Katalog'da örnek barkod sorgulayın. API kapalıyken hata ekranındaki etiket/elle giriş düğmesi paket modunu açmalı. Fotoğraf çekmeden Kapat'a basınca ürün ekranına dönülmeli. Ardından içeriksiz ve içerikli ürün durumları ayrı denenmeli. Kamera/galeri/izin testi bu denemenin kapsamı değildir.
