# Barkod sorgusundan etiketle devam

4 Kasım 2026: Mobilde ürün bulunmaması veya kaynak hatası yanıtının yanında yerel API bağlantı hatası/bozuk JSON durumunda da paketli etiket OCR/elle giriş yolu sunulur. Ürün bulunup özgün içindekiler boş veya yalnız boşluksa boş çeviri formu yerine etiket okuma/elle giriş düğmesi gösterilir. İçeriği olan ürünlerin çeviri bağlantısı korunur. Fotoğraf/metin ürün kaydına otomatik bağlanmaz ve kaynak içeriğinin yerine geçirilmez.

Webde boş içerikte veya başarısız sorguda /ingredients elle giriş bağlantısı vardır. Başarısız sorguda /translate tam metin bağlantısı da korunur. Webde kamera OCR eklendiği iddia edilmez.

OCR ekranında paket modu seçili açılır; kamera/galeri otomatik başlatılmaz. Metin kullanıcı tarafından kontrol edilir. OCR/elle girişe ulaşmak analiz/çeviri sunucularının çevrimdışı çalıştığı anlamına gelmez; mobil açıklama bu ayrımı belirtir.

Elle kontrol: (1) bulunmayan barkod, (2) API kapalı, (3) bulunan ama içeriksiz ürün, (4) içerikli ürün. İlk üçünde etiket yolu; dördüncüde dolu özgün metinle çeviri olmalı. Mobil etiket düğmesi paket modu açmalı; kapat/geri çalışmalı. Yerel/demo ürün kayıtları ve favoriler değiştirilmez.

Doğrulama: web build ve Android TestBuild Compile hedefi 0 hata / 0 uyarı ile geçti. Mevcut OffChecks testleri geçti (eşleme, eksik içerik, 404/429/503, önbellek, eşzamanlılık ve istek sınırı). Bunlar yeni mobil dokunma akışını doğrulamaz. Bu görevde yeni APK paketlenmedi; yukarıdaki cihaz denemeleri bekliyor.
