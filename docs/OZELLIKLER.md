# Helal Rehber — özellikler ve geliştirme sırası

1. C# ve .NET MAUI ile Android emülatöründe çalışan mobil uygulama.
2. Beslenme profili: ilk sürüm helal; diğer dini profiller kaynaklı kurallar hazırlandığında eklenir.
3. Ürün adı, marka veya barkodla arama.
4. Kamerayla barkod okuma ve katalogdan ürün bilgisi getirme.
5. İçindekiler etiketini OCR ile okuma; kullanıcı metni düzeltip onaylar.
6. Uygun / uygun değil / şüpheli / yetersiz bilgi sonuçları, gerekçe ve kaynak tarihi.
7. Aynı kategoriden kaynakları kontrol edilmiş alternatif ürün önerileri.
8. Açık yemeklerde yemek adı, tarif veya içerik girme.
9. Menü fotoğrafını OCR ile okuma; çıkan yemekleri kullanıcıya onaylatma.
10. Menüdeki yemek ve belirtilen içeriklere göre işletmeye sorulacak sorular üretme.
11. Uygun alternatif yemek veya tarif önerileri.
12. Şube bazında restoran kataloğu; sertifika, işletme beyanı ve kullanıcı deneyimini ayrı gösterme.
13. Kullanıcı deneyimleri: dükkân, şube, yemek, ziyaret tarihi, sorulan soru, alınan yanıt ve isteğe bağlı kaynak.
14. Kullanıcı deneyimi doğrulanmış uygunluk sayılmaz; yayın öncesi moderasyon ve yanlış bilgi bildirimi.
15. Ürün ve restoran favorileri.
16. Favori değerlendirmesi değiştiğinde, eksik bilgi tamamlandığında veya katkı sonuçlandığında bildirim.
17. Katkı yönetimi, kaynak kontrolü ve işlem geçmişi.
18. Gizlilik, isteğe bağlı konum, kamera izni ve hesap/veri silme.

## Branch görevleri

- feat/maui-baslangic: uygulama iskeleti ve yerel demo akışları.
- feat/profil-kurallar: kaynaklı değerlendirme modeli ve profil tercihleri.
- feat/urun-katalog: ürün verisi, arama, kaynak ve alternatif eşleştirme.
- feat/barkod: gerçek kamera taraması.
- feat/etiket-ocr: etiket OCR ve metin onayı.
- feat/menu-ocr: menü OCR, yemek ayrıştırma ve soru akışı.
- feat/restoranlar: kaynaklı restoran/şube/yemek kataloğu.
- feat/topluluk: kalıcı katkılar ve moderasyon.
- feat/favoriler-bildirim: favoriler ve değişiklik bildirimi.

Her görev güncel main'den açılır. Derleme ve ilgili akış kontrolünden sonra main'e birleştirilir.
Demo kayıtlar gerçek ürün, restoran, sertifika veya dini hüküm değildir.

## Uyarı dili
Paketli üründe şüpheli bileşen varsa Şüpheli gıda ve gerekçe gösterilir; kullanıcı üreticiye yönlendirilmez. Bilgi eksikliği tek başına haram hükmü değildir. Restoran ve açık yemeklerde İşletmeye danışın ve sorulacak sorular gösterilir.
