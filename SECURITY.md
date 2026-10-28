# Güvenlik ve hassas veri

Bu proje geliştirme ve yerel sunum aşamasındadır; üretim hizmeti olarak güvenlik garantisi veya destek süresi verilmez.

API anahtarı, yönetici parolası/özeti, User Secrets, .env dosyası, özel anahtar, veritabanı, kullanıcı belgesi/fotoğrafı, kişisel günlük ve model ağırlıklarını depoya eklemeyin. Yönetici yapılandırması yerel User Secrets veya sunucunun gizli yapılandırmasında tutulur. UserSecretsId değeri parola değildir.

Gerçek kimlik bilgisi yanlışlıkla paylaşılırsa yalnız dosyayı silmek yeterli değildir: ilgili kimlik bilgisi iptal/değiştirilmelidir; geçmiş ve barındırılmış kopyalar ayrıca incelenmelidir.

Açık bir Issue'ya gerçek kullanıcı verisi, parola, belge veya saldırıda kullanılabilir hassas ayrıntı koymayın. GitHub'da özel güvenlik bildirimi etkinse Security → Report a vulnerability yolunu kullanın. Etkin değilse önce yalnız özel iletişim kanalı talep edin; hassas ayrıntıları public olarak paylaşmayın.

Yerel HTTP emülatör geliştirmesi içindir. Public kod deposu oluşturmak uygulamayı internete açmak anlamına gelmez. Canlı dağıtım için HTTPS, kimlik doğrulama, gizli yapılandırma, kalıcı veriler ve bağımlılık güvenliği ayrıca değerlendirilir.
