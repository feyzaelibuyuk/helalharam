# İlk kullanıcı geri bildirimi

Web: /feedback. Mobil: Profil → Uygulama hakkında geri bildirim. Yönetici: /admin/feedback (korumalı). Tür, 10–2.000 karakter açıklama, platform ve sürüm alınır; isim/iletişim/fotoğraf istenmez. Kimlik doğrulanmadığından platform/sürüm bir kullanıcı beyanıdır. Gönderiler herkese açık değildir. Durum değiştirme/yanıt/bildirim özelliği bu görevde yoktur.

POST /api/feedback: 201 kayıt, 200 aynı kimlikle aynı içerik, 409 kimlik çakışması, 400 doğrulama, 429 hız sınırı. Web EditForm antiforgery kullanır; web ve API diğer anonim gönderilerle birlikte IP başına 10 POST/dakika sınırına tabidir. POST gövdesi 64 KB. Yeni tablo mevcut veriyi silmeden eklenir. Anonim kullanıcılar kayıt listesini okuyamaz.

Bağlantı sonucu belirsizse mobil aynı kimlik/içerikle tekrar dener. Yeni mesaj için formu yeniden açın. Form kapatılırsa gönderilmemiş metin cihazda kalıcı saklanmaz. Gizlilik ekranında saklama bilgisi var; otomatik silme henüz yok.

## Gerçek kullanıcı denemesi

Önce sunum test paketini kurun. 3–5 kişiden ürün bulma, etiket metnini kontrol etme, soru kartı ve favoriye ekleme görevlerini denemelerini isteyin. Formda ne beklediklerini, ne olduğunu, tekrar adımlarını ve iyileştirme önerisini yazsınlar. Kişisel/sağlık bilgisi istemeyin. Yönetici listesinde gerçek bulguları görevleştirin; örnek/test verilerini TEST olarak belirtin.

Bu geliştirme, toplama aracını hazırlar. Henüz gerçek kullanıcı denemesi yapılmış sayılmaz; Trello 23 ancak kullanıcılar denedikten ve sonuçlar incelendikten sonra tamamlanır.

## Doğrulama

FeedbackChecks: eski veriyi koruma, tekrar şema kurulumu, giriş sınırları, tekrar gönderim/çakışma, yetkisiz erişim ve sayfalama geçti. AdminHttpChecks: API/web gönderimi, CSRF, HTML kaçışlama, özel liste, tekrar gönderim ve web/API 429 kontrolleri geçti; mevcut yönetici/düzeltme kontrolleri de geçti. Testler izole veritabanındadır.

Web derlemesi 0 hata/0 uyarı. Android TestBuild 0 hata, mevcut 4 XA4301 OCR uyarısı. Test APK'sı güncellendi. Mobil geri bildirim formuna dokunarak gönderme henüz elle denenmedi.
