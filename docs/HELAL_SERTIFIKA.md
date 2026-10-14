# Helal sertifikası

OFF halal/halal-certified etiketi yalnız kaynak beyanıdır. Belge doğrulaması olarak gösterilmez.
Sertifika bölümü web ve mobil gerçek ürün ekranında bulunur. İçerik bulgularını veya ürün değerlendirmesini otomatik değiştirmez.

Data/certificates.json yalnız proje bakımcısının resmî kayıt üzerinden kontrol ettiği belgeleri içerir.
Dosya şu an boştur; sahte örnek veya doğrulanmamış gerçek belge eklenmedi. Dış sertifika kurumuyla otomatik API bağlantısı yoktur.
Kayıt ekleme için barkod, OFF ürün adı, kurum (DHCE/IFANCA), belge numarası, üretim yeri,
belgeye özel resmî bağlantı, geçerlilik başlangıç/bitiş, kayıt kontrol tarihi ve scopeConfirmed gerekir.
Genel doğrulama rehberi bağlantısı, marka adı veya logo tek başına yeterli değildir.
Ürün/üretim yeri kapsamı kontrol edilmeden scopeConfirmed işaretlenmemelidir. Son kullanıcı bu dosyayı web üzerinden değiştiremez.

Durumlar: etiket var/doğrulanmadı, bilgi yok, doğrulanmamış belge, geçerli kayıt, süresi dolmuş,
iptal/geri çekilmiş, henüz başlamamış, kontrolü eski, birden fazla üretim yeri kapsamı.
Kontrol 30 günden eskiyse yeniden kontrol gerekir. Tarihler Türkiye takvim gününe göre ele alınır.
Bir üretim yerinin belgesi bütün marka veya ülkelerdeki ürünlere genellenmez. Kullanıcı ambalajın üretim yerini karşılaştırır.
Sertifika bilgisi yokluğu haram hükmü değildir. Kayıtlar her sorguda yeniden kontrol edilir; OFF önbelleği belge süresini dondurmaz.

Kaynak: https://halalcertification.ie/verify-halal-certificates/
Resmî numara/QR kayıt doğrulaması ve belge kapsamı gereksinimleri esas alınır.
Testler: dotnet run --project tests/CertificateChecks/CertificateChecks.csproj
Test belgeleri yalnız bellek içindeki test fixture'larıdır; certificates.json içinde yer almaz.

