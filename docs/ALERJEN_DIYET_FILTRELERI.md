# 30 — Alerjen ve özel diyet tercihleri

## Kullanım

Web: üst menü **Alerjen / diyet tercihleri** (`/food-preferences`). Mobil: **Profil → Alerjen / diyet tercihleri**. Süt, yumurta, gluten içeren tahıllar, yer fıstığı, sert kabuklu yemişler, soya, susam, balık, kabuklular, yumuşakçalar, kereviz, hardal, acı bakla ve sülfitler başlangıç listesidir. Bu liste bütün alerjileri veya ülkelerin bütün kurallarını kapsamaz.

Vegan/vejetaryen, glutenden ve laktozdan kaçınma ayrı tercihlerdir. Ek kaçınılan içerikler virgülle girilebilir: en fazla 8, her biri 2–60 karakter, toplam 500 karakter. Tanı/sağlık öyküsü/kişisel bilgi yazılmamalıdır. Eski mobil **kaçınılan içerik notu** otomatik filtre değildir; yeni kurallara otomatik aktarılmaz.

Seçimler, OFF ve yerel ürün detaylarında, elle girilen/OCR'dan kontrol edilen içerikte ve çeviri sonuçlarında eşleşmeleri öne çıkarır. Çeviri ekranında özgün metin ve makine çevirisi ayrı kontrol edilir. Çeviri hataları güvenli sonuç oluşturmaz. Menü kontrolü gerçek tarif, et suyu veya ortak ekipmanı doğrulamaz. Filtreler katalog kayıtlarını gizleyerek kalanları güvenli ilan etmez; seçili içeriklerle ilgili uyarıları gösterir.

## Eşleşmelerin anlamı

- **Kaynak alerjen etiketi:** OFF `allergens_tags` beyanı.
- **Kaynak eser miktar / çapraz temas etiketi:** OFF `traces_tags` beyanı; ilkinden ayrı tutulur.
- **Metinde eşleşme:** sözcük eşleşmesi; içerik/uyarı/olumsuzluk bağlamı ayrıca kontrol edilir, kesin mevcudiyet sonucu değildir.
- **Olası içerik / eser miktar beyanı:** tanınan `may contain`/`eser miktarda` vb. ifadeler.
- **İçermez beyanında eşleşme:** metinde yazılı iddia; bağımsız doğrulama değildir ve başka bir bileşen eşleşmesini bastırmaz.

Metin sözlüğü sınırlı TR/EN/FR/DE'dir. Diğer dillerde OFF'nin tanınan standart etiketleri işlenebilir; ek içerikler yalnız birebir sözcük olarak aranır. Her başlıkta en fazla 8 örnek gösterilir; tekrar eden bir alerjen diğer alerjenin uyarısını düşürmez. Sülfit miktarı/eşiği, gluten veya laktoz miktarı ölçülmez. Laktozdan kaçınma açık laktoz sözcüklerini ve süt türevlerini işaretler; süt türevi eşleşmesi laktoz miktarını göstermez. Laktozsuz beyanı süt proteini/alerjisi değerlendirmesi değildir.

**Bulgu olmaması alerjensiz, uygun veya tüketimi güvenli onayı değildir.** Tam ambalajdaki alerjen/iz uyarıları ve özgün içerik korunur. Helal kaynak kuralları/sertifika ile kişisel alerjen-diyet kontrolü ayrı başlıklar ve ayrı sonuçlardır. Hiçbiri diğerini geçersiz kılmaz. Tıbbi/klinik doğrulama veya otomatik restoran uygunluk kararı uygulanmaz.

OFF yanıtındaki alerjen/iz etiketleri tercih seçilmemiş olsa da ürünün kaynak bilgilerinde gösterilir. Boş etiket listesi yokluk kanıtı değildir. OFF sorgusuna ek alanlar dahil edildi; önbellek anahtarı v4'e geçirildi. Önbellek kişisel tercihler içermez; aynı ham ürün verisi kullanıcıya göre değiştirilmez.

## Saklama ve güvenlik

