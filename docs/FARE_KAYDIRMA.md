# Android emülatöründe fare kaydırması

İlk generic mouse sınırına rağmen kullanıcı kaydırmayı hâlâ hızlı buldu. Emülatörde tanılama, tekerlek hareketinin dokunarak sürükleme biçiminde geldiğini gösterdi; yalnız mouse Scroll dinlemek yeterli olmadı.

## Güncel davranış

- Doğrudan mouse Scroll olaylarında dikey eksen [-1,1], adım 24 dp ile sınırlıdır.
- Yalnız DeviceType.Virtual cihazlarda, native ScrollView/CollectionView üzerindeki tek parmak dikey sürüklemede Android touch-slop sınırından sonraki mesafe yüzde 20 oranında uygulanır. Touch-slop sınırı korunur; kaydırma düğme tıklamasına dönüşmemelidir.
- Emülatör ScrollView savrulması durdurulur. RecyclerView mevcut fling dinleyicisi yoksa dikey savrulmayı tüketir; mevcut dinleyici ezilmez.
- Fiziksel telefonun touch hareketi değiştirilmez. Emülatörde elle sürükleme de yavaşlar; bu bir emülatör kullanım ayarıdır.
- Yazı kutuları, metin seçimi, WebView harita/topluluk ekranları ve yatay hareketler bu dokunma dönüşümünün dışındadır. Birden fazla parmak dönüşümü sonlandırır.

Katalog önceki değişiklikte tek CollectionView olarak düzenlendi; başlık ve filtreler Header, durum Footer içindedir. İç içe iki dikey kaydırma alanı yoktur.

## Doğrulama

28 Ekim 2026: Güncel kaynak sonrasında Android DLL ve imzalı TestBuild APK oluşturuldu. Emülatör kurulumu ve hissiyat kontrolü ayrıca kaydedilir. Otomasyon scrollY değeri fiziksel fare tıkı sayısı değildir; kullanıcının tekrar denemesi gerekir.

Elle kontrol: Katalog ve Profil üzerinde küçük tekerlek adımları, arka arkaya adımlar, yukarı/aşağı yön, listenin başı/sonu ve düğmelere tek tıklama. Metin kutusunda seçim/yazma ve haritada hareket ayrıca kontrol edilmeli. Fiziksel telefon davranışı bu görevde ölçülmedi.
