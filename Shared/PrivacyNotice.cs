namespace HelalHaram.Shared;
public static class PrivacyNotice
{
    public const string Limits = "Eksik içerik, okunamayan metin veya risk bulunamaması; helal onayı, alerjen yokluğu ya da tüketim güvenliği anlamına gelmez. Etiketteki alerjen ve eser miktar uyarılarını ayrıca kontrol edin. Kişisel alerjen filtresi ve tam etiket çevirisi henüz uygulanmadı.";
    public static IReadOnlyList<(string Title, string Text)> Sections { get; } = new (string, string)[]
    {
        ("Bu sürüm", "Bu açıklama geliştirme/test sürümü içindir. Veriler eksik veya eski olabilir. Yayından önce işletmeci, iletişim, saklama süreleri ve başvuru kanalı ayrıca belirlenmelidir."),
        ("Fotoğraf ve kamera", "Android uygulamasında barkod ve Latin alfabeli OCR cihazda işlenir. Fotoğraf analiz sunucusuna yüklenmez. İçerik analizini istediğinizde kontrol ettiğiniz metin ve dil uygulama sunucusuna gönderilir. Menü soruları cihazda hazırlanır. OCR için oluşturulan geçici işleme dosyası işlem sonunda silinir; kamera veya fotoğraf sağlayıcısının sakladığı asıl fotoğraf otomatik silinmez. İzinleri cihaz ayarlarından yönetebilirsiniz."),
        ("Ürün sorguları", "Barkod sorgusu uygulama sunucusundan Open Food Facts'e gönderilir. Sonuçlar sunucuda geçici önbelleğe alınabilir. Kaynak veya belge bağlantısını açarsanız ilgili sitenin gizlilik koşulları geçerlidir. Konum ve harita servisi henüz bağlı değildir."),
        ("Yerel veriler ve çerezler", "Mobil favoriler, kaçınılan içerik tercihi ve deneyim taslağı cihazda saklanır; hesaplar arasında eşitlenmez. Favorileri ve deneyim taslağını kendi ekranından silebilirsiniz. Kaçınılan içerik alanını boşaltıp kaydetmek tercihi temizler. Web favorileri en fazla 365 günlük korumalı çerezde saklanır; Favoriler ekranından temizlenebilir. Güvenlik ve oturum çerezleri de kullanılır. Yönetici oturumu en fazla 30 dakikadır; Çıkış yap ile kapatılabilir."),
        ("Düzeltme önerileri", "Öneri, kaynak bağlantısı, ilgili kayıt ve zaman bilgisi sunucu veritabanında saklanır. Yetkili yönetici öneriyi ve karar geçmişini inceleyebilir; bunlar herkese açık listelenmez. Ad, telefon, sağlık bilgisi veya başkalarının özel bilgilerini yazmayın. Otomatik silme süresi ve kullanıcı silme başvuru ekranı henüz uygulanmadı."),
        ("Bağlantı ve teknik kayıtlar", "Sunucu IP adresi gibi teknik bağlantı bilgilerini işler; istek sınırlandırması IP'ye göre yapılır. Sunucu ve barındırma günlükleri teknik istek bilgileri içerebilir. Web içerik formu metni sayfa adresine ekler; metin tarayıcı geçmişinde ve adres kaydeden günlüklerde kalabilir. Kişisel veya hassas bilgi girmeyin; sonucu paylaşmadan önce adresi kontrol edin."),
        ("Sonuçların sınırları", Limits)
    };
}
