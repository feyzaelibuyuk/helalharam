# İçerik değerlendirmesi v1

Profil: Helal — DHCE/IFANCA kaynak kontrolü yaklaşımı.
Kural sürümü: helal-source-check/1.0. Kaynaklar 14.10.2026 tarihinde incelendi.
Bu profil bütün dini yorumları temsil etmez; kaynak kuralları ve kapsamı sonuç ekranında açıklanır.

## Kurallar

- Domuz ve açıkça domuz türevi ifadeler: seçilen profile göre kısıtlı içerik.
- Jelatin: kaynak/üretim bilgisi doğrulanmadığında şüpheli. Sığır veya balık adı tek başına sertifika kabul edilmez.
- E471, mono/digliserit: kaynağı belirsizse şüpheli. Aynı bileşene açıkça bağlı bitkisel beyan ayrı gösterilir.
- Lesitin/E322: kaynak belirsizse şüpheli. Aynı bileşene bağlı soya/bitkisel beyan ayrı gösterilir.
- Aroma: kaynak ve taşıyıcı bilgisi belirsizse şüpheli.
- Etil alkol/etanol: kullanım ve işlem bilgisi eksik olduğundan kaynak kontrolü uyarısı.

İçermez ve iz/olası içerik beyanları kesin mevcut içerik gibi ele alınmaz.
Bitkisel yağdan sonra gelen başka bir katkıya bitkisel beyan taşınmaz.
Aynı maddeden birden fazla söz edildiğinde her eşleşme kontrol edilir.
Risk eşleşmemesi yalnız sınırlı kurallarda eşleşme olmadığı anlamına gelir; Uygun veya helal sertifikalı sonucu üretilmez.
Etanol kuralı şeker alkollerini ve chocolate liquor ifadesini kapsamaz.

## Dil ve kapsam

TR/EN/FR/DE terimleri desteklenir. Desteklenmeyen veya belirtilmeyen dilde Yetersiz bilgi döner.
OCR/çeviri henüz bağlı değildir. Dil seçimi kullanıcı veya OFF dil alanına dayanır; yanlış belirtilmiş dil bağımsız doğrulanamaz.
Metin eşleştirme sınırlıdır; tüm eşanlamlılar, katkılar ve karmaşık gramer desteklenmez.
Üretim, kesim, tedarik, bulaşma ve sertifika kontrolü yapılmaz. E120 gibi ayrıca yorum gerektiren kurallar bu ilk sürüme eklenmedi.
Kural sonucu doğrulanmış ürün/alternatif listesine otomatik aktarılmaz.

## Kaynaklar

- DHCE: https://halalcertification.ie/certification-requirements/
  Hayvansal kısıtlar, jelatin/aroma kaynakları ve üretim/izlenebilirlik gereksinimleri.
- IFANCA Pakistan: https://ifancahalal.pk/faqs/?include_category=general-halal
  Mono/digliserit ve lesitin kaynaklarının değerlendirilmesi, chocolate liquor ayrımı.
- EFSA: https://www.efsa.europa.eu/en/consultations/call/call-food-additives-usage-level-andor-concentration-data-food-0
  E471 kodunun mono/digliseritlerle eşleştirilmesi. EFSA kaynağı dini uygunluk kaynağı değildir.

## Deneme

Web: /ingredients → metin ve dil seç.
Şeker, jelatin, E471 → Şüpheli gıda.
Şeker, domuz jelatini → Seçilen profile göre uygun değil.
Soya lesitini → beyan gösterilir; helal sertifikası verilmez.
OFF barkod sonuçlarında bulgular, gerekçeler ve kaynak bağlantıları otomatik görünür.

API: POST /api/ingredients/assess {"ingredients":"Şeker, jelatin, E471","language":"tr"}
Metin yalnız yerel sunucuda incelenir. En fazla 12.000 karakter.
Web elle giriş GET formu metni yerel URL'ye ekler.
Testler: dotnet run --project tests/IngredientChecks/IngredientChecks.csproj
OFF entegrasyonu: dotnet run --project tests/OffChecks/OffChecks.csproj
