# Ürün favorileri

Web: ürün detaylarında Favorilere ekle / Favorilerden çıkar; ana sayfada Favorilerim bağlantısı; /favorites sayfasında liste, ürün bağlantısı, tek kayıt çıkarma ve tümünü silme.

Mobil: Katalog ürün detaylarında ve gerçek OFF ürün detayında favori düğmesi; Favoriler sekmesinde liste, ürün açma, tek kayıt çıkarma ve onaylı toplu silme. Yerel Ürünler sekmesinin eski kurgu örnekleri bu göreve dahil değildir.

## Saklama

- Web: tarayıcıya özel, ASP.NET Core Data Protection ile korunan HttpOnly çerez. HTTPS'te Secure, SameSite=Lax, bir yıl ömür. Gerekirse CookieManager çerezi parçalara ayırır. Tarayıcı çerezleri silinirse liste kaybolur. Sunucunun Data Protection anahtarları değişir/kaybolursa eski kayıt okunamaz; uygulama boş listeye güvenli döner. Favori listesi sunucu veritabanına yazılmaz.
- Mobil: cihazın Preferences deposunda JSON. Uygulama verisi silinirse liste kaybolur. Hesap, web/mobil veya cihazlar arası eşitleme yoktur.
- Kaynak + ürün kimliği anahtardır: yerel katalog için id, OFF için API'nin döndürdüğü kanonik barkod. Tek ürün tekrar kaydedilmez; kaynaklar birbirine karışmaz.
- En fazla 20 kayıt ve yeni kayıt eklerken 4.000 karakter JSON alan sınırı. Uzun/çok dilli adlarla alan daha erken dolabilir. Kısa ürün adı/marka ve bağlantı saklanır; içerik, değerlendirme veya sertifika sonucu saklanmaz.
- Ürün açıldığında mevcut API'den bilgi alınır; OFF servisinin normal önbelleği kullanılabilir. Favori listesi uygunluk onayı veya içerik değişikliği bildirimi değildir. Ürün artık bulunamazsa mevcut detay ekranı durumu gösterir; favoriden çıkarılabilir.

Web değişiklikleri standart POST formu ve antiforgery doğrulaması kullanır. Yalnız doğrulanmış kaynak/kimlik için uygulamanın kendi ürün servisi üzerinden ad ve marka alınır. Yönlendirmeler doğrulanmış yerel ürün yollarıyla sınırlıdır. Kullanıcıdan gelen başlık veya dış URL kaydedilmez.

## Kontrol

1. Webde /products/1 veya /off?barcode=3017620422003 açın, Favorilere ekle seçin.
2. Favorilerim sayfasına gidin; ürünü tekrar açın ve sayfayı yenileyin. Kayıt korunmalı, düğme Favorilerden çıkar göstermeli.
3. Başka tarayıcı oturumu kendi boş listesini görmeli. Bir kaydı çıkarın; tümünü silme ile listenin boşaldığını doğrulayın.
4. Mobilde Katalogdan ürün detayı açın, favoriye ekleyin; kapatıp Favoriler sekmesini açın. Uygulamayı yeniden başlatınca kayıt korunmalı. Ürünü açın, çıkarın ve toplu silmede Vazgeç/Sil seçeneklerini deneyin.

Otomatik testler tests/FavoriteChecks: JSON kalıcılığı, tekrar kayıt, kaynak ayrımı, güvenli bağlantı, bozuk veri, kayıt ve boyut sınırları. Web uçtan uca kontrolde yerel ürün/OFF ekleme, çıkarma, toplu silme, tarayıcı ayrılığı ve tokensiz POST reddi geçti. Web sayfaları tarayıcıda açıldı; hata kaydı görülmedi. Android derlemesi kontrol edildi; mobil favori düğmelerinin gerçek cihazdaki etkileşimi ayrıca kontrol edilmelidir. Mevcut ML Kit paketleme XA4301 uyarısı sürer.
