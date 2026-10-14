# Genel kontrol düzeltmeleri

- Menü soruları: Türkçe metin tr-TR kültürüyle küçük harfe çevrilir; JELATİN, PİLAV ve TATLI anahtar kelimeleri eşleşir. Yalnız Türkçe dil seçiminde uygulanır; sözcük sınırları korunur. Yeni hata tekrar testleri tests/MenuChecks içindedir.
- OFF ürün bulunamadı mesajı: artık mobil etiket OCR akışına yönlendirir.
- Web SQLite: SQLitePCLRaw.bundle_e_sqlite3 3.0.5 ve SourceGear.sqlite3 3.53.3 açık paket başvuruları eklenmiştir. Eski SQLitePCLRaw.lib.e_sqlite3 2.1.11 bağımlılığı kaldırılmıştır; çözümlenen SQLite paketi/motoru 3.53.4'tür. EF Core 10 veri erişimi korunur.

Web derlemesi uyarısız tamamlandı. NuGet geçişli güvenlik taramasında bilinen açık bildirilmedi. Katalog, OFF, içerik, sertifika ve menü testleri geçti. Katalog testi çalışan SQLite motorunun en az 3.50.2 olduğunu da kontrol eder. Mevcut veritabanı salt okunur açılarak quick_check=ok sonucu, 5 ürün ve 2 restoran kaydı doğrulandı; veri dosyası silinmedi veya yeniden oluşturulmadı.

Android'deki aynı ML Kit yerel kütüphanesinin tekrar paketlenmesi XA4301 uyarısı bu görevden ayrıdır; gizlenmedi. Gerçek kamera ve fotoğraf izin akışları cihaz testi beklemektedir.

Paket geçişi: https://github.com/ericsink/SQLitePCL.raw/blob/main/v3.md
Güvenlik kaydı: https://github.com/advisories/GHSA-2m69-gcr7-jv3q

Merge sonrasında eski çalışan Blazor sürecini durdurup yeniden başlatın; belleğe yüklenmiş eski SQLite motoru kendiliğinden güncellenmez.
