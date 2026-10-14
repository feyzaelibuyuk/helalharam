# Türkiye yemek detayları

İlk restoran kapsamı Türkiye'dir. Ülke, şehir, şube, kayıt dili, içerik beyanı ve kontrol tarihi RestaurantMetadata tablosundadır; mevcut Restaurants tablosu ve eski kayıtlar korunur. Başlangıç tekrar çalışınca kayıtlar çoğalmaz. Bilinen başka ülke kayıtları bu sürümün aramasında/detayında gösterilmez; ülkesi bilinmeyen eski kayıtlar varsayım yapılmadan görünür. Şehir filtresi kullanıldığında şehri bilinmeyen kayıtlar eşleşmez.

Mevcut Örnek Esnaf Lokantası (İstanbul) ve Örnek Izgara Salonu (Ankara) kayıtları, şehir ve Kurgu Merkez Şube dahil DEMO'dur; gerçek işletme veya uygunluk önerisi değildir. Gerçek Türkiye restoran verisi henüz bağlanmamıştır. Diğer eski kayıtların şehir/ülke/kapsamı uydurulmaz.

Web: ana sayfada Restoran yemekleri → şehir/yemek araması → Yemek detayını incele; doğrudan /restaurants/{id}. Arama bağlantısı /?mode=restaurants&city=istanbul şeklindedir.
Mobil: Katalog → Restoran yemekleri → Türkiye şehir alanı → Ara → Yemek detayını incele. Ürün ülke filtresi ve restoran şehir filtresi ayrı akışlardır.

API: /api/restaurants?q=...&city=... önceki kayıt alanlarını koruyarak yeni metadata alanlarını ekler. /api/restaurants/{id} restoran kaydı, işletmeye sorulabilecekler ve belirsizlik notu döndürür; bulunmayan/kapsam dışı kayıt 404 verir.

Detayda restoran/şehir/şube, kayıtlı içerik beyanı, ayrı olarak doğrulanmamış olası içerikler, bilgi kaynağı ve kontrol tarihi gösterilir. İşletmenin kayıttaki soruları korunur; mevcut menü anahtar kelime kurallarının soruları eklenir. Yemek adı tam tarifin kanıtı değildir. Restoran sertifikası doğrulanmaz; uygunluk onayı verilmez. Kaynaklı alternatif yemek verisi yokken ürün/yemek uydurulmaz; içeriği açık alternatif veya belirsiz bileşeni çıkarma sorusu sunulur.

## Kontrol

1. Blazor uygulamasını çalıştırın. Webde /?mode=restaurants&city=istanbul ile tek kurgu çorba kaydı beklenir; Ankara ile tek kurgu tavuk kaydı, Bursa ile boş sonuç beklenir.
2. Yemek detayında demo uyarısını, şehri/şubeyi, tam içerik olmadığını, kaynak notunu ve işletmeye sorulabilecekleri kontrol edin.
3. Şehir alanını arama formunda değiştirip Ara'ya basın; restoran görünümü korunmalıdır. Barkod formu bu alanı göndermez.
4. Mobilde aynı arama/detay ve sunucu kapalıyken hata/tekrar dene akışını kontrol edin.

Otomatik testler tests/DishChecks: eski kayıt korunması, eksik metadata tablosu kurulumu, tekrarlı başlangıç, Türkiye kapsamı, Türkçe şehir/yemek eşleştirmesi, demo ayrımı ve eksik kayıt. Mevcut ürün CatalogChecks de geçti. Web API/HTML ve tarayıcı kontrolleri yapıldı. Android derlemesi kontrol edildi; mobil yemek ekranındaki tıklama akışı ayrıca cihazda denenmelidir. Mevcut ML Kit paketleme XA4301 uyarısı sürmektedir.
