# Open Food Facts bağlantısı

## Kullanım

Web ana sayfasında Gerçek barkod sorgula alanına ambalajdaki barkodu girin.
Mobilde Katalog → Gerçek ürünü getir. Örnek canlı sorgu: 3017620422003.
Gerçek marka, ad, ambalaj fotoğrafı, özgün içerik, içerik dili, satış ülke/kategori etiketleri ve kaynak bağlantısı gösterilir.
Yerel demo katalog ayrı tutulur; gerçek ürünler demo veritabanına veya doğrulanmış alternatiflere otomatik eklenmez.
Bu sürüm dış API'de barkod sorgular; marka/ad ile dış arama ve gerçek kamera taraması sonraki görevdir.

## Değerlendirme ayrımı

OFF topluluk verisidir. API'deki helal etiketi, kontrol edilmiş sertifika değildir.
İçerik metni yoksa veya dili desteklenmiyorsa Yetersiz bilgi gösterilir. Desteklenen dillerde sınırlı kaynaklı kurallar uygulanır; ayrıntılar docs/ICERIK_KURALLARI.md dosyasındadır.
İçerik bulguları gerekçe ve kaynaklarıyla gösterilir; ürünün sertifikası ve üretim koşulları otomatik doğrulanmaz.
Kaynak güncellemesi ve veri alınma tarihi gösterilir; bunlar helal kontrol tarihi olarak kullanılmaz.
Birden çok satış ülkesi saklanır; barkod önekinden ülke/üretim yeri tahmin edilmez.
Özgün içerik değiştirilmez ve otomatik çeviri yapılmış gibi gösterilmez.

## Servis

GET /api/off/products/{barcode}
200 found, 400 invalid_barcode, 404 not_found, 429 rate_limited, 502 unavailable.
8, 12, 13 ve 14 rakamlı barkodlar kabul edilir. UPC/EAN baştaki sıfır varyantları OFF kurallarına göre aynı kayda eşlenir; kaynak barkodu gösterilir.
Sonuçlar bellekte 24 saat, bulunamayan ürünler 10 dakika tutulur. Hatalar ürün bulunamadı gibi önbelleğe alınmaz.
Aynı barkod için eşzamanlı sorgular tek dış isteğe dönüşür; dakikada en fazla 15 dış ürün isteği gönderilir.
Önbellek süreç yeniden başlatıldığında sıfırlanır. Ağ hatası ve zaman aşımı ayrı mesajlarla gösterilir.
API anahtarı gerekmez. Dış servise yalnız barkod ve istenen alanlar gönderilir; yazma/fotoğraf yükleme yapılmaz.

## Kaynak ve test

Resmî doküman: https://openfoodfacts.github.io/openfoodfacts-server/api/
Veri: Open Food Facts, ODbL; fotoğraflar CC BY-SA. Kaynak/atıf web ve mobil ekranlarında görünür.
Üretim kaynağı world.openfoodfacts.org. Otomatik testler sahte HTTP yanıtlarıyla çalışır; üretim API'sini tüketmez.
Canlı bağlantı tek gerçek barkodla kontrol edildi.

Test: dotnet run --project tests/OffChecks/OffChecks.csproj
Kapsam: özgün dil/metin, eksik içerik, etiket/sertifika ayrımı, farklı barkod yanıtı,
önbellek ve eşzamanlılık, geçersiz girdi, 404/429/503 ve istek sınırı.
