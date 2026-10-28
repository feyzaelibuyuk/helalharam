# Ücretsiz yerel etiket / menü çevirisi — Trello 25

Azure/Google hesabı, API anahtarı ve ücretli abonelik kullanılmaz. C# web/API ve MAUI uygulaması, sunucunun bilgisayarındaki LibreTranslate servisini çağırır. Çeviri motoru Python tabanlı dış bağımlılıktır; uygulama kodu C# kalır. Hocanın C# şartı sunumda bu ayrımla anlatılmalıdır.

## Yerel kurulum

Python 3.12 ile depo kökünde (PowerShell script çalıştırmak gerekmez):

```powershell
py -3.12 -m venv .local-translate
.local-translate\Scripts\python.exe -m pip install --index-url https://pypi.org/simple libretranslate==1.9.6
$env:PYTHONIOENCODING="utf-8"
.local-translate\Scripts\python.exe tools/LocalTranslation/server.py --host 127.0.0.1 --port 5000 --load-only en,tr
```

İlk açılışta EN/TR modelleri internetten indirilir; disk alanı ve süre gerekir. Sonraki çeviriler yerelde çalışır. Diğer diller gerektiğinde `--load-only en,tr,ar,ja` gibi eklenebilir; yalnız gerçekten yüklenen dil çiftleri desteklenir. Tüm dünya dilleri vaadi verilmez. Kurulumdan sonra `start-local-translation.cmd` dosyasına çift tıklamak servisi başlatır. Pencereyi açık bırakın. Web/API ayrı terminalde Development ortamında başlatılır. Android aynı mevcut web/API üzerinden bu servise erişir; telefonda Python kurulmaz. Bilgisayar kapalıyken bu yerel sunum düzeni çalışmaz.

Servis yalnız 127.0.0.1:5000 adresine bağlanır; genel internete veya rastgele ücretsiz servis sağlayıcısına metin gönderilmez. HTTP yönlendirmeleri ve sistem proxy kullanımı kapalıdır. Çeviri motoru adresi kullanıcı girdisiyle değiştirilemez. Eski Azure anahtarı kullanılmaz; eski TranslatorSetup aracı bu kurulumda çalıştırılmaz.

## Kullanım ve sınırlar

Web /translate; mobil Profil → Etiket / menü çevirisi veya OCR sonrası çeviri. Kaynak/hedef dil arayüz dilinden bağımsızdır. /languages gerçek model listesini ve desteklenen hedef çiftlerini sağlar (24 saat önbellek; modeller değişince webi yeniden başlatın). Yüklü olmayan çiftler açık hata verir. Servis kapalıysa metin korunur; sahte çeviri gösterilmez.

Etiketin tüm içindekiler, alerjen/eser miktar, besin değerleri ve diğer metni girilebilir. 12.000 karakter üstü reddedilir; otomatik kısaltılmaz. Özgün metin ve makine çevirisi ayrı gösterilir. Fotoğraf gönderilmez; yalnız kontrol edilip onaylanan metin yerel servise iletilir. Metin URL/DB/kalıcı çeviri önbelleğine yazılmaz. Otomatik dil güveni %80 altında elle kaynak dil seçilmesi istenir.

TR/EN/FR/DE özgün kuralları ayrı uygulanır. Makine çevirisi ön kontrolü helal/alerjen güvenliği garantisi değildir. Diğer hedefler için ek İngilizce ön kontrol ayrı gösterilir; gerekli model yoksa işlem başarısız olur. E kodu kaybı uyarılır; tüm anlam kayıpları saptanamaz. Menü, gerçek tarifi/et kaynağını doğrulamaz.

Dakikada 5 POST ve RAM içinde UTC günlük 50.000 hedef karakter uygulama sınırı sürer; ücret kotası değildir, bilgisayar yükünü sınırlar. Servis zaman aşımı 120 saniyedir; ilk modelleri başlatmak zaman alabilir. Otomatik tekrar denemesi yapılmaz.

## Doğrulama

Derleme ve kontrollü test sonuçları geliştirme sonunda raporlanır. Gerçek çeviri ancak yerel motor/model kurulumu ve gerçek metin testi tamamlanınca doğrulanmış sayılır. Android yeni APK akışı ayrıca test edilmelidir; Trello 25 bu kontroller bitmeden tamamlandı sayılmaz.

Resmî kaynaklar: https://docs.libretranslate.com/ ve https://docs.libretranslate.com/api/operations/translate/ . Açık kaynak yazılım ve indirilen modellerin lisans bildirimleri dağıtımda korunmalıdır.

Bu uyarlamada C# web ve TranslationChecks derlemesi 0 hata/0 uyarı verdi. Kontrollü servis testleri geçti (gerçek çeviri değildir); sonradan eklenen yüklü olmayan dil çifti testi derlendi. Yeni web uygulamasını başlatma Windows Uygulama Denetimi tarafından 0x800711C7 ile engellendi; tarayıcı uçtan uca test geçmedi. Koruma değiştirilmedi.

