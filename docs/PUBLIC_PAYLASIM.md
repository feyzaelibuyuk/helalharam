# Public paylaşım hazırlığı — 28 Ekim 2026

## Tamamlanan yerel inceleme

Hazırlıktan önceki 49 commit ve 599 benzersiz dosya sürümü yerelde mevcut bütün Git referansları üzerinden tarandı. Gerçek sağlayıcı anahtarı, özel anahtar veya kayıtlı yönetici parolası bulunmadı. Kütüphane DATA_API_KEY sabiti ve test URL'lerindeki örnek kimlik bilgileri gerçek sır değildir. Bu bir örüntü/dosya incelemesidir; bütün sır biçimlerini yakalama garantisi değildir.

Geçmişte izlenen veritabanı, sertifika yüklemesi, imzalama anahtarı veya model ağırlığı bulunmadı. Yerel veritabanları, artifacts, model/ortam dosyaları ve doğrulama çıktıları Git dışında tutulur. GeoNames şehir JSON'u yaklaşık 8,7 MB'tır; kullanıcı konum geçmişi değildir ve kaynak/lisans bildirimi mevcuttur.

README güncellendi. Open Sans / Bootstrap / Popper lisans metinleri ve üçüncü taraf bildirimleri eklendi. Projeye özgü kod için açık kaynak lisansı seçilmedi; README hakları saklı tuttuğunu belirtir. Public görünürlük ve açık kaynak kullanım lisansı ayrı kararlardır.

## Commit e-postası

Bu depodaki yeni CLI/GitHub Desktop commitleri için yerel Git e-postası hesabın ID tabanlı GitHub noreply adresine ayarlandı. Bu bir parola veya hesap ayarı değişikliği değildir; yalnız bu depoyu etkiler.

Kullanıcının onayıyla yereldeki tüm referansların eriştiği 50 commitin geçmişi yeniden yazıldı. Okul adresi hem author hem committer alanlarında gizli GitHub adresiyle değiştirildi; dosya ağaçları, mesajlar, tarihler ve ebeveyn bağlantıları korundu. Her commit için dosya ağacı ve mesaj eşitliği, bütün erişilebilir commitlerde eski adresin bulunmaması doğrulandı. İmzalı commit/tag yoktu; tag veya stash bulunmadı. Yerel geçmişteki eski commit kimlikleri değişti.

Git dışında, yalnız yerelde tam Git bundle yedeği ve commit eşleme planı saklandı. Yedek eski kişisel metaveriyi içerir; GitHub’a yüklenmemelidir. Yerel reflog/eski Git nesneleri de özel çalışma kopyasında kalabilir; normal branch gönderimi bunları yayımlamaz.

GitHub’daki 40 branch için atomik ve her branch’in eski uzak kimliğine bağlı force-with-lease gönderim planı hazırlandı. Gönderim tamamlanıp uzaktaki referanslar doğrulanana kadar uzak geçmiş temiz kabul edilmez. Eski klonlardan/branch’lerden pull veya merge eski geçmişi tekrar getirebilir; diğer klonları yeniden klonlayın veya kontrollü eşitleyin. GitHub’ın sakladığı PR referansları/önbellekler yerel referans taramasıyla doğrulanmaz.

GitHub webinde oluşturulan commitler ayrı hesap ayarına bağlıdır: Settings → Emails → Keep my email addresses private. Bu ayar kullanıcı tarafından kontrol edilmelidir; hesabın güvenlik/gizlilik ayarları otomatik değiştirilmedi.

## Public yapmadan önce kalanlar

1. Bu branch'i gözden geçirip main'e birleştirin; gerekli commitleri GitHub'a gönderin.
2. Hazırlanan geçmiş güncellemesini GitHub’a gönderip bütün uzak branch’lerin yeni kimliklerini doğrulayın. Bu tamamlanmadan Fetch/Pull veya public görünürlüğe geçiş yapmayın.
3. GitHub'daki bütün branch/tag'leri, Releases eklerini, Actions günlük/artifact'lerini, Issue/PR eklerini ve wiki'yi kontrol edin. Bunlar yerel dosya taramasıyla doğrulanmadı. GitHub tarafında bu alanlara erişen bir oturum bu incelemede kullanılmadı.
4. Görünürlüğü GitHub'da kullanıcı değiştirsin. Public olduktan sonra başkaları klonlayabilir/forklayabilir; yeniden private yapmak bu kopyaları geri toplamaz.

Kaynaklar: [GitHub görünürlük](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/managing-repository-settings/setting-repository-visibility), [commit e-postası](https://docs.github.com/en/account-and-profile/how-tos/email-preferences/setting-your-commit-email-address), [lisans verilmemesi](https://choosealicense.com/no-permission/).