Mobil seçimler MAUI SecureStorage'da tutulur, ağ isteğine eklenmez. Web seçimleri Data Protection ile korunan HttpOnly/SameSite=Strict çerezde en fazla 365 gündür; HTTPS'te Secure kullanılır. Uygulama sunucusu sayfayı oluştururken çerezi okur; veritabanına, OFF'ye veya LibreTranslate'e göndermez. Web ve mobil arasında hesap/eşitleme yoktur. Tercihler ekranından temizlenebilir. Güvenli depo okunamazsa mobil ekranda kişisel kontrolün uygulanamadığı yazılır.

Web kaydetme/temizleme POST+CSRF korumalı; istek gövdesi 16 KiB ile sınırlı. Tercih sayfası ve kişisel sonuçlar no-store kullanır. Ürün detayları normal sunucu render'ıyla tercih çerezini her açılışta okur; yeniden deneme tam sayfa isteğidir. Webde tercih değişikliğinden sonra ürünü yeniden açmak veya metni tekrar değerlendirmek gerekir. Mobilde tercihlerden geri dönünce açık kişisel paneller güncellenir; metin ve çeviri korunur. Etkileşim için cihaz kontrolü ayrıca gerekir.

## Dayanak kaynaklar

- [Birleşik Krallık resmî alerjen etiketleme rehberi](https://www.gov.uk/understanding-food-labelling/allergen-labelling) — 14 başlangıç grubu; miktar eşiğini uygulama ölçmez.
- [FDA: Food Allergies — What You Need to Know](https://www.fda.gov/food/buy-store-serve-safe-food/food-allergies-what-you-need-know) — farklı başlangıç listeleri ve kaynak/iz beyanlarının bağlamı. Uygulama küresel eksiksiz bir alerjen sınıflandırması olarak sunulmaz.

## Doğrulama — 27.10.2026

Web derlemesi 0 hata/0 uyarı. Son Android APK derlemesi 0 hata ve önceden mevcut 4 OCR yerel kitaplığı XA4301 uyarısıyla tamamlandı. Mevcut AdminHttpChecks regresyonları geçti. Yeni `tests/PreferenceChecks` DLL'si ve mevcut OffChecks çalıştırıcısı Windows Uygulama Denetimi tarafından engellendi: **bu C# testler çalışmış/geçmiş sayılmaz. Koruma değiştirilmedi, engellenen dosyalar başka yolla çalıştırılmadı.**

Açık ve normal çalışan web uygulamasına bağımsız HTTP istekleri yapan `tests/food-preferences-http.py` başarıyla çalıştı. Engellenen DLL'yi yüklemez. Ayrı cookie jar'ları ve sentetik metinlerle: CSRF, korumalı çerez bayrakları, kullanıcı ayrımı, no-store, kaydetme/temizleme, dil/kelime sınırları, içermez/iz beyanları, laktoz, 14 örnek grup, bulgu sınırında farklı alerjenin korunması, ek içerik sınırları kontrol edildi.

Sunucu ayrı test veritabanıyla 5095 portunda açıkken:

`python tests/food-preferences-http.py http://127.0.0.1:5095`

HTTP testinde Türkçe “İNEK SÜTÜ” biçimi de doğrulandı. Tarayıcıda 14 gruptan birer örnek, İngilizce milk-free + whey + may contain peanuts, Fransızca œuf, Rusça birebir ek içerik, eksik metin ve güvenli sonuç vermeyen eşleşmesiz örnek görüldü. Canlı OFF ürün sorgusunda metinde geçmeyen süt/kuruyemiş/soya kaynak etiketleri de gösterildi; bu ürünün bütün bilgilerinin doğrulandığı anlamına gelmez. Sentetik test tercihleri iş bitiminde tarayıcıdan temizlendi.

Android üzerinde SecureStorage kaydetme/temizleme, kamera/OCR sonrası yerel kontrol ve gerçek cihaz kullanımı ayrıca denenmelidir. Tüm dünya dilleri, tüm alerjiler, klinik doğruluk veya eksiksiz çapraz temas tespiti tamamlanmış sayılmaz. Bu görev temel kişisel eşleşme/uyarı akışıdır.
