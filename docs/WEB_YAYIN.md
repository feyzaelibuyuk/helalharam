# Web sürümünü yayıma hazırlama

Bu görev paket hazırlar; hosting/alan adı seçilmediği için internet yayını yapılmadı.

## Paket

Depo kökünde:

```text
dotnet publish BlazorApp1/BlazorApp1.csproj -c Release -o artifacts/web
```

Sunucu .NET 10 ASP.NET Core runtime desteklemeli; kalıcı disk, HTTPS ve Blazor Server için WebSocket bağlantısı gereklidir. Geçici dosya sistemi olan sunucuda SQLite verileri kaybolabilir. İlk sürüm tek uygulama örneği içindir.

## Sunucu yapılandırması

Ortam değişkenleri (parola özetini repoya yazmayın):

- ASPNETCORE_ENVIRONMENT=Production
- Storage__DataDirectory: yayın klasörünün dışında, yalnız uygulama hesabının eriştiği kalıcı mutlak klasör.
- AllowedHosts: gerçek alan adı; birden fazla ise noktalı virgülle ayrılır.
- Admin__PasswordHash: AdminSetup ile oluşturulan mevcut özet, hosting secret ayarıyla sağlanır. Düz parola değildir. Development User Secrets sunucuda otomatik yüklenmez. Ayarlanmazsa yönetici girişi kapalı kalır.

Kestrel HTTPS uç noktası veya sağlayıcının güvenilir proxy/HTTPS yapılandırması gerekir. Proxy adresi bilinmeden tüm proxy'lere güven açılmamalıdır. TLS sonlandırması ve gerçek istemci IP'si sağlayıcıya göre ayrıca ayarlanıp test edilmelidir; IP tabanlı sınırlandırma buna bağlıdır.

Kalıcı klasörde helalharam.db ve keys/ oluşur. Anahtarlar oturum/favori çerezlerini korur; anahtar klasörü gizli tutulmalı, disk şifrelemesi ve erişim izinleri sunucuda sağlanmalıdır. Paket içine DB, anahtar veya parola koyulmaz.

## Mevcut veriyi taşıma ve güncelleme

İlk geçişte uygulamayı durdurun; SQLite yedeğini doğrulayın. Aktif DB dosyasını tek başına kopyalamayın: WAL dosyasında kaydedilmemiş sayfalar bulunabilir. Durdurulmuş uygulamanın tutarlı yedeğini kalıcı klasöre taşıyın. Anahtarlar ve veritabanını birlikte yedekleyin. Mevcut eski çerezler yeni anahtar deposuyla geçersizleşebilir; kullanıcı yeniden giriş/favori ekleme ihtiyacı duyabilir.

Her güncellemeden önce durdurma, DB+anahtar yedeği, yeni paket ve kontrol yapılır. Kalıcı klasör silinmez. Başlangıçta şema eklemeleri yapılabildiği için geri dönüşte eski paketle birlikte uyumlu DB yedeğini de geri yükleyin. Üretim verileriyle deneme yapılmaz.

## Yayına çıkmadan önce

Gizlilik sayfasındaki işletmeci, iletişim, saklama/silme politikası tamamlanmalı. Gerçek HTTPS alan adında giriş/çıkış, anonim yönetici reddi, öneri gönderme/inceleme, favori, OFF hatası, 404 ve yeniden başlatma sonrası veri korunması test edilmeli. Sertifika kayıtları demo/onay sınırlarını korumalı. Bu kontroller tamamlanmadan Trello 21 tamamen bitti sayılmaz.

Mobil uygulama hâlen emülatörün 10.0.2.2:5088 yerel API adresini kullanır; internete yayın bu adresi kendiliğinden değiştirmez. Gerçek cihaz dağıtımı için HTTPS API adresi ayrıca yapılandırılacaktır.

## Bu görevde yapılan doğrulama

Release publish ve Debug web derlemesi başarılı. İzole Production başlangıcı, zorunlu kalıcı klasör, HTTPS 307 yönlendirmesi, dış klasörde DB oluşması, yeniden başlatmada aynı anahtarların korunması doğrulandı. Pakette sertifika JSON'u var; DB ve key XML dosyası yok.

BarcodeChecks, MenuChecks, IngredientChecks, CertificateChecks, FavoriteChecks, OffChecks, CatalogChecks, DishChecks, CorrectionChecks ve AdminHttpChecks geçti. ReviewChecks DLL çalıştırması Windows Uygulama Denetimi tarafından 0x800711C7 ile engellendi; kontrol devre dışı bırakılmadı ve bu test başarılı sayılmadı. Yönetim inceleme HTTP akışı bağımsız olarak geçti.

Git çakışma işareti bulunmadı. Hosting/alan adı seçilmediği için gerçek internet yayını, sağlayıcı proxy/HTTPS ve üretim gizlilik içeriği tamamlanmayı bekliyor.