Android TestBuild de 0 hata ve önceden bilinen 4 XA4301 OCR kitaplık uyarısıyla derlendi. Yeni mobil akış emülatörde çalıştırılmadı.

## Gerçek yerel motor denemesi

LibreTranslate 1.9.6 kuruldu; resmi Argos indeksinden EN→TR 1.5 ve TR→EN 1.5 modelleri indirildi. Büyük dosya bağlantısı kesildiğinden resmi HTTPS sunucusunun Range desteğiyle parçalı indirme tamamlandı. /languages yalnız en/tr çiftlerini verdi; /detect İngilizce test metnine %100 döndürdü. /translate gerçek metni çevirdi, satırlar/E471/sayısal besin değerleri korundu. Ancak MILK/WHEAT/SOY gibi bazı büyük harfli sözcükler İngilizce kaldı ve “fat” yanlış anlamda “şişman” çevrildi. Bu modelin kalite sınırı gerçektir: tam ve doğru etiket çevirisi henüz sağlanmış sayılmaz, özellikle alerjen güvenliği için kullanılmaz. Sözlük eklemek genel çeviri doğruluğu garantisi vermez. Trello 25 devam ediyor.

Windows kullanıcı adındaki Türkçe karakterleri SentencePiece okuyamadığından başlatıcı model klasörünün Windows kısa yol adını kullanır; güvenlik koruması değişmez. Argos varsayılan INFO günlüğü kullanıcı metni içerdiğinden başlatıcı bu günlüğü devre dışı bırakır. Gerçek testten sonra konsolda kaynak metin görünmedi. Bu başlatıcıyla çalıştırın; ham libretranslate komutunu kullanmayın.

Gerçek motor testi C# web uçtan uca testinin yerine geçmez. Windows Uygulama Denetimi engeli devam eder; yeni web/Android çeviri akışı gerçek servise karşı henüz doğrulanmadı. Azure hesabı/anahtarı gerekmez, dış ücretli çeviri çağrısı yapılmadı.

## 28 Ekim 2026 — C# üzerinden gerçek motor doğrulaması

Önceki Windows engeli bu çalıştırmada tekrarlanmadı: izole 5095 Development web sunucusu normal `dotnet run` ile, kurulu motor normal başlatıcısıyla çalıştı. Güvenlik politikası değiştirilmedi. `tests/translation-live-http.py` gerçek C# API → LibreTranslate → sonuç akışında geçti. Yeni model indirilmedi; yalnız kurulu EN/TR kullanıldı.

Doğrulananlar: gerçek dil listesi, EN→TR yanıtı, özgün metnin birebir korunması, E471 korunması, no-store, onaysız isteğin ve kurulu olmayan Japonca hedefin reddi. Test sentetik ambalaj metni kullanır; motoru başlatmaz veya model kurmaz. Motor hazır değilse açıkça başarısız olur. Bu test çeviri doğruluğu ölçümü değildir.

Kalite sorunu tekrar görüldü: MILK/WHEAT/SOY çevrilmedi, “fat” besin bağlamında yanlış olarak “şişman” çevrildi. Otomatik sözlükle çıktı değiştirilmedi; tüm metnin güvenilir çevirisi sağlanmış sayılmaz. Trello 25 devam ediyor. Cihazdaki OCR→çeviri dokunma akışı bu kontrolde denenmedi.

Tekrar kontrol: kurulu yerel motoru ve izole web sunucusunu açtıktan sonra `python tests/translation-live-http.py`. Gerçek kullanıcı metni veya kişisel veri kullanılmaz. Test çevirisinin konsolda gösterilmesi yalnız sentetik örneğe özeldir.

## Eksik çeviri sözcük uyarısı

EN→TR çıktısında sınırlı bir içerik/besin terimleri listesi özgün metinle karşılaştırılır. Her iki metinde ayrı sözcük olarak aynı kalan terimler uyarının başında listelenir. Büyük/küçük harf ve Unicode sözcük sınırları dikkate alınır. Çıktı değiştirilmez. Gluten/laktoz gibi ortak terimler de eşleşebilir; bu nedenle mesaj olasılık olarak ifade edilir. Liste tüm dilleri veya tüm içerikleri kapsamaz ve anlam hatalarını tespit etmez.

28 Ekim 2026: Web derlemesi 0 hata/0 uyarı. Gerçek motor üzerinden `tests/translation-live-http.py` tekrar geçti: MILK/WHEAT/SOY kalan çıktıda özel uyarı bulundu; özgün metin ve E471 korundu, onaysız ve kurulu olmayan dil isteği reddedildi. Aynı API uyarıları mevcut web ve mobil çeviri ekranlarında gösterilir; yeni mobil cihaz testi yapılmadı. Modelin “fat” anlam hatası devam ediyor; görev 25 tamamlanmış değildir.
