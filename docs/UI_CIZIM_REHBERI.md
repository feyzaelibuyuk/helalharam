# Helal Rehber — tablet çizimi için ekran rehberi

Bu rehber bir tasarım önerisidir. Her ekranda bütün bilgileri aynı anda göstermek zorunda değilsin. Önce kullanıcı ne yapmak istiyor, sonra hangi sonuca ulaşacak diye düşün. Çizdiğin kartlara örnek metinler ve düğme adları koyabilirsin; marka/renk/yazı tipi kararlarını birlikte değiştirebiliriz.

## Ana gezinme

Şu an beş bölüm var: **Katalog — Favoriler — Menü — Deneyimler — Profil**. Alt menü ana ekranlarda sabit kalsın; ürün detayı, fotoğraf okuma gibi bir alt ekranda üstte geri düğmesi yeterli olabilir. Harita ve Bildirimler ayrı büyük sekme olmak zorunda değil; ilgili bölümlerde kolay bulunan girişleri olabilir.

## 1. Ana sayfa / Katalog

Amaç: markette tek elle hızlıca ürün sorgulamak.

- Üstte uygulama adı ve küçük profil/bildirim girişi.
- Bir arama alanı: “Ürün adı, marka veya barkod”.
- En belirgin üç eylem: **Barkod tara**, **Etiketi fotoğrafla oku**, **Menüyü oku**.
- Daha aşağıda ürün listesi ve istenirse ülke filtresi. Teknik ülke kodu yerine ad/arama seçimi tasarlanabilir.
- Ürün kartı: fotoğraf, marka + ürün adı, kısa değerlendirme, kaynağı ve Detayı aç düğmesi. Demo kayıtlar açıkça demo olmalı.
- Son aramalar/geçmiş bölümü tasarlanabilir ama bugün kalıcı tarama geçmişi özelliği yok; bunu gelecek bölüm olarak işaretle.

## 2. Barkod tarama

- Kamera alanı, barkodun hizalanacağı çerçeve, kısa yönlendirme.
- Barkodu elle gir, galeriden/etiketten devam et seçenekleri.
- Tararken, ürün aranırken ve bulunamadığında ayrı durumlar.
- “Ürün bulunamadı” ekranından **Etiketi okut** yoluna geçiş çok belirgin olsun.
- Barkod ürün kimliğidir; içerik metni veritabanından gelir. Veri eksikse bunu göstereceğiz.

## 3. Ürün detayı / Paketli gıda sonucu

Üst bölüm: ürün fotoğrafı, marka, ürün adı, barkod ve Favoriye ekle.

Sonuç bölümü:

- Kısıtlı madde eşleşmesi / şüpheli madde / yeterli bilgi yok gibi açık başlık.
- Neden bu sonuç çıktı? İlgili bileşen, gerekçe ve bilgi kaynağı.
- **Alerjen ve kişisel diyet uyarıları**, helal değerlendirmesinden ayrı bir kart.
- Sertifika bilgisi ayrı: hangi kurum, kapsam, geçerlilik, doğrulama durumu. Logo veya topluluk yorumu sertifika onayı gibi görünmesin.

Metin bölümü:

- **İçindekilerin tamamı** ve varsa alerjen/eser miktar metni.
- Özgün dilde metin ile tam çeviri ayrı bloklar. Hedef dil seçimi ve Çevir düğmesi.
- Besin değerleri kaynakta varsa ayrı bölüm. Eksik alanın yerine uydurma veri gelmesin.
- Çeviride makine hatası olabileceği kısa ve görünür biçimde yazsın.

Alt bölüm: alternatif ürünler, kaynak/güncelleme bilgisi, Bilgi düzelt düğmesi. Alternatifler yalnız gerçek uygun kayıt varsa dolmalı; şimdilik katalog kapsamı sınırlı. Paketli üründe şüpheli bileşen varsa gerekçesini söyle; restoran sorusu gibi “garsona danış” gösterme.

## 4. Etiket fotoğrafı / OCR

Üç adım halinde çizebilirsin:

1. **Fotoğrafı seç/çek:** etiket veya menü seçimi; alfabe; çekim ve galeri düğmeleri; netlik/ışık ipucu; varsayılan kapalı Kontrastı artır seçeneği.
2. **Metni kontrol et:** fotoğraf önizlemesi, düzeltilebilir tam metin, gerçek kaynak dili. Ekran fotoğrafı ve metni kolay karşılaştırılabilir yapsın.
3. **Devam et:** tüm metni karşılaştırdım onayı; İçeriği değerlendir veya Tamamını çevir.

Boş/bulanık/parlayan fotoğraf için yeniden çekim durumu çiz. Metin bulunması, her kelimenin doğru olduğu anlamına gelmez. Karanlık/silik fotoğrafta kontrast bazen yardım eder, bazen hata artırır. Yeni fotoğraf okunamazsa önceki düzenlenmiş metin ve ona ait önizleme korunur.

Kamera şu an telefonun sistem ekranıyla açılıyor. Çizeceğin özel kamera çerçevesi/flaş arayüzü sonraki geliştirme olabilir; şimdiden tasarımda ayırabilirsin.

## 5. Menü / açık yemek

- Menüyü fotoğrafla oku veya yemek adını/metni elle gir.
- Kaynak/hedef dil seçimi ve özgün menü + çeviri.
- Tasarımda yemekleri kartlar halinde gösterebilirsin: özgün ad, çevrilmiş ad, açıklamada belirtilen içerikler.
- Açık yemekte gerçek tarif bilinmeyebilir. Öne çıkan eylem **İşletmeye ne sorayım?** olsun.
- Soru kartı örnekleri: etin kaynağı, alkol kullanımı, jelatinin kaynağı, ortak ekipman/yağ. Sorular tarif hakkında bilgi almaya yardımcıdır.
- Soru kartının altında işletmenin dili ve Seslendir düğmesi.
- Alternatif yemek/yer önerisi, veri varsa gösterilir. Fotoğraftan görünmeyen bütün malzemeleri bildiğimizi söylemeyiz.

