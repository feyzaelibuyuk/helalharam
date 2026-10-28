# Android emülatöründe fare kaydırması

Kullanıcının doğruladığı sorun: Android emülatöründe fare tekerleğiyle sayfanın çok hızlı ilerlemesi.

Katalogdaki ScrollView içinde CollectionView yapısı tek CollectionView'e dönüştürüldü. Arama/filtre/okuma düğmeleri Header'da, açıklama/durum Footer'da; ürünler sanallaştırılmış listede. Böylece iç içe iki dikey kaydırma alanı yoktur. Sonuç güncellemelerinde KeepScrollOffset kullanılır.

Android'de ScrollView ve CollectionView native platform görünümlerine yalnız mouse Scroll olayını ele alan dinleyici bağlanır. Vscroll ekseni olay başına [-1,1] ile sınırlandırılır, adım 24 dp olarak ekran yoğunluğuna çevrilir. Parmak touch/fling olayları bu dala girmez. Yatay mouse kaydırması ve diğer olaylar native davranışa bırakılır. WebView içindeki harita/topluluk veya web tarayıcısı için bu dinleyici uygulanmaz.

Android TestBuild Compile hedefi 0 hata/0 uyarıyla geçti; XAML ayrıca XML olarak doğrulandı. Bu görevde yeni tam APK paketlemesi ve emülatörde fare hissiyatı doğrulaması yapılmadı. Sonraki denemede Katalog ve Profil'de birer tekerlek adımı, art arda adımlar, listenin başı/sonu, parmak sürükleme ve sonuç güncellemesi kontrol edilmeli. Kaydırma adımı kullanıcının geri bildirimiyle ayarlanabilir.
