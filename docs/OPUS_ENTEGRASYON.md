# OPUS yerel çeviri geçidi

## Çalıştırma

Depo kökünde `start-opus-translation.cmd` dosyasını açıp "OPUS ready" mesajını bekleyin. Model ve bağımsız Python ortamı önceki denemede bu bilgisayara kurulmuştur. Başka bilgisayara Git klonlamak modelleri getirmez: ortam/model kurulumu için OPUS_MODEL_DENEMESI.md ve değerlendirme aracı gerekir. Başlangıçta otomatik indirme yoktur.

C# API artık sabit `127.0.0.1:5002` geçidine bağlanır. Webi yeniden başlatın. Android aynı C# API'yi kullanır; telefona Python/model kurulmaz. İlk denemede kaynak English (en), hedef Turkish (tr) seçin. Metin/OCR onayı ve yerel servise gönderim onayı gereklidir.

İngilizce→Türkçe yalnız OPUS-MT kullanır. Geçit/model kapalıysa hata alınır; eski Argos modeline sessizce dönülmez. Diğer çiftler için eski `start-local-translation.cmd` ayrıca 5000 portunda açık olabilir; geçit gerçek dil listesini birleştirir. LibreTranslate kapalıysa yalnız EN→TR ve aynı dil geçişleri sunulur. Otomatik dil algılama eski yerel servise bağlıdır; kapalıysa kaynak dili elle seçme açıklaması verilir. Modeller/dil çiftleri değiştiğinde C# webi yeniden başlatın (24 saatlik dil listesi önbelleği).

## Metin işleme

12.000 karakter üstü açıkça reddedilir. Uzun satırlar 192 kaynak token parçasına ayrılır; `truncation=False` kullanılır, son bölüm atılmaz. LF/CRLF/CR ve boş satırlar korunur; parça içindeki boşluklar modele bağlı değişebilir. Üretilen parça EOS ile tamamlanmadıysa tüm istek hata verir, kısmi başarı gösterilmez. Parçalara ayırmak bağlamı/kaliteyi etkileyebilir. Üç örnekteki başarı genel doğruluk garantisi değildir.

Çıktıda motorun adı gösterilir. Özgün metin C# API'de birebir korunur. Mevcut E kodu kaybı ve çevrilmeden kalan terim uyarıları geçerlidir; tüm sayı/anlam kayıpları otomatik tespit edilmez. Alerjen yokluğu veya helal onayı çıkarılmaz.

## Gizlilik ve sınırlar

Geçit yalnız 127.0.0.1 üzerinde dinler. Model yerel dosyadan yüklenir; çeviri metni internete gönderilmez. Diğer motor için sabit loopback 5000 kullanılır; proxy ve yönlendirme kapalıdır. İstek metni günlüğe yazılmaz. Geçit bir OPUS isteği işler; eşzamanlı ikinci istek 429 verir. C# kota/onay kontrolleri sürer. Teknik testte sentetik metin kullanılır. Durdurmak için geçit penceresinde Ctrl+C.

## Kontrol

`python tests/opus-gateway-http.py` yalnız gerçek yerel geçidi kontrol eder; C# uçtan uca testi yerine geçmez. C# üzerinden kontrol için ayrı izole web sunucusunda `tests/translation-quality-http.py` kullanılabilir. Son yürütme sonuçları geliştirme raporunda belirtilir.

### 28 Ekim 2026 sonuçları

Web build 0 hata/0 uyarı. `tests/opus-gateway-http.py` gerçek OPUS geçidinde geçti: üç kalite örneği; birden fazla parçaya bölünen uzun metnin son işareti; CRLF/LF ve boş satırlar; E471 ve örnek sayılar; tür/karakter sınırı ve bilinmeyen yol. Yeni C# uygulaması normal `dotnet run` sırasında Windows Uygulama Denetimi tarafından engellendi. Başka yükleyiciyle çalıştırma denenmedi, güvenlik politikası değiştirilmedi. Bu nedenle güncel C#→OPUS uçtan uca testi geçti denemez. Android derleme ve cihaz durumu ayrıca aşağıda belirtilir.

Android Compile hedefi 0 hata/0 uyarı ile geçti. Yeni APK paketleme, Android dokunma akışı ve C# uygulamasının Windows engeli sonrasında çalıştırılması yapılmadı.

### C# uçtan uca tamamlanan kontrol ve protokol düzeltmesi

28 Ekim 2026 sonraki denemede normal `dotnet run` çalıştı; güvenlik koruması değiştirilmedi. Gerçek entegrasyon denemesinde C# `PostAsJsonAsync` isteğinin chunked gövde göndermesi ve geçidin Content-Length beklemesi nedeniyle 502 alındı. Algılama/çeviri gövdeleri sınırlı JSON olarak bellekte hazırlanıp `StringContent` ile gönderilerek Content-Length sağlandı. Özgün içerik değiştirilmedi. Web build 0 hata/0 uyarı.

Düzeltme sonrası `tests/translation-quality-http.py` üç örneğin tamamında gerçek C# API→OPUS→sonuç akışında geçti. Ayrı uzun metin isteğinde özgün metin birebir, sağlayıcı OPUS, E471, son 987654 işareti, örnek sayılar, CRLF/LF ve boş satırlar doğrulandı. Geçit durdurulduktan sonra C# API 502 ve boş Document döndürdü; sahte başarı/eski motora dönüş olmadı. Yalnız bu görevin açtığı test sunucusu ve geçit kapatıldı.

Bu sonuç önceki çalıştırma engelinden kalan C# uçtan uca belirsizliğini giderir. Üç kalite örneği genel çeviri doğruluğu değildir; gerçek ambalajlar ve Android dokunma akışı ayrıca denenmelidir.
