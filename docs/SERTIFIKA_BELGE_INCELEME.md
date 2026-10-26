# 28 — Resmî kaynaklar ve özel belge inceleme

## Kullanım

Web: `/certificates` veya üst menüde **Sertifika kaynakları**. Mobil: **Profil → Sertifika kaynakları / belge inceleme**. Mobil form C# MAUI ile yazıldı; web/API bilgisayarda 5088 portunda açık olmalı (Android emülatörü `10.0.2.2`). Fiziksel telefon sunucu adresi henüz yapılandırılmıyor.

TSE, HAK, JAKIM, MUIS, BPJPH, DHCE ve IFANCA resmî sorgulama bağlantıları bulunur. Bu bir başlangıç dizinidir; tüm dünya ve kuruluşlar kapsanmaz. HAK akreditasyon kaynağıdır; ürün sertifikası düzenleyen kuruluş olarak seçilemez. Dış bağlantılar açılana kadar bu sitelere belge/metin gönderilmez; otomatik dış site taraması yapılmaz.

Ürün/işletme adı, belge düzenleyeni, numara, kapsam ve paylaşım onayı gerekir. PDF/JPG/PNG dosyası (en fazla 5 MiB) **veya** HTTPS kaynak bağlantısı eklenir. Dosya uzantısı ile başlangıç imzası eşleştirilir; tam belge özgünlüğü/virüs taraması yapılmaz. Kişisel bilgileri kaldırın ve yalnız paylaşma hakkınız olan belgeleri gönderin. Yerel motor, ücretli API veya yeni hesap gerekmez.

Yönetici: `/admin → Sertifika belgelerini incele`. Yüklenen bilgiler başlangıçta doğrulanmamıştır. Dosyalar yalnız yetkili yönetici tarafından indirilir; tarayıcıda otomatik gömülmez. Kaynak, numara, ürün/üretim yeri/şube, tarihler ve iptal kontrolü ile belgeye özgü resmî kayıt bağlantısı olmadan **İnceleme tamamlandı** kaydedilemez. Genel sorgulama sayfası veya logo tek başına kanıt değildir. Diğer kuruluşlar bu sürümde tamamlandı durumuna geçirilemez; ek bilgi/ret durumları kullanılabilir.

**İnceleme tamamlandı katalog/helal onayı değildir.** Kararlar ürünleri, Open Food Facts'i veya `Data/certificates.json` dosyasını otomatik değiştirmez. Gerçek belge örneği doğrulanmadan kataloğa eklenmedi; mevcut gerçek sertifika listesi boştur. Kaynak etiketindeki `halal` etiketi ayrı gösterilir. Katalogdaki doğrulanmış kayıt için ayrıca operatörün `ReviewedBy` ve `EvidenceNote` alanları, kapsam, tarih ve resmî kaynak bilgisi gerekir. Yalnız alan adı eşleşmesi belge doğruluğunu sağlamaz.

## Saklama ve koruma

- Özel dosyalar geliştirmede web projesinin `certificate-evidence` klasöründe; `Storage:DataDirectory` ayarlanmışsa onun altında saklanır. `wwwroot` altında dosya saklanması reddedilir. Git/publish çıktısına alınmaz.
- Veritabanında meta bilgiler, SHA-256 özeti ve değiştirilmeyen inceleme geçmişi tutulur. İndirmede özet/boyut yeniden kontrol edilir. Dosya adı sunucuda rastgele GUID'dir; kullanıcı yolu kullanılmaz.
- İndirme yetki kontrolü, `attachment`, `nosniff`, `no-store`; gönderme ve karar formlarında CSRF kontrolü vardır. Mobil form aynı çerez+token düzenini kullanır.
- İstek gövdesi 6 MiB, dosya 5 MiB; geliştirme kotası toplam 100 gönderim ve 100 MiB. Tek uygulama sürecinde aynı anda bir yükleme işlenir; IP başına dakikada 5 gönderim. Birden çok sunucu örneği için dağıtık kota/lock henüz yoktur.
- Aynı gönderim/karar kimliği aynı içerikle yinelenirse ikinci kayıt oluşmaz. Farklı içerik veya eski inceleme sürümü 409 ile reddedilir.
- Otomatik silme süresi, kullanıcı silme ekranı, zararlı dosya taraması ve otomatik kuruluş doğrulama API'si bu sürümde yoktur. Yedekleme veritabanıyla birlikte özel dosya klasörünü de kapsamalıdır. Yayın öncesi saklama politikası belirlenmeli.

## Doğrulama (26.10.2026)

- `dotnet run --project tests/CertificateChecks/CertificateChecks.csproj`
- `dotnet run --project tests/EvidenceChecks/EvidenceChecks.csproj -- "<depo tam yolu>"`

EvidenceChecks ayrı SQLite/test klasörü ve rastgele test parolası kullanır; gerçek yönetici parolasını okumaz/değiştirmez. Dosya/uzantı/boyut, izin, CSRF, özel indirme başlıkları, XSS kaçışlama, özet bütünlüğü, tekrar gönderim, kontrol listesi, eski sürüm ve katalog ayrımı kontrol edildi. Sentetik test belgesi gerçek ürün/sertifika değildir ve uygulama kataloğuna eklenmez.

Web formu tarayıcıda kaynak bağlantısıyla gönderildi ve başarı mesajı görüldü. Android paketi derlendi: 0 hata, önceki OCR kitaplıklarından 4 XA4301 uyarısı. Web derlemesi 0 hata/0 uyarı; mevcut AdminHttpChecks testi de geçti. cihaz üzerinde dosya seçme/gönderme ve gerçek belgeyle insan incelemesi ayrıca denenmelidir. 28'in bu temel akışı uygulanmıştır; tüm kurumlarla otomatik sertifika doğrulaması yapılmış sayılmaz.
