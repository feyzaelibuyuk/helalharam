using BlazorApp1.Services;
var today = new DateOnly(2026,10,14);
CertificateRecord Fixture() => new() { Barcode="TEST", ProductName="Test", Issuer="DHCE", Number="TEST-ONLY", Factory="Test üretim yeri", VerificationUrl="https://halalcertification.ie/register/test-only", ScopeConfirmed=true, ValidFrom=today.AddDays(-30), ValidUntil=today.AddDays(30), CheckedOn=today };
void Check(string expected, CertificateRecord[] records, string[]? labels=null)
{
    var result=CertificateRegistry.Check("TEST","Test",labels??[],records,today);
    if(result.Status!=expected) throw new Exception($"Beklenen {expected}; gelen {result.Status}");
}
Check("not_found",[]);
Check("label_only",[],["en:halal"]);
Check("not_found",[],["en:not-halal"]);
Check("verified",[Fixture()]);
var expired=Fixture(); expired.ValidUntil=today.AddDays(-1); Check("expired",[expired]);
var stale=Fixture(); stale.CheckedOn=today.AddDays(-31); Check("recheck_required",[stale]);
var revoked=Fixture(); revoked.Revoked=true; Check("revoked",[revoked]);
var future=Fixture(); future.ValidFrom=today.AddDays(1); Check("not_yet_valid",[future]);
var wrong=Fixture(); wrong.ProductName="Başka ürün"; Check("label_only",[wrong],["en:halal"]);
var incomplete=Fixture(); incomplete.Factory=""; Check("unverified",[incomplete]);
var bad=Fixture(); bad.VerificationUrl="https://halalcertification.ie.evil.example/record"; Check("unverified",[bad]);
var issuer=Fixture(); issuer.Issuer="Başka kurum"; Check("unverified",[issuer]);
var generic=Fixture(); generic.VerificationUrl="https://halalcertification.ie/verify-halal-certificates/"; Check("unverified",[generic]);
Check("scope_uncertain",[Fixture(),Fixture()]);
Console.WriteLine("PASS: etiket/sertifika ayrımı, ürün kapsamı, resmî kaynak, geçerlilik, iptal, kontrol tazeliği ve eksik kayıt kontrolleri. Test belgeleri gerçek katalogda yok.");
