# Kamera ile ürün barkodu okuma

Katalog ekranındaki **Kamerayla barkod tara** düğmesi Android kamera izni ister. Ambalaj barkodu cihazda ZXing.Net.Maui.Controls 0.10.4 ile okunur; Open Food Facts servisine yalnız barkod gönderilir. Kamera görüntüsü sunucuya yüklenmez. QR bağlantıları ürün barkodu olarak kabul edilmez.

İlk sürüm sayısal 8, 12, 13 ve 14 haneli barkodları kabul eder. Sık kullanılan EAN-13 ve UPC-A ambalajlarını deneyin. Her ülkenin her ürünü OFF üzerinde bulunmayabilir. Sıkıştırılmış UPC-E numaraları bu sürümde UPC-A biçimine dönüştürülmez.

Aynı okumadan birden fazla ürün ekranı açılmaz. Ekrandan ayrılınca kamera durdurulur; ürün ekranından geri dönülünce tekrar açılır. İzin verilmezse veya kamera desteklenmiyorsa elle giriş kullanılabilir. Ürün ekranı mevcut içerik değerlendirmesini ve sertifika bilgilerini gösterir; barkod taraması helal sertifikası doğrulaması değildir.

## Elle kontrol

1. Web/API projesini 5088 portunda çalıştırın; Android emülatörü API'ye 10.0.2.2 üzerinden ulaşır.
2. MAUI uygulamasında Katalog → Kamerayla barkod tara seçin, kamera iznine izin verin.
3. Net ve aydınlık bir ambalaj barkodu gösterin. Ürün varsa marka, ad ve içerik açılmalı; yoksa bulunamadı mesajı çıkmalı.
4. Geri dönün, tekrar okutun; tek ürün ekranı açıldığını kontrol edin.
5. Kamera iznini reddederek elle 3017620422003 girin ve ürün ekranını kontrol edin.
6. Kapat düğmesiyle taramadan çıkın. Android kamera kullanım göstergesi kapanmalı.

Barkod doğrulama konsol testleri tests/BarcodeChecks içindedir. Android derlemesi kontrol edilmiştir. Gerçek kamera görüntüsünden okuma, izin reddi ve geri dönüş davranışları cihaz üzerinde ayrıca kontrol edilmelidir. Emülatörde kamera görüntüsü için sanal sahne veya webcam yapılandırması gerekir.
