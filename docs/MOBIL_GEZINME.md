# Mobil gezinme — ilk arayüz düzenlemesi

Android ana gezinmesi 7 üst sekmeden 5 simgeli alt sekmeye indirildi: Katalog, Favoriler, Menü, Deneyimler/Topluluk ve Profil. Kaydırarak sekme değiştirme kapalı; form yazarken yanlışlıkla başka sekmeye geçilmez. Diğer platformlar kendi TabbedPage davranışını kullanır.

Ayrı Ürünler sekmesindeki başlangıç demo araması kaldırıldı. Gerçek OFF barkod akışı ve açıkça etiketlenmiş yerel demo kataloğu Katalog ekranında kalır. Bildirimler Profil'de bir karttan ve Topluluk ekranından açılır.

Profil: beslenme tercihleri, keşif araçları, bildirimler, dil, özel not ve uygulama bilgileri kartlarda gruplanır. Dil değişikliği sonrası yine Profil'e dönülür. Kayıtlı alerjen tercihleri, favoriler ve eski özel notlar korunur.

Toplulukta gerçek gönderim/oylama girişi öne çıkarıldı. Eski yerel deneyim formu açılıp kapanan özel taslak bölümündedir; mevcut communityDraft silinmez ve sunucuya otomatik gönderilmez.

Bu çalışma tüm mobil ekranların son tasarımı değildir. TR/EN/AR arayüz çevirileri hâlâ kısmi; yeni grup açıklamaları Türkçedir. Küçük ekran, büyük yazı, Arapça yön ve tüm modal dönüş yolları ayrıca denenmelidir. Çeviri doğruluğu veya veri kaynaklarının kapsamı bu UI değişikliğiyle artmaz.

28 Ekim 2026: Tam Android TestBuild paketlemesi 0 hata ve bilinen 4 XA4301 uyarısıyla geçti. Emülatöre adb install -r başarılı; Katalog açılışı, 5 simgeli alt menü ve Profil kartlarının açılması görsel olarak doğrulandı. Diğer tüm düğmeler, büyük yazı ve RTL akışı bu görevde denenmedi. Kayıtlı verileri silen işlem yapılmadı.
