# Uygulama içi inceleme bildirimleri

`/community/notifications` yalnız korumalı topluluk çerezinin sahibine ait son 100 inceleme/geri çekme olayını gösterir. Kimlik sorgu veya form alanından alınmaz. İç yönetici notları, inceleyenin kimliği ve diğer kullanıcıların kayıtları döndürülmez. Topluluk yollarının mevcut no-store kuralı geçerlidir.

Ekran açıldığında/yenilendiğinde sorgulanır. Push, okunma sayacı, otomatik arka plan kontrolü, favori değişiklikleri veya bilgi düzeltme sonuçları bu sürümde yoktur. Geçmiş olay güncel durum değildir; Paylaşımlarım bağlantısı güncel kaydı açar. İnceleme helal/sertifika onayı değildir.

Mobil Bildirimler sekmesindeki düğme aynı topluluk WebView kökenini açar. Gerçek cihazda topluluk gönderimi, yönetici incelemesi ve bildirimi yenileme denemesi yapılmalıdır. Web ve mobil kimlikleri eşitlenmez.

Kontrol: Web build ve Android Compile hedefi 0 hata/0 uyarı ile geçti. İzole HTTP denemesinde sentetik paylaşım/inceleme olayı kullanıldı: sahip görürken ikinci çerez kimliği görmedi, no-store başlığı korundu, özel not ve inceleyen bilgisi HTML içinde bulunmadı. Sentetik kayıt temizlendi. APK paketleme ve gerçek mobil tıklama/çerez paylaşımı bu görevde denenmedi.
