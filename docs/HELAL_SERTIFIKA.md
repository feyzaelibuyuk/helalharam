# Helal sertifikası

OFF halal/halal-certified etiketi yalnız kaynak beyanıdır. Belge doğrulaması olarak gösterilmez.
Sertifika bölümü web ve mobil gerçek ürün ekranında bulunur. İçerik bulgularını veya ürün değerlendirmesini otomatik değiştirmez.

Data/certificates.json yalnız proje bakımcısının resmî kayıt üzerinden kontrol ettiği belgeleri içerir.
Dosya şu an boştur; sahte örnek veya doğrulanmamış gerçek belge eklenmedi. Dış sertifika kurumuyla otomatik API bağlantısı yoktur.
Kayıt ekleme için barkod, OFF ürün adı, kurum (TSE/JAKIM/MUIS/BPJPH/DHCE/IFANCA), belge numarası, üretim yeri,
belgeye özel resmî bağlantı, geçerlilik başlangıç/bitiş, kayıt kontrol tarihi, scopeConfirmed, reviewedBy ve evidenceNote gerekir.
Kuruluş alan adı tek başına kanıt değildir; bakımcı numara/kapsam/tarihler/iptal durumunu belgeye özgü resmî kayıttan kontrol eder.
Genel doğrulama rehberi bağlantısı, marka adı veya logo tek başına yeterli değildir.
Ürün/üretim yeri kapsamı kontrol edilmeden scopeConfirmed işaretlenmemelidir. Son kullanıcı bu dosyayı web üzerinden değiştiremez.
Özel belge gönderimi ve inceleme akışı için [28. görev yönergesi](SERTIFIKA_BELGE_INCELEME.md) dosyasına bakın; inceleme kararı bu kayıt dosyasını otomatik değiştirmez.

Durumlar: etiket var/doğrulanmadı, bilgi yok, doğrulanmamış belge, geçerli kayıt, süresi dolmuş,
iptal/geri çekilmiş, henüz başlamamış, kontrolü eski, birden fazla üretim yeri kapsamı.
Kontrol 30 günden eskiyse yeniden kontrol gerekir. Tarihler Türkiye takvim gününe göre ele alınır.
Bir üretim yerinin belgesi bütün marka veya ülkelerdeki ürünlere genellenmez. Kullanıcı ambalajın üretim yerini karşılaştırır.
Sertifika bilgisi yokluğu haram hükmü değildir. Kayıtlar her sorguda yeniden kontrol edilir; OFF önbelleği belge süresini dondurmaz.

Kaynak: https://halalcertification.ie/verify-halal-certificates/
Resmî numara/QR kayıt doğrulaması ve belge kapsamı gereksinimleri esas alınır.
Testler: dotnet run --project tests/CertificateChecks/CertificateChecks.csproj
Test belgeleri yalnız bellek içindeki test fixture'larıdır; certificates.json içinde yer almaz.

