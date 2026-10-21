# Arayüz dilleri — ilk çeviri seti

Web üst bölümünden, mobil Profil ekranından Türkçe / English / العربية seçilir. Web seçimi 365 günlük HttpOnly SameSite=Lax helal-ui-language çerezinde, mobil seçim ui-language Preferences kaydında saklanır. Geçersiz/bilinmeyen değer Türkçeye döner. Arapça sağdan sola düzen kullanır. Dil değişimi ana ekranı/Profili yeniden oluşturur; kaydedilmemiş form metninin kaybolabileceği uygulamada belirtilir. Mobil aynı tercihle yeniden açılır.

Shared/UiCatalog.cs ortak metin kaynağıdır. Metin anahtarları yalnız arayüz noktalarında kullanılır; ürün adı/içerik, OCR metni, kullanıcı gönderisi, sertifika ve değerlendirme kaynağı otomatik çevrilmez. Arayüz dili OCR alfabe modelini veya içerik değerlendirme dilini değiştirmez.

Çevrilen ilk kapsam: ortak web gezinmesi/dil seçimi, web katalog ana ekranı, geri bildirim formu temel etiketleri; mobil sekmeler, katalog temel eylemleri, Profil kontrolleri/açıklamaları, OCR kontrolleri ve temel mesajları, geri bildirim formu temel kontrolleri. EN/AR kullanıcıya çevrilmemiş ekran ve verilerin özgün dilinde kalabileceği açıklanır. Ürün/yemek detayları, yönetim ekranları, gizlilik açıklamasının tamamı, bazı hata/sayı/bilgi mesajları ve eski demo ekranları henüz Türkçe olabilir. Bu branch tüm arayüzün çevirisini bitirmez. Trello 24 devam eder.

Web /language POST antiforgery ve izin verilen üç dil listesiyle korunur; haricî geri dönüş URL'si kabul edilmez. Kullanıcı dili global değişkende veya sunucu culture'ında tutulmaz; scoped WebUi her bağlantının kendi çerezinden okuyup saklar. Kodlar/kaynak değerler arayüz dilinden bağımsızdır.

Arapça çeviriler temel arayüz setidir; anadili Arapça olan kullanıcıyla terminoloji ve cihaz yerleşimi kontrolü gerekir. Bu seçim tam etiket/menü çevirisi veya Arapça OCR desteği sağlamaz; bunlar ayrı işlerdir.

## Doğrulama

139 ortak metin anahtarında üç dilin her biri dolu ve anahtarlar benzersiz. İlk AdminHttpChecks çalışmasında mevcut yönetim/geri bildirim akışları ile dil CSRF, kalıcı çerez, EN/LTR, AR/RTL, geçersiz dil, oturum ayrımı ve kaynak ürün değişmezliği geçti. Son HTML etiket düzeltmelerinden sonra test tekrarında Windows Uygulama Denetimi test DLL'sini engelledi; koruma değiştirilmedi, bu tekrar tamamlandı sayılmadı.

Son web çıktısı bağımsız izole HTTP kontrolünde TR/EN/AR, kalıcılık, CSRF, geçersiz çerezde Türkçe geri dönüş, başka oturuma sızmama, sabit yerel yönlendirme, geri bildirim formu çevirisi ve kaynak ürünün aynı kalmasını geçti. Web derlemesi 0 hata/0 uyarı. Arapça sayfa tarayıcıda açılıp RTL yerleşimi, üst bölüm ve logo yönü görsel kontrol edildi. İzole test verileri .verification/ui-visual altında; gerçek kullanıcı dil çerezi değiştirilmedi.

Mobil dil seçimi/restart/izin pencereleri ve RTL dokunma akışları kullanıcıyla elle doğrulanmalı. Tüm ekranlar çevrilmediği için Trello 24 henüz tamamlanmaz.

Son Android TestBuild 0 hata ve mevcut 4 XA4301 OCR uyarısıyla tamamlandı. Güncel APK emülatöre başarıyla kuruldu; hata ayıklayıcı olmadan açılışta işlem çalışıyor ve AndroidRuntime çökme kaydı yok. Dil seçme/uygulama yeniden açma dokunma testi henüz yapılmadı.