## 6. Harita ve seyahat / restoran

Harita üstünde: şehir ara veya konumumu kullan; ülke/şehir ve arama bölgesi. Konum otomatik alınmasın; kullanıcı düğmeyle başlatsın.

Haritanın altında ya da liste görünümünde restoran kartları:

- İşletme/şube adı, şehir/adres, varsa mesafe.
- Bilgi kaynağı: harita etiketi, işletme beyanı, topluluk deneyimi veya belgeli sertifika. Bunları aynı yeşil rozetle birleştirme.
- Detay, haritada aç/yol tarifi gibi eylemler tasarlanabilir; uygulanması ayrı değerlendirilecek.

Restoran detayında yemekler, kaynak bilgisi, sorulacak sorular ve deneyimler olabilir. İnternet/yerel sunucu yok ve sonuç bulunamadı durumlarını da çiz. Mevcut geliştirme sürümü bilgisayardaki yerel API'ye bağlı; bağımsız yayımlanmış mobil servis henüz yok.

## 7. Favoriler

- Kaydedilmiş ürün kartları; marka, ad, kısa kaynak/durum ve aç/kaldır.
- Boş liste: “Henüz favorin yok” ve Kataloğa git.
- Güncel bilgi için ürünü aç; eski kayıt uygunluk onayı değildir.
- Bugünkü favori altyapısı ürünler içindir. Restoran/yemek favorisi istersen tasarımda gelecek özellik diye yer ayırabiliriz. Kalıcı tarama geçmişiyle favoriler aynı şey değil.

## 8. Deneyimler / topluluk

- İşletme, şube, yemek, ziyaret tarihi ve sorulan soru/alınan cevap içeren kartlar.
- Faydalı / güncel değil oyları, katkı rozeti ve kaynak bağlantısı.
- **Deneyim paylaş**, **Paylaşımlarım**, **İnceleme bildirimlerim** girişleri.
- Paylaşım formu: işletme, şehir/ülke, yemek, tarih, soru, cevap, isteğe bağlı kaynak ve yayın onayı.
- İnceleme bekliyor/yayında/gizlendi/geri çekildi durumları.
- Mevcut rozetler katkı sayısını anlatır; uzmanlık veya doğrulanmış helal rozeti değildir.
- Cihazdaki özel taslak notlar ikincil, açılır bir alan olabilir. Özel not ile herkese yayınlanan paylaşımın farkı açık olsun.

## 9. Profil / tercihler

Öncelikli bölüm: **Beslenme tercihlerim**.

- Helal profil bilgisi. Diğer din profilleri henüz uygulanmadı; aktif seçenek gibi çizmek yerine gelecek alan olarak ayır.
- Alerjen seçimi, vegan/vejetaryen tercihleri, gluten/laktozdan kaçınma ve özel kaçınılan içerik alanı.
- Dil seçimi: arayüz dili, metnin kaynak dili ve çeviri dili birbirinden farklı işler; tek seçimmiş gibi birleştirme. Bugünkü arayüz TR/EN/AR kısmi, OCR ve çeviri modelleri ayrıca sınırlı.

İkincil bölüm: çeviri, harita, sertifika kaynakları ve belge fotoğrafı gönderimi.

Ayarlar bölümü: gizlilik, geri bildirim, sürüm ve bağlantı kontrolü. Yerel sunucu/IP gibi teknik açıklamalar normal kullanıcının ana ekranını doldurmasın. Doğrulanmış hesap/kayıt/giriş henüz yok; şimdilik zorunlu giriş ekranı gerekmiyor.

## 10. Bildirimler

Şu an topluluk paylaşımlarının durum değişiklikleri gösteriliyor. Kartta işletme/yemek, durum, tarih ve ilgili kayda git eylemi bulunabilir. Boş liste ve yenileme durumu çiz.

Telefonun arka plan push bildirimi, favori içerik değişikliği ve bütün düzeltme sonuçlarının bildirilmesi henüz bağlı değil; bunları ayrı gelecek başlığı olarak düşün.

## Çizim yaparken

- İlk turda Ana sayfa, Ürün sonucu, OCR kontrol, Menü/soru, Harita ve Profil en önemli altı çizim.
- Her ekranda bir ana düğme öne çıksın. Diğer işlemler ikincil olabilir.
- Aynı işi iki farklı başlık altında tekrarlamaktan kaçın; kart başlıkları kısa olsun.
- Sadece renk kullanma: uyarının yanında yazı ve simge de olsun. Örneğin sarı “Şüpheli”, gri “Yetersiz bilgi”.
- Boş, yükleniyor, hata ve sonuç durumlarını küçük ek çizimlerle göster.
- Geri düğmesinin nereye götüreceğini ve bir düğmeye basınca hangi ekranın açılacağını oklarla işaretle.
- Tek uzun sayfa yerine özet + açılabilir ayrıntı kullanabilirsin. Tam etiket metni yine erişilebilir kalsın.
- Yönetici inceleme ekranı ayrı web panelidir; ilk mobil çizim turuna eklemek zorunda değilsin.

Renk, köşe, ikon, kart boyutu ve yerleşim özgür. Bu liste fonksiyonları kaybetmemek için; mevcut görünümü kopyalaman gerekmiyor.
