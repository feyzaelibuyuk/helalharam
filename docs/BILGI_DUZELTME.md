# Bilgi düzeltme önerileri

Ürün, gerçek OFF ürünü ve yemek detayındaki Bilgi düzeltme öner bağlantısı formu açar. Kullanıcı yanlış/eksik alanı seçer, 10–2.000 karakter öneri ve isteğe bağlı en fazla 1.000 karakterlik https kaynak bağlantısı girer. Ad, telefon veya e-posta istenmez; kişisel bilgi yazılmaması belirtilir. Kaynak URL'si sunucu tarafından açılmaz ve otomatik doğrulama sayılmaz.

- Web: /corrections/catalog/{id}, /corrections/off/{barcode}, /corrections/restaurant/{id}; C# statik SSR EditForm ve antiforgery koruması. Hatalı girişte metin sayfada korunur. Başarıda gönderim numarası ve inceleme bekliyor durumu görünür.
- Mobil: aynı üç detay ekranından form açılır; JSON POST /api/corrections. Sunucuya ulaşılamazsa açık formdaki metin korunur. Belirsiz ağ sonucundan sonra aynı kimlik/içerikle tekrar gönderilir; alanlar sonuç netleşene kadar kilitli kalır. Formdan çıkıldığında veya uygulama kapandığında taslak kalıcılığı yoktur.
- Sunucu: yalnız geçerli ve alınabilen ürün/yemek hedefi kabul edilir. Başlık istemciden alınmaz; katalogdan alınır. Kayıtlar ayrı CorrectionSubmissions tablosuna pending durumunda yazılır; ürün veya restoran bilgisi değiştirilmez. Aynı gönderim kimliği ve içerik ikinci kayıt oluşturmaz; aynı kimlikle farklı içerik 409 verir. İstemciden gelen onay/durum değeri kabul edilmez.
- Kayıt: kimlik, hedef türü/kimliği, hedef adı, alan, öneri, kaynak, UTC gönderim tarihi ve pending durumu. Genel öneri listesi veya başka kullanıcıların önerilerine erişim API'si yoktur. İnceleme/onay yönetimi ve otomatik bildirim bu göreve dahil değildir.
- Gönderim sınırı: IP başına dakikada 10 web/API öneri POST'u, ortak sayaç. Sınırda 429 ve Retry-After: 60; normal GET sayfaları bu sınıra dahil değildir. İstek gövdesi en fazla 64 KB. Mobil JSON uç noktası anonim öneri alır; hiçbir istek doğrudan yayımlanmaz. Bu akış oturum/cihaz eşitlemesi içermez.

Mevcut veritabanına yalnız yeni tablo eklenir. Eski ürün/yemek verileri silinmez veya yeniden oluşturulmaz. Otomatik CorrectionChecks; üç hedef türü, eski veri korunması, bekleyen kayıt, tekrar gönderim ve kimlik çakışması, bulunmayan kayıt, alan/metin/URL sınırları ile null isteğe bağlı kaynağı kontrol eder. Mevcut CatalogChecks ve DishChecks de geçti.

Uçtan uca web testleri gerçek kullanıcı veritabanından ayrı .verification/correction-test-root içinde yapıldı: normal POST, aynı isteği tekrar gönderme, hatalı metnin korunması, CSRF reddi, JSON API pending yanıtı, web/API 429 ve GET erişimi doğrulandı. Bu deneme kayıtları gerçek katalog veritabanına gönderilmedi. Form tarayıcıda açıldı; tarayıcı hata kaydı görülmedi. Android derlemesi ayrıca kontrol edilir; mobil formun ekranda tıklanarak gönderilmesi cihaz testi bekler. Mevcut ML Kit XA4301 paketleme uyarısı sürer.

## Elle kontrol

1. Blazor sunucusunu çalıştırın. Bir ürün/yemek detayından Bilgi düzeltme öner seçin.
2. Alan seçmeden veya kısa metinle gönderin; açıklayıcı hata ve mevcut metin beklenir.
3. Geçerli öneri gönderin; takip numarası ve inceleme bekliyor durumu görünmeli. Katalog metni değişmemeli.
4. Mobilde aynı formu açın. Blazor kapalıyken gönderin; metin korunmalı, sunucu açılınca aynı öneri tekrar gönderilebilmeli.

Bu branch feat/turkiye-yemek-detay üzerinden açılmıştır; henüz birleştirilmemiş yemek detayları değişikliklerini de içerir. Merge işlemi bu geçmişi de main'e taşır.
