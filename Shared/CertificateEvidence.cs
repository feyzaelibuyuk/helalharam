namespace HelalRehber.Shared;
public sealed record CertificateAuthority(string Id,string Name,string Region,string Url,string Purpose,string[] Hosts);
public static class CertificateAuthorities
{
    public static IReadOnlyList<CertificateAuthority> All { get; } = Array.AsReadOnly(new[] {
        new CertificateAuthority("TSE","Türk Standardları Enstitüsü","Türkiye","https://basvuruportal.tse.org.tr/Genel/sorgulamalar.aspx","Belge / helal kapsamı sorgulama",["tse.org.tr","www.tse.org.tr","basvuruportal.tse.org.tr","belge.tse.org.tr"]),
        new("HAK","Helal Akreditasyon Kurumu","Türkiye / uluslararası","https://www.hak.gov.tr/akredite-kuruluslar","Kuruluşun akreditasyonu; ürün sertifikası değildir",["hak.gov.tr","www.hak.gov.tr"]),
        new("JAKIM","JAKIM / Halal Malaysia","Malezya","https://www.halal.gov.my/","Helal kayıt sorgulama",["halal.gov.my","www.halal.gov.my","myehalal.halal.gov.my"]),
        new("MUIS","MUIS","Singapur","https://halal.muis.gov.sg/halal/establishments","Sertifikalı işletme sorgulama",["muis.gov.sg","www.muis.gov.sg","halal.muis.gov.sg"]),
        new("BPJPH","BPJPH","Endonezya","https://bpjph.halal.go.id/","Ürün / sertifika numarası sorgulama",["bpjph.halal.go.id","ptsp.halal.go.id"]),
        new("DHCE","Department of Halal Certification Europe","İrlanda / uluslararası","https://halalcertification.ie/verify-halal-certificates/","Belgedeki QR kodu / sertifika kayıt kontrolü",["halalcertification.ie"]),
        new("IFANCA","IFANCA","ABD / uluslararası","https://ifanca.org/halal-certified-products/","Sertifikalı ürün / firma kaydı",["ifanca.org","www.ifanca.org"]) });
    public static bool SafeHttps(string? value)=>Uri.TryCreate(value,UriKind.Absolute,out var u)&&u.Scheme=="https"&&u.UserInfo.Length==0&&u.Port==443;
    public static bool Matches(string issuer,string? url)=>SafeHttps(url)&&All.Any(x=>x.Id==issuer&&x.Id!="HAK"&&x.Hosts.Contains(new Uri(url!).IdnHost,StringComparer.OrdinalIgnoreCase));
    public static bool IsRecordUrl(string issuer,string? url)
    {
        if(!Matches(issuer,url))return false;
        var u=new Uri(url!);
        if(u.Fragment.Length>0)return false;
        if(u.AbsolutePath.Trim('/').Length==0)return false;
        return !All.Any(x=>string.Equals(x.Url.TrimEnd('/'),u.GetLeftPart(UriPartial.Path).TrimEnd('/'),StringComparison.OrdinalIgnoreCase) && u.Query.Length==0);
    }
}
public sealed class CertificateEvidenceInput
{
    public Guid SubmissionId { get; set; }
    public string Kind { get; set; }="product";
    public string Reference { get; set; }="";
    public string Name { get; set; }="";
    public string Issuer { get; set; }="TSE";
    public string Number { get; set; }="";
    public string Scope { get; set; }="";
    public string SourceUrl { get; set; }="";
    public string Notes { get; set; }="";
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool Consent { get; set; }
}
public static class CertificateEvidenceRules
{
    public const long MaxFileBytes=5*1024*1024;
    public static List<string> Validate(CertificateEvidenceInput x)
    {
        x.Name??="";x.Reference??="";x.Number??="";x.Scope??="";x.Notes??="";x.SourceUrl??="";
        var errors=new List<string>();
        if(x.SubmissionId==Guid.Empty)errors.Add("Gönderim kimliği eksik; formu yeniden açın.");
        if(x.Kind is not ("product" or "restaurant"))errors.Add("Ürün veya restoran türünü seçin.");
        if(x.Name.Trim().Length is <2 or >160)errors.Add("Ürün/işletme adı 2–160 karakter olmalı.");
        if(x.Reference.Length>100)errors.Add("Barkod/kayıt referansı en fazla 100 karakter olabilir.");
        if(!CertificateAuthorities.All.Any(a=>a.Id==x.Issuer&&a.Id!="HAK")&&x.Issuer!="OTHER")errors.Add("Belgeyi düzenleyen kuruluşu seçin.");
        if(x.Number.Trim().Length is <2 or >100)errors.Add("Sertifika numarası 2–100 karakter olmalı.");
        if(x.Scope.Trim().Length is <3 or >1000)errors.Add("Ürün/üretim yeri/şube kapsamını 3–1.000 karakterle yazın.");
        if(x.Notes.Length>2000)errors.Add("Not en fazla 2.000 karakter olabilir.");
        if(x.SourceUrl.Length>1000||(x.SourceUrl.Length>0&&!CertificateAuthorities.SafeHttps(x.SourceUrl)))errors.Add("Kaynak bağlantısı geçerli https adresi olmalı; kullanıcı bilgisi/özel port içermemeli.");
        if(x.ValidFrom>x.ValidUntil)errors.Add("Belge başlangıcı bitişten sonra olamaz.");
        if(!x.Consent)errors.Add("Belgeyi paylaşma ve yalnız yönetici tarafından incelenme onayını verin.");
        return errors;
    }
    public static string StatusLabel(string status)=>status switch {"pending"=>"İnceleme bekliyor","needs_info"=>"Ek bilgi gerekiyor","reviewed"=>"İnceleme tamamlandı — katalog onayı değildir","rejected"=>"Belge talebi reddedildi",_=>"Durum bilinmiyor"};
    public static string? Mime(byte[] bytes)
    {
        if(bytes.Length>=5&&bytes.AsSpan(0,5).SequenceEqual("%PDF-"u8))return "application/pdf";
        if(bytes.Length>=8&&bytes.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}))return "image/png";
        if(bytes.Length>=3&&bytes[0]==255&&bytes[1]==216&&bytes[2]==255)return "image/jpeg";
        return null;
    }
}
