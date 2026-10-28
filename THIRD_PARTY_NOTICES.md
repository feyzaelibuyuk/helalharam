# Üçüncü taraf kaynakları ve lisans bildirimleri

Bu belge projeye özgü kodun lisansı değildir. Üçüncü taraf kaynaklarının hakları kendi sahiplerine aittir. NuGet/Python bağımlılıklarının ve modellerin lisansları birbirinden ayrıdır; aşağıdaki kaynak atıfları bunların yerine ortak bir lisans vermez. Uygulama ikili paketlerinin dağıtımından önce doğrudan ve geçişli bağımlılıkların dağıtım koşulları ayrıca gözden geçirilmelidir.

## Depoda bulunan dosyalar

- **Open Sans Regular / Semibold**: fontların gömülü bildirimleri Google Corporation, 2010–2011 / 2011; Apache License 2.0. Depodaki bu font sürümleri için [tam lisans](helalharam/Resources/Raw/OpenSans-APACHE-2.0.txt) ve [telif bildirimi](helalharam/Resources/Raw/OpenSans-NOTICE.txt) Android ham varlıklarına da eklenmiştir. Fontlar değiştirilmedi.
- **Bootstrap 5.3.3**: Copyright 2011–2024 The Bootstrap Authors; [MIT lisansı](BlazorApp1/wwwroot/lib/bootstrap/LICENSE). Kaynak dosyaların mevcut başlıkları korunur.
- **Popper 2.11.8** (Bootstrap bundle içinde): Copyright 2019 Federico Zivolo; [MIT lisansı](BlazorApp1/wwwroot/lib/bootstrap/POPPER-LICENSE).
- **Leaflet 1.9.4**: Volodymyr Agafonkin / CloudMade; [BSD 2-Clause lisansı](BlazorApp1/wwwroot/lib/leaflet/LICENSE).
- **GeoNames şehir verisi**: CC BY 4.0. Kaynak, dönüştürme ve kapsam bilgisi [TRAVEL_CITIES_SOURCE.md](BlazorApp1/Data/TRAVEL_CITIES_SOURCE.md) içindedir. Yerel JSON 34.188 yerleşim içerir; otomatik güncellenmez.

## Dış servisler ve ayrı kurulan modeller

- **Open Food Facts**: veritabanı ODbL, tekil veritabanı içerikleri DbCL; ürün fotoğrafları CC BY-SA. [Resmî lisans açıklaması](https://openfoodfacts.github.io/documentation/docs/Product-Opener/api/tutorials/license-be-on-the-legal-side/). Uygulama kaynak bağlantısı/atıf gösterir; bu repo dış ürün veritabanını veya ürün fotoğraflarını topluca içermez.
- **OpenStreetMap**: © OpenStreetMap contributors, ODbL; [telif ve lisans](https://www.openstreetmap.org/copyright). Harita döşemesi ve Overpass servislerinin kullanım koşulları veri lisansından ayrıca değerlendirilmelidir.
- **Helsinki-NLP / OPUS-MT, opus-mt-tc-big-en-tr**: CC BY 4.0; [resmî model kartı](https://huggingface.co/Helsinki-NLP/opus-mt-tc-big-en-tr). Model uygulama tarafından eğitilmedi; ağırlıklar depoya eklenmez. [Deneme ve revision bilgisi](docs/OPUS_MODEL_DENEMESI.md).
- **LibreTranslate / Argos Translate**: isteğe bağlı, ayrı yerel kurulum. LibreTranslate AGPL-3.0, Argos Translate MIT veya Apache-2.0 kapsamında sunulur; indirilen model paketlerinin kendi lisansları ayrıca geçerlidir. [LibreTranslate](https://github.com/LibreTranslate/LibreTranslate), [Argos Translate](https://github.com/argosopentech/argos-translate). Bu depoya bu servislerin kurulum ortamları veya model dosyaları eklenmez.

.NET / MAUI, Entity Framework, SQLite, ZXing, OCR/ML Kit bağlamaları ve Python bağımlılıklarının sürümleri proje/requirements dosyalarındadır. Paket yöneticileri üzerinden geri yüklenen bileşenler kendi lisanslarını korur; bu bildirim eksiksiz bir geçişli bağımlılık lisans envanteri iddiası taşımaz.
