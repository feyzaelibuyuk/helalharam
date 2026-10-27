# 29 — Topluluk deneyimleri, oylar ve katkı geçmişi

## Kullanım

Web: üst menüde **Topluluk deneyimleri** (`/community`). **Deneyim paylaş** formunda işletme/şube, şehir, ülke, yemek/ürün, ziyaret tarihi, soru, alınan yanıt ve isteğe bağlı kaynak girilir. Açık paylaşım onayı gerekir. Gerçek kişi/iletişim/sağlık bilgisi gönderilmemelidir.

Paylaşım önce `pending` durumunda yalnız sahibinin **Paylaşımlarım** ekranında ve yönetici incelemesinde görünür. Yönetici `/admin → Topluluk paylaşımlarını incele` yoluyla yayınlar/gizler. Gerekçe ve sürümlü karar geçmişi yalnız yöneticidedir. Yayınlama paylaşım kurallarına yönelik incelemedir; kullanıcı iddiası, işletme uygunluğu, helal durumu veya sertifika doğrulanmış sayılmaz. Ürünler, restoran kataloğu ve sertifika kayıtları otomatik değiştirilmez.

Mobil: **Topluluk → Topluluk paylaşımları ve oylar**. Sunucudaki C# Blazor ekranı MAUI WebView içinde kullanılır; web/API bilgisayarda 5088 portunda açık olmalıdır. Cihazdaki eski deneyim taslağı ayrı kalır ve kendiliğinden gönderilmez. Fiziksel telefon sunucu adresi henüz yapılandırılmıyor.

## Oylar ve rozetler

Yayındaki paylaşım için **Yararlı** veya **Güncelliği şüpheli** seçilebilir; oy değiştirilebilir/kaldırılabilir. Aynı tarayıcı kimliği + paylaşım için veritabanında tek satır vardır. Aynı oyu tekrarlamak sayıyı artırmaz. Kendi paylaşımına veya yayında olmayan paylaşıma oy verilemez. Çekilmiş/gizlenmiş paylaşımın oyları genel katkı sayacına katılmaz. Altı aydan eski ziyaretler eski bilgi olarak işaretlenir.

- 0 yayındaki paylaşım: **Yeni katılımcı**.
- 1–2 yayındaki paylaşım: **İlk paylaşım**.
- 3 ve üzeri: **Düzenli katkı**.

Yayındaki katkı sayısı, bağlantı eklenen katkı sayısı ve alınan yararlı oyları gösterilir. Bağlantı olması kaynağın doğruluğunu göstermez. Bu rozet/sayaçlar güvenilirlik puanı, kimlik/uzmanlık belgesi veya dini uygunluk oylaması değildir. Doğrulanmış hesap/kimlik ve gelişmiş kötüye kullanım analizi sonraki aşamadır.

## Kimlik, gizlilik ve sınırlar

Bu sürümde hesap sistemi yoktur. GET `/community...` sunucudan üretilmiş, imzalı/şifrelenmiş `helal-community-v1` çerezi verir: HttpOnly, SameSite=Strict, HTTPS'te Secure, en fazla 365 gün. Veritabanı kimlik özeti tutar; tam kimlik veya çerez metni herkese gösterilmez. Katılımcı takma adı özetin kısa bölümünden üretilir. Sahip kimliği form alanından alınmaz. Çerez sahteciliği geçerli kimlik oluşturmaz.

Çerez silmek veya başka tarayıcı/cihaz kullanmak yeni kimlik oluşturabilir; bu yüzden kişi başına tek oy garantisi yoktur. Çerez silmek sunucu kaydını silmez ve eski paylaşımları yönetme erişimini kaybettirebilir. Hesaplar arası eşitleme/kurtarma yoktur. **Paylaşımlarım → Paylaşımımı geri çek** yayını kaldırır; içerik ve inceleme geçmişi korunur, yönetici sahibinin geri çektiği içeriği yeniden yayınlayamaz. Bu sürümde kalıcı silme başvuru ekranı ve otomatik saklama süresi yoktur.

Yeni paylaşım/oy/geri çekme ve yönetici kararları POST + CSRF korumalıdır. Topluluk POST'ları mevcut öneri/geri bildirim IP kotasıyla dakikada toplam 10 isteği paylaşır. Gövde 64 KiB; geliştirme sınırı toplam 500 paylaşım ve tarayıcı kimliği başına son 24 saatte 10 yeni paylaşımdır. Bunlar çok sunuculu üretim servisi için dağıtık kötüye kullanım koruması değildir. Herkesin ve sahibinin listesi 20 kayıt/sayfa, yönetici kuyruğu geliştirme sınırındaki en fazla 500 kayıttır.

## Kontrol durumu — 27.10.2026

Web derlemesi ve son Android artımlı derlemesi 0 hata/0 uyarı. Mevcut AdminHttpChecks regresyon testi geçti (yönetim, CSRF, öneriler, geri bildirim, dil ve katalog değişmezliği). Topluluk test projesinin kaynak derlemesi de 0 hata/0 uyarı; derlenmesi testlerin çalıştığı anlamına gelmez. `tests/CommunityChecks` kaynakları: gizlilik, onay, kimlik, tek oy, değiştirme/kaldırma, rozet, yetki, CSRF, idempotans, eski sürüm, geri çekme ve katalog ayrımı kontrollerini içerir. **Bu yeni çalıştırıcı Windows Uygulama Denetimi tarafından engellendi; testler çalışmış/geçmiş sayılmaz. Güvenlik ilkesi değiştirilmedi veya aşılmadı.** Normal izinli ortamda komut:

`dotnet run --project tests/CommunityChecks/CommunityChecks.csproj -- "<depo tam yolu>"`

Tarayıcıda ayrı test veritabanı ve geçici test yönetici hesabıyla şu akış görüldü: deneyim gönderme → sahibinin bekleyen kaydı → diğer katılımcıya görünmeme → yönetici yayınlama → genel listede görünme/İlk paylaşım rozeti → başka katılımcıdan oy → tekrar oy sayısını koruma → oyu değiştirme → oy kaldırma → sahibinin geri çekmesi → genel listeden kaldırılma → yönetici ekranında geri çekme geçmişi ve yayınlama formunun kalkması. Gerçek işletme veya uygunluk iddiası eklenmedi. Test verisi gerçek kullanıcı veritabanına yazılmadı.

Android/WebView üzerinde gerçek dokunma/çerez kalıcılığı denemesi ayrıca yapılmalıdır. Otomatik test çalıştırıcısının engeli ve hesap doğrulaması sınırı nedeniyle 29'un tüm üretim düzeyi güvenilirlik gereksinimleri tamamlanmış sayılmaz; bu sürüm temel topluluk/oy/katkı geçmişi akışıdır.
