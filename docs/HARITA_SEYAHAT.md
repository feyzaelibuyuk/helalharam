# Harita entegrasyonu ve seyahat — Trello 27

## Kullanım ve kapsam

Web: üst menü → Harita ve seyahat (/travel). Mobil: Menü → Yakındaki restoranlar veya Profil → Harita ve seyahat. Mobil ekran, C# web sayfasını MAUI WebView içinde kullanır; native C# konum düğmesi eklenmiştir. Bilgisayardaki API/webin 5088 portunda açık olması ve internet gerekir; bağımsız telefonda localhost/emülatör adresiyle çalışacağı iddia edilmez.

Şehir araması GeoNames cities15000 paketindeki 34.188 şehir/başkent kaydını kullanır. Kaynak ad, alternatif adlar (farklı alfabeler dahil), ASCII/aksan eşleştirme ve ülke koduyla seçim yapılır; en fazla 20 aday gösterilir. Tüm küçük yerleşimler yoktur. Alternatif olarak enlem/boylam girilebilir veya Konumumu forma getir düğmesi kullanılabilir. İzin reddinde şehir/manuel giriş çalışır; sürekli/arka plan konumu yoktur. Konum forma geldikten sonra ayrıca onay ve arama gerekir.

Arama merkezi 0,01 dereceye yuvarlanır (enleme göre yaklaşık 1 km grid); 3 km çevre içinde restaurant/cafe/fast_food/food_court ve diet:halal=yes/only OSM kayıtları sorgulanır. En fazla 100 kaynak elemanı işlenir; tüm işletmeler ya da en yakın 100 işletme garantisi yoktur. Bu taramada halal etiketi olmayan işletmeler görünmez; görünmemesi uygun seçenek bulunmadığı anlamına gelmez. Kuş uçuşu uzaklık yaklaşık merkezden hesaplanır, yol mesafesi değildir. Kutup enlemleri ±85 dışında desteklenmez.

Haritada işaretler, metin listesinde ad/adres/mutfak/OSM kaynağı/varsa check_date etiketi gösterilir. Etiketler topluluk beyanıdır: uygulama bu işletmelerin sertifikasını, et kaynağını veya güncel açık olma durumunu doğrulamaz. Kaynakta tarih bulunması uygulamanın denetimi değildir. Yol tarifi Google Maps URL bağlantısıdır; API anahtarı veya ücretli yönlendirme API'si kullanılmaz. Yalnız hedef işletme koordinatı eklenir, kullanıcının başlangıç konumu eklenmez. Uygunluk değerlendirmesi yerine soru kartlarına bağlantı verilir.

Seyahat dil seçimi 10 sabit soru kartı dili arasında yapılır; kart ve /translate?target= bağlantılarına yansır. Çeviri ekranı kurulu model desteğini ayrıca doğrular; örneğin Japonca soru kartı olması Japonca çeviri modelinin kurulduğu anlamına gelmez. Seyahat ekranı henüz Türkçedir; arayüzün tamamı çok dillileştirilmiş sayılmaz.

## Ücretsiz servis ve kullanım sınırları

GeoNames dosyası yereldir; hesap/ücretli şehir arama API'si, Nominatim veya otomatik tamamlamalı halka açık geocoder kullanılmaz. GeoNames CC BY 4.0 kaynak/değişiklik bilgisi Data/TRAVEL_CITIES_SOURCE.md dosyasındadır. OSM veri atfı ve ODbL bağlantısı ekranda görünür. Leaflet 1.9.4 dosyaları lisansıyla yerel paketlenmiştir; resmi SHA256 SRI değerleri doğrulandı. Harita için gereken küçük JavaScript köprü kodu dışında uygulama/backend C#'tır.

Geliştirme ayarı: Maps:Enabled=true ve Maps:OverpassEndpoint=https://maps.mail.ru/osm/tools/overpass/api/interpreter . OSM küresel sunucu listesinde VK Maps kaynağı proje kullanımına açık ücretsiz servis olarak listelenir; hesap/anahtar eklenmedi. Private.coffee ve ana Overpass kaynaklarında hata/zaman aşımı görüldüğü için geliştirme ayarı çalışan VK Maps kaynağına geçirildi; garanti/SLA yoktur. Sağlayıcı kendiliğinden değiştirilmez, otomatik tekrar denemesi yapılmaz. Endpoint yalnız sunucu yapılandırmasından değişir, kullanıcıdan URL/sorgu kabul edilmez. Üretimde Maps:Enabled varsayılan false; yayımlarken sağlayıcı koşulları ve kapasite yeniden değerlendirilmeli. Ücretli servis açılmadı.

Uygulama dış restoran sorgularını tekilleştirir: bir seferde bir istek, en az 10 saniye ara, RAM içinde UTC günde en fazla 50 istek; 6 MiB üstüne gelince yeni indirme başlatılmaz, tek yanıt 4 MiB ile sınırlıdır (günlük üst sınır yaklaşık 10 MiB). 429/406/504 servis cevabından sonra 30 saniye bekleme uygulanır. Sayaç yeniden başlatmada sıfırlanır, birden çok sunucuda paylaşılmaz; yoğun/yayın ortamına yönelik dağıtık kota değildir. Web ve API POST hız sınırları da vardır. Aynı yaklaşık merkez 30 dakika RAM önbelleğinden döner. Konum kullanıcı veritabanına/kalıcı geçmişe yazılmaz; POST gövdesindedir, arama URL'sinde değildir. Dönen koordinatlar yaklaşık merkez ve halka açık işletme koordinatlarıdır.

