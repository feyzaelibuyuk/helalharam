# Akış tutarlılığı kontrolü — 27.10.2026

Bu tur 30. görev sonrası birleştirilmiş main üzerinden `fix/akis-tutarliligi` dalında yapıldı.

## Düzeltilenler

1. Mobil kişisel uyarılar, tercihler sayfasından değiştirip geri dönüldüğünde yenilenir. Yalnız kişisel panel yeniden hesaplanır; OCR metni ve çeviri sonucu korunur. OFF/API/çeviri servisine sırf tercih değişti diye yeni istek gönderilmez. Kaydetme/temizleme başarıyla tamamlanırsa yerel sürüm değişir. SecureStorage okunamazsa eski uyarılar bırakılmaz, kontrolün uygulanamadığı gösterilir ve sonraki görünmede yeniden denenir. Ekrandan kaldırılan paneller olay aboneliğini bırakır; geç kalan okuma kaldırılmış paneli yeniden doldurmaz.
2. Yerel katalog ürününde web ve mobil **İçindekilerin tamamını çevir** bağlantısı eklendi. Önce özgün metin, yoksa kayıtlı içerik bütünüyle aktarılır. Web adresi `/translate/catalog/{id}`. Formu göndermede kullanıcının değiştirdiği metin katalog metniyle ezilmez. Eksik ürün, elle metin girme açıklamasıyla ele alınır.
3. Yerel üründe eski “çeviri henüz bağlı değil” ifadesi kaldırıldı. Çeviri kendiliğinden başlamaz: metin kontrolü/onayı ve çalışan yerel motor gerekir. Demo ürünün metnini aktarmak onu gerçek/doğrulanmış ürün yapmaz.
4. Ürünlerde helal kaynak değerlendirmesinin başlığı kişisel alerjen/diyet sonucundan ayrıldı. Mobil çeviri, kişisel paneller okunurken sayfa kapatıldıysa iptal edilmiş işleme devam etmez.

## Kontrol sonucu

- Web derlemesi: 0 hata / 0 uyarı.
- Android APK derlemesi: 0 hata / önceden mevcut 4 XA4301 OCR kitaplığı uyarısı.
- `tests/flow-integration-http.py`: ürün→çeviri bağlantısı, tam özgün metin aktarımı, gönderimde değiştirilmiş metnin korunması, CSRF, eksik ürün, eski mesajın kalkması geçti.
- `tests/food-preferences-http.py`: mevcut kişisel filtre/çerez/CSRF/örnek alerjen kontrolleri geçti.
- Tarayıcıda ürün detayından yeni bağlantı açıldı, özgün metnin tamamı çeviri formunda görüldü. Bu kontrolde yerel LibreTranslate motoru kapalıydı; kaynak metin korunurken uygun servis-kapalı mesajı gösterildi. Tam ve doğru çeviri kalitesi bu testte doğrulanmış sayılmaz.
- `adb devices` listesinde açık Android cihazı yoktu. Mobil tercih→geri dönüş, çoklu panel, SecureStorage hata/iptal ve uzun kullanım bellek davranışı gerçek cihazda ayrıca kontrol edilmelidir; derleme bunların etkileşim testinin yerine geçmez.

Normal test web sunucusu 5095 portunda ayrı `.verification/flow-visual` veritabanıyla açıldı. Gerçek kullanıcı veritabanı silinmedi, yönetici parolası okunmadı/değiştirilmedi. Test sunucusu ve geçici tarayıcı sekmesi işlem sonunda kapatılır. Main'e otomatik merge veya push yapılmaz.
