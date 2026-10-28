# Public paylaşım hazırlığı — 28 Ekim 2026

## Tamamlanan yerel inceleme

Hazırlıktan önceki 49 commit ve 599 benzersiz dosya sürümü yerelde mevcut bütün Git referansları üzerinden tarandı. Gerçek sağlayıcı anahtarı, özel anahtar veya kayıtlı yönetici parolası bulunmadı. Kütüphane DATA_API_KEY sabiti ve test URL'lerindeki örnek kimlik bilgileri gerçek sır değildir. Bu bir örüntü/dosya incelemesidir; bütün sır biçimlerini yakalama garantisi değildir.

Geçmişte izlenen veritabanı, sertifika yüklemesi, imzalama anahtarı veya model ağırlığı bulunmadı. Yerel veritabanları, artifacts, model/ortam dosyaları ve doğrulama çıktıları Git dışında tutulur. GeoNames şehir JSON'u yaklaşık 8,7 MB'tır; kullanıcı konum geçmişi değildir ve kaynak/lisans bildirimi mevcuttur.

README güncellendi. Open Sans / Bootstrap / Popper lisans metinleri ve üçüncü taraf bildirimleri eklendi. Projeye özgü kod için açık kaynak lisansı seçilmedi; README hakları saklı tuttuğunu belirtir. Public görünürlük ve açık kaynak kullanım lisansı ayrı kararlardır.

## Commit e-postası

Bu depodaki yeni CLI/GitHub Desktop commitleri için yerel Git e-postası hesabın ID tabanlı GitHub noreply adresine ayarlandı. Bu bir parola veya hesap ayarı değişikliği değildir; yalnız bu depoyu etkiler.

**Önceki commitlerdeki okul e-postası hâlâ bulunur.** Yeni e-posta ayarı geçmişi değiştirmez. Tamamen kaldırmak tüm ilgili branch/tag geçmişini yeniden yazmayı, uzak referansları güncellemeyi ve mevcut klonları yeniden eşitlemeyi gerektirir. Bu hazırlıkta geçmiş yeniden yazılmadı ve force-push yapılmadı. Okul adresi öğrenci numarasını açığa çıkarabileceğinden public yapmadan önce bu kararı verin.

GitHub webinde oluşturulan commitler ayrı hesap ayarına bağlıdır: Settings → Emails → Keep my email addresses private. Bu ayar kullanıcı tarafından kontrol edilmelidir; hesabın güvenlik/gizlilik ayarları otomatik değiştirilmedi.

## Public yapmadan önce kalanlar

1. Bu branch'i gözden geçirip main'e birleştirin; gerekli commitleri GitHub'a gönderin.
2. Okul e-postasının eski commitlerde görünmesini kabul edip etmediğinize karar verin. Gizlenecekse görünürlüğü değiştirmeden önce geçmiş temizliği ayrı yapılmalıdır.
3. GitHub'daki bütün branch/tag'leri, Releases eklerini, Actions günlük/artifact'lerini, Issue/PR eklerini ve wiki'yi kontrol edin. Bunlar yerel dosya taramasıyla doğrulanmadı. GitHub tarafında bu alanlara erişen bir oturum bu incelemede kullanılmadı.
4. Görünürlüğü GitHub'da kullanıcı değiştirsin. Public olduktan sonra başkaları klonlayabilir/forklayabilir; yeniden private yapmak bu kopyaları geri toplamaz.

Kaynaklar: [GitHub görünürlük](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/managing-repository-settings/setting-repository-visibility), [commit e-postası](https://docs.github.com/en/account-and-profile/how-tos/email-preferences/setting-your-commit-email-address), [lisans verilmemesi](https://choosealicense.com/no-permission/).