Harita görselleri https://tile.openstreetmap.org/{z}/{x}/{y}.png üzerinden tarayıcının normal HTTP önbelleğiyle istenir; görünür atıf ve normal Referer korunur. Mobil WebView User-Agent'e uygulama kimliği eklenir. Toplu/offline tile indirme, arka plan tarama veya pan/zoom sonrası otomatik restoran sorgusu yoktur. Maps:TileUrl yönetici ayarıyla değiştirilebilir. Harita görsellerinin sağlayıcısı değiştirilirse atıf ve politika da gözden geçirilmelidir.

Restoran sorgusu başarısızsa hata restoran yokluğu/uygunluk olarak yorumlanmaz. Onaylı ve geçerli bölge için harita gösterilebilir; olmayan restoran sonuçları uydurulmaz. Görsel harita da yüklenmezse metin/kaynak listesi kullanılabilir. Konum/servis gizlilik açıklaması /privacy ekranına eklendi; geliştirme sağlayıcısı: https://maps.mail.ru/osm/tools/overpass/ .

## Doğrulama ve açık işler

C# TravelChecks geçti: dünya şehirleri (İstanbul, Roma, Tokyo ve 東京), kısa/uzun girdi, onaysız dış çağrı engeli, NaN/sınır koordinat, hassasiyet azaltma, sabit sorgu, halal/mesafe filtreleri, tekrar/sıralama, kaynak URL'si, cache, hız sınırı, upstream 429 ve eksik yanıt. Node harita köprüsü 5/5 geçti: otomatik konum/tile çağrısı yok, açık eylemle yaklaşık konum, izin reddi/güvensiz HTTP alternatifi, OSM adını HTML çalıştırmadan popup metni yapma, seyahat dil bağlantıları.

Web derlendi ve normal test portunda açıldı (bu kez önceki Windows Uygulama Denetimi engeli tekrarlanmadı; koruma değiştirilmedi). Gerçek tarayıcıda şehir araması/seçim, onaysız arama reddi, Japonca dil bağlantıları ve OSM harita görseli doğrulandı. Gerçek kullanıcının GPS izni/konumu istenmedi.

Önceki sürümde ana Overpass ve Private.coffee bağlantılarında 502/504 görüldü. Düzeltmede DNS/TLS/istek aşamaları ayrı ölçüldü; bağlantı ve TLS hızlı, bekleme sunucu yanıtındaydı. VK Maps üzerinde ilk canlı C# kontrolü HTTP 200/5 kayıt verdi. Şehir merkezindeki sorgu için helal regex filtresi iki doğrudan `diet:halal=yes` / `only` eşleşmesine ayrıldı; sınırlı amenity/3 km alanı korundu. Sunucu sorgu bellek bütçesi 16 MiB oldu; indirilen yanıt sınırı hâlâ 4 MiB. İstek zaman aşımı 45 saniye (sorgu işlem süresi 25 saniye) olarak düzenlendi.

Düzeltme sonrası C# canlı testinde seçilen İstanbul merkezi (41.01, 28.95) HTTP 200 ve 4 OSM kaydı verdi. Gerçek tarayıcıda GeoNames İstanbul seçimi → açık onay → arama → 4 kaynak kartı ve harita işaretleri uçtan uca doğrulandı. Bunlar dört OSM elemanıdır; aynı işletme hem node hem way kaydıyla bulunabilir, dört benzersiz şube garantisi verilmez. Kartlar kaynak eleman kimliğini korur. Sertifika doğrulaması değildir. Ücretsiz kaynakların gelecekte her isteğe yanıt vereceği garanti edilmez; hata gösterimi ve önbellek korunur.

Kontrollü TravelChecks düzeltmeden sonra tekrar geçti. Yeni test derlemesinin bir çalıştırması Windows Uygulama Denetimi tarafından 0x800711C7 ile engellendi; koruma değiştirilmedi. Son normal test ve gerçek web çalıştırması başarılı oldu. `--trace` yalnız testte bağlantı aşamalarının adını/zamanını yazdırır, kullanıcı verisi veya kimlik bilgisi yazdırmaz.

Canlı testi özellikle çalıştırmak için (dış servise bir istek gönderir): `dotnet run --project tests/TravelChecks -- --live --city-center --endpoint=https://maps.mail.ru/osm/tools/overpass/api/interpreter`. Normal testler canlı servisi çağırmaz.

Android TestBuild 0 hata ve önceden bilinen 4 XA4301 OCR kitaplık uyarısıyla derlendi. Androidde gerçek WebView/GPS izin/geri dönüş akışı henüz cihazda denenmedi. iOS plist XML doğrulandı; iOS/Mac/Windows cihaz çalıştırması doğrulanmadı. Canlı web restoran araması düzeltildi; Trello 27 için gerçek mobil GPS/WebView testi hâlâ bekler.

## Kaynaklar

- https://download.geonames.org/export/dump/readme.txt
- https://www.openstreetmap.org/copyright
- https://wiki.openstreetmap.org/wiki/Key:diet:halal
- https://wiki.openstreetmap.org/wiki/Overpass_API#Public_Overpass_API_instances
- https://overpass.private.coffee/
- https://operations.osmfoundation.org/policies/tiles/
- https://leafletjs.com/download.html
- https://developers.google.com/maps/documentation/urls/get-started
- https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/geolocation?view=net-maui-10.0
