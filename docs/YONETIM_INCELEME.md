# Gönderim inceleme ve yönetim — ilk sürüm

18 ve 19. görevlerin ilk kapsamı, mevcut düzeltme önerilerinin tek yönetici tarafından incelenmesidir. Panel /admin, giriş /admin/login, inceleme /admin/corrections/{guid}. Genel katalog düzenleme, OFF verisine yazma, sertifika doğrulama, kullanıcı hesapları yönetimi ve bildirim gönderimi dahil değildir.

## Yerel yönetici parolası

Proje klasöründe PowerShell'de çalıştırın:

```powershell
.\configure-admin.ps1
```

Araç parolayı konsolda görünmeden iki kez alır (12–256 karakter). Kullanıcı adı admin'dir. ASP.NET Core Identity PasswordHasher ile yalnız parola özeti, projenin User Secrets deposuna kaydedilir; düz parola yazdırılmaz, dosyaya veya komut satırı argümanlarına yazılmaz. Repo/appsettings içinde varsayılan parola yoktur. Araç mevcut yerel parola ayarını değiştirir; bunu yalnız kendi geliştirme hesabınız için çalıştırın. Geliştirme amaçlı User Secrets deposu genel amaçlı şifreli kasa değildir; burada saklanan değer parola özetidir.

Ardından Blazor uygulamasını Development ortamında yeniden başlatın ve http://localhost:5088/admin/login açın. User Secrets Development ortamında otomatik okunur. Parola ayarlı değilse giriş kapalıdır. Parolanızı sohbetten göndermeyin.

Yayınlama ayrı bir görevdir: üretimde Admin:PasswordHash değerini sunucunun gizli yapılandırmasında ayarlamak, HTTPS ve Data Protection anahtarlarını uygun yönetmek gerekir; yerel geliştirme ayarı tek başına yayımlama işlemi değildir.

## İnceleme akışı

- Oturum: korumalı HttpOnly/SameSite Strict çerez, en fazla 30 dakika, otomatik süre uzatma yok. Üretimde Secure gerekir; yerel Development HTTP ile çalışabilir. Parola özeti değiştirilince eski oturum bir sonraki istekte reddedilir. Çıkış POST/antiforgery ile yapılır. Giriş denemeleri IP başına dakikada 5 ile sınırlıdır.
- Panel ve servisler yönetici rolünü denetler. Girişsiz kullanıcı kuyruk/detay sayfasından girişe yönlenir; /api/admin/corrections 401/403 döndürür. Genel kullanıcılara öneri metinleri veya inceleme notları açılmaz. Etkileşimli yönlendirmede de AuthorizeRouteView kullanılır.
- Kuyruk: durum ve kayıt türü filtreleri, durum sayıları, 25 kayıtlık sayfalama, boş sonuç. Her öneri hedef kaydı ve kullanıcı metniyle açılır. Kaynak linki yalnız güvenli https ise kullanıcı tıklamasıyla açılır; otomatik indirme/gezinti yoktur.
- Durumlar: inceleme bekliyor, ek bilgi gerekiyor, öneri uygun bulundu (kataloğa uygulanmadı), öneri reddedildi. Uygun bulma ürünün helal veya sağlıklı olduğunu doğrulamaz. Katalogda ürün/yemek, sertifika, OFF kaydı veya favoriler değiştirilmez.
- İnceleme notu 5–2.000 karakterdir. Her kayıt yeni CorrectionReviews satırı oluşturur; önceki/yeni durum, yönetici, UTC zaman ve sürüm saklanır. Notu aynı durumda kaydetmek de geçmişe eklenir. Gerekçeli yeniden açma için durum bekliyor seçilebilir.
- İşlem kimliği tekrarı aynı geçmişi çoğaltmaz. Eski sürümle açılmış form kaydı ezmez; notu kopyalayıp formu yenileme mesajı verir. Durum ve geçmiş aynı veritabanı işlemiyle kaydedilir. Yeni tablo ve benzersiz sürüm indeksi eski gönderimler silinmeden eklenir.
- Kullanıcının aynı öneriyi tekrar göndermesi artık güncel inceleme durumunu gösterir; otomatik bildirim veya herkese açık takip API'si eklenmemiştir.

## Doğrulama

ReviewChecks: yetkisiz/rolsüz erişim, yapılandırılmamış parola, yanlış parola, parola değişiminden sonra eski yetki, eski kayıt/geçmiş, tekrar işlem, eski sürüm, izinli kararlar, filtre/sayfa sınırları ve katalog değişmezliği.

AdminHttpChecks: yalnız kendisine ait geçici veritabanı ve rastgele, yazdırılmayan test parolasıyla yerel alt süreç açar. Gerçek yönetici parolasına/User Secrets ayarına dokunmaz. Girişsiz sayfa/API engeli, giriş/çıkış, CSRF, çerez bayrakları, öneri HTML kaçışlaması, inceleme/tekrar/eski sürüm, karar sonrasında kullanıcının aynı gönderimi tekrarı, katalog değişmezliği ve giriş deneme sınırı geçti.

CorrectionChecks ve DishChecks geçti. Web derlemesi uyarısız, Android derlemesi mevcut dört ML Kit XA4301 tekrar paketleme uyarısıyla başarılı. CatalogChecks derlendi fakat bu çalıştırmada Windows Uygulama Denetimi DLL'yi engelledi (0x800711C7); politika değiştirilmedi ve testin bu tekrarı geçti sayılmadı. AdminHttpChecks içinde ürün API'si inceleme öncesi/sonrası karşılaştırması geçti. Mobilde yeni durum metinleri derlendi; fiziksel cihaz form etkileşimi ayrıca kontrol edilmelidir.

Çerçeve kaynakları: [Cookie authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0), [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0).
